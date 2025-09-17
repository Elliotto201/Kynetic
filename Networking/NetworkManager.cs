using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public static class NetworkManager
    {
        private const int SERVER_PORT = 51241;
        private const int CLIENT_PORT = 51244;

        private static NetworkManagerState State = NetworkManagerState.Dead;
        private static NetworkSender Sender = new();

        public static event Action<byte[]> OnMessageClient;
        public static event Action<(uint clientId, byte[] data)> OnMessageServer;

        public static List<Client> NetworkClientList 
        { 
            get
            {
                if(State != NetworkManagerState.Server)
                {
                    throw new Exception($"Tried to access NetworkClients on a non server NetworkManager with state {State}");
                }

                return Sender.ConnectedClients.Select(t => new Client(t.ClientId)).ToList();
            } 
        }

        //-----Server-----
        private static Dictionary<uint, NetworkMessageHandler> ServerMessageHandlers;

        //-----Client-----
        private static NetworkMessageHandler MessageHandler;

        public static async void StartServer()
        {
            Sender = new NetworkSender();
            ServerMessageHandlers = new();

            Sender.StartServer(SERVER_PORT);
            State = NetworkManagerState.Server;

            await ReceiveSenderLoop();
            await ReceiveHandledLoop();
        }

        public static async void StartClient(IPEndPoint serverEndpoint)
        {
            Sender = new();
            MessageHandler = new(Sender);

            Sender.StartClient(CLIENT_PORT, serverEndpoint);
            State = NetworkManagerState.Client;

            await ReceiveSenderLoop();
            await ReceiveHandledLoop();
        }

        public static void Stop()
        {
            Sender.Stop();
            State = NetworkManagerState.Dead;
        }

        public static async void SendMessageClient(byte[] message)
        {
            if (State != NetworkManagerState.Client) return;

            await Sender.SendClientAsync(message);
        }

        public static async void SendMessageServer(uint clientId, byte[] message)
        {
            if (State != NetworkManagerState.Server) return;

            await Sender.SendServerAsync(clientId, message);
        }

        private static async Task ReceiveSenderLoop()
        {
            while(State != NetworkManagerState.Dead)
            {
                if(State == NetworkManagerState.Server)
                {
                    var message = await Sender.ReceiveServerAsync();

                    if (ServerMessageHandlers.TryGetValue(message.clientId, out var handler))
                    {
                        await handler.AddMessage(message.data);
                    }
                    else
                    {
                        var newHandler = new NetworkMessageHandler(Sender);
                        await newHandler.AddMessage(message.data);

                        ServerMessageHandlers.Add(message.clientId, newHandler);
                    }
                }
                else if(State == NetworkManagerState.Client)
                {
                    var message = await Sender.ReceiveClientAsync();

                    await MessageHandler.AddMessage(message);
                }
            }
        }

        private static async Task ReceiveHandledLoop()
        {
            while (State != NetworkManagerState.Dead)
            {
                if (State == NetworkManagerState.Server)
                {
                    foreach(var connectedClient in Sender.ConnectedClients)
                    {
                        if (ServerMessageHandlers.TryGetValue(connectedClient.ClientId, out var handler))
                        {
                            var message = await handler.ReceiveMessageAsync();

                            OnMessageServer?.Invoke((connectedClient.ClientId, message.Payload));
                        }
                        else
                        {
                            var _handler = new NetworkMessageHandler(Sender);
                            var _message = await _handler.ReceiveMessageAsync();

                            OnMessageServer?.Invoke((connectedClient.ClientId, _message.Payload));

                            ServerMessageHandlers.Add(connectedClient.ClientId, _handler);
                        }
                    }
                }
                else if(State == NetworkManagerState.Client)
                {
                    var message = await MessageHandler.ReceiveMessageAsync();

                    OnMessageClient?.Invoke(message.Payload);
                }
            }
        }

        private enum NetworkManagerState : byte
        {
            Dead,
            Client,
            Server,
        }
    }
}
