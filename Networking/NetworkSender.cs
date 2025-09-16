using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    internal sealed class NetworkSender
    {
        private Transport transport;

        //-----Server-----
        private Dictionary<IPEndPoint, uint> clientEndpointToClientId;
        private Dictionary<uint, IPEndPoint> clientIdToClientEndpoint;

        //-----Client-----
        private IPEndPoint ServerEndPoint;


        public void StartServer(int localPort)
        {
            transport = new Transport();
            transport.Start(localPort);

            clientEndpointToClientId = new();
            clientIdToClientEndpoint = new();
        }

        public void StartClient(int localPort, IPEndPoint serverEndPoint)
        {
            transport = new Transport();
            transport.Start(localPort);

            ServerEndPoint = serverEndPoint;
        }

        public async Task SendServerAsync(uint clientId, byte[] data)
        {
            if(clientIdToClientEndpoint.TryGetValue(clientId, out var endPoint))
            {
                await transport.SendAsync(endPoint, data);
            }
        }

        public async Task SendClientAsync(byte[] data)
        {
            await transport.SendAsync(ServerEndPoint, data);
        }

        public async Task<(uint clientId, byte[] data)> ReceiveServerAsync()
        {
            while (true)
            {
                var data = await transport.ReceiveAsync();

                if (clientEndpointToClientId.TryGetValue(data.endPoint, out uint clientId))
                {
                    return (clientId, data.data);
                }
                else
                {
                    TryAcceptClientServer(data);
                }
            }
        }

        public async Task<byte[]> ReceiveClientAsync()
        {
            while (true)
            {
                var receiveData = await transport.ReceiveAsync();
                if (IpAreEqual(ServerEndPoint, receiveData.endPoint))
                    return receiveData.data;
            }
        }

        public void TryDisconnectClientServer(uint clientId)
        {
            if (clientIdToClientEndpoint.ContainsKey(clientId))
            {
                clientIdToClientEndpoint.Remove(clientId, out var value);
                clientEndpointToClientId.Remove(value);
            }
        }

        public void Stop()
        {
            transport.Stop();
        }

        private bool IpAreEqual(IPEndPoint a, IPEndPoint b)
        {
            return a.Address.Equals(b.Address) && a.Port == b.Port;
        }

        private void TryAcceptClientServer((IPEndPoint endPoint, byte[] data) data)
        {
            var message = NetworkMessageSerializer.DeserializeMessage(data.data);

            if (message.PacketType == PacketType.ClientConnectRequest)
            {
                if (CheckAcceptForClientServer(message.Payload))
                {
                    var id = GetNewClientIdServer();

                    clientEndpointToClientId.Add(data.endPoint, id);
                    clientIdToClientEndpoint.Add(id, data.endPoint);
                }
            }
        }

        private bool CheckAcceptForClientServer(byte[] data)
        {
            //TODO: Implement actual checking ie from a db
            return true;
        }

        private uint GetNewClientIdServer()
        {
            uint id = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(4));

            while (clientIdToClientEndpoint.ContainsKey(id))
            {
                id = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(4));
            }

            return id;
        }
    }
}
