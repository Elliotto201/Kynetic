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
        private const int CLIENT_PORT = 51242;

        private static NetworkManagerState State = NetworkManagerState.Dead;
        private static NetworkSender Sender = new();


        public static void StartServer()
        {
            Sender = new NetworkSender();

            Sender.StartServer(SERVER_PORT);
            State = NetworkManagerState.Server;
        }

        public static void StartClient(IPEndPoint serverEndpoint)
        {
            Sender.StartClient(CLIENT_PORT, serverEndpoint);
            State = NetworkManagerState.Client;
        }

        public static async void SendMessage(byte[] message)
        {
            if (State != NetworkManagerState.Client) return;

            await Sender.SendClientAsync(message);
        }

        public static async void SendMessage(uint clientId, byte[] message)
        {
            if (State != NetworkManagerState.Server) return;

            await Sender.SendServerAsync(clientId, message);
        }

        private enum NetworkManagerState : byte
        {
            Dead,
            Client,
            Server,
        }
    }
}
