using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Networking
{
    /// <summary>
    /// Handles taking in a message. And handling it so that messages act in the expected way. It sends a ack if message needs it and it reorders messages if needed.
    /// </summary>
    internal sealed class NetworkMessageHandler
    {
        private const int MaxResendLimit = 4;

        private readonly NetworkSender Sender;

        private readonly Queue<NetworkMessage> orderedQueue = new();
        private readonly SortedDictionary<ushort, NetworkMessage> orderedBuffer = new();
        private ushort expectedOrderedId = 0;

        private readonly Dictionary<ushort, TaskCompletionSource<bool>> awaitingAcks = new();
        private readonly object queueLock = new();
        private readonly object ackLock = new();

        public event Action OnPacketDropped;

        internal NetworkMessageHandler(NetworkSender sender)
        {
            Sender = sender;
            _ = ReceiveLoopAsync();
        }

        public async Task AddMessage(NetworkMessage message)
        {
            if (message.PacketFlags.HasFlag(PacketFlag.Reliable))
            {
                var tcs = new TaskCompletionSource<bool>();
                lock (ackLock)
                    awaitingAcks[message.PacketId] = tcs;

                _ = SendWithRetryAsync(message, tcs);

                bool acked = await tcs.Task;
                if (!acked)
                {
                    OnPacketDropped?.Invoke();
                }
            }
            else
            {
                _ = Sender.SendClientAsync(message.Payload);
            }
        }

        public async Task<NetworkMessage> ReceiveMessageAsync()
        {
            while (true)
            {
                NetworkMessage msg = default;

                lock (queueLock)
                {
                    if (orderedQueue.Count > 0)
                    {
                        msg = orderedQueue.Dequeue();
                        expectedOrderedId++;
                    }
                }

                if (msg.Payload != null)
                    return msg;

                await Task.Delay(10);
            }
        }

        private async Task SendWithRetryAsync(NetworkMessage message, TaskCompletionSource<bool> tcs)
        {
            int attempt = 0;
            int delay = 50;

            while (!tcs.Task.IsCompleted && attempt < MaxResendLimit)
            {
                await Sender.SendClientAsync(message.Payload);
                await Task.Delay(delay);
                delay = Math.Min(1000, delay * 2);
                attempt++;
            }

            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetResult(false);
                lock (ackLock) awaitingAcks.Remove(message.PacketId);
            }
        }

        private async Task ReceiveLoopAsync()
        {
            while (true)
            {
                var data = await Sender.ReceiveClientAsync();
                var message = new NetworkMessage { Payload = data };

                bool reliable = message.PacketFlags.HasFlag(PacketFlag.Reliable);
                bool ordered = message.PacketFlags.HasFlag(PacketFlag.Ordered);

                if (reliable)
                {
                    var ack = new NetworkMessage
                    {
                        PacketId = message.PacketId,
                        PacketFlags = 0,
                        PacketType = PacketType.Ack,
                        Payload = Array.Empty<byte>()
                    };
                    await Sender.SendClientAsync(ack.Payload);
                }

                if (message.PacketType == PacketType.Ack)
                {
                    lock (ackLock)
                    {
                        if (awaitingAcks.TryGetValue(message.PacketId, out var tcs))
                        {
                            tcs.TrySetResult(true);
                            awaitingAcks.Remove(message.PacketId);
                        }
                    }
                    continue;
                }

                if (ordered)
                {
                    lock (queueLock)
                    {
                        if (message.PacketId == expectedOrderedId)
                        {
                            orderedQueue.Enqueue(message);
                            expectedOrderedId++;
                            while (orderedBuffer.ContainsKey(expectedOrderedId))
                            {
                                orderedQueue.Enqueue(orderedBuffer[expectedOrderedId]);
                                orderedBuffer.Remove(expectedOrderedId);
                                expectedOrderedId++;
                            }
                        }
                        else if (message.PacketId > expectedOrderedId)
                        {
                            orderedBuffer[message.PacketId] = message;
                        }
                        // messages with PacketId < expectedOrderedId are duplicates; ignore
                    }
                }
                else
                {
                    lock (queueLock)
                        orderedQueue.Enqueue(message);
                }
            }
        }
    }
}
