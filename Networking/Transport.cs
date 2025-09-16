using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    internal sealed class Transport
    {
#pragma warning disable CS8618
        private UdpClient client;
#pragma warning restore CS8618

        public void Start(int port)
        {
            client = new UdpClient(port);
        }

        public void Stop()
        {
            client.Close();
            client.Dispose();
        }

        public async Task SendAsync(IPEndPoint endPoint, byte[] data)
        {
            CheckInit();
            
            await client.SendAsync(data, endPoint);
        }

        public async Task<(IPEndPoint endPoint, byte[] data)> ReceiveAsync()
        {
            CheckInit();

            var receiveData = await client.ReceiveAsync();

            return (receiveData.RemoteEndPoint, receiveData.Buffer);
        }

        private void CheckInit()
        {
            if(client == null)
            {
                throw new InvalidOperationException("The transport was not initialized");
            }
        }
    }
}
