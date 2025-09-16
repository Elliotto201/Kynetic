using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    internal sealed class NetworkClient
    {
        public IPEndPoint IpEndPoint { get; private set; }
        public uint ClientId { get; private set; }

        public NetworkClient(IPEndPoint ipEndPoint, uint clientId)
        {
            IpEndPoint = ipEndPoint;
            ClientId = clientId;
        }
    }
}
