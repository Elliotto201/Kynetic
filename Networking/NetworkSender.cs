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
        public List<NetworkClient> ConnectedClients { get; private set; }

        //-----Client-----
        private IPEndPoint ServerEndPoint;


        public void StartServer(int localPort)
        {
            transport = new Transport();
            transport.Start(localPort);

            clientEndpointToClientId = new();
            clientIdToClientEndpoint = new();
            ConnectedClients = new();
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

        public async Task<(uint clientId, NetworkMessage data)> ReceiveServerAsync()
        {
            while (true)
            {
                try
                {
                    var data = await transport.ReceiveAsync();
                    var message = NetworkMessageSerializer.DeserializeMessage(data.data);

                    if (clientEndpointToClientId.TryGetValue(data.endPoint, out uint clientId))
                    {
                        if (message.PacketType == PacketType.ClientDisconnectRequest)
                        {
                            clientEndpointToClientId.Remove(data.endPoint);
                            clientIdToClientEndpoint.Remove(clientId);

                            ConnectedClients.RemoveAll(t => CompareIpEndPoint(data.endPoint, t.IpEndPoint));
                        }
                        else
                        {
                            return (clientId, message);
                        }
                    }
                    else
                    {
                        HandleUnknownClientServer(data);
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        public async Task<NetworkMessage> ReceiveClientAsync()
        {
            while (true)
            {
                var receiveData = await transport.ReceiveAsync();
                var receiveMessage = NetworkMessageSerializer.DeserializeMessage(receiveData.data);

                if (CompareIpEndPoint(ServerEndPoint, receiveData.endPoint))
                    return receiveMessage;
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

        private bool CompareIpEndPoint(IPEndPoint a, IPEndPoint b)
        {
            return a.Address.Equals(b.Address) && a.Port == b.Port;
        }

        private void HandleUnknownClientServer((IPEndPoint endPoint, byte[] data) data)
        {
            var message = NetworkMessageSerializer.DeserializeMessage(data.data);

            if (message.PacketType == PacketType.ClientConnectRequest)
            {
                if (CheckAcceptForClientServer(message.Payload))
                {
                    var id = GetNewClientIdServer();

                    clientEndpointToClientId.Add(data.endPoint, id);
                    clientIdToClientEndpoint.Add(id, data.endPoint);
                    ConnectedClients.Add(new NetworkClient(data.endPoint, id));
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
