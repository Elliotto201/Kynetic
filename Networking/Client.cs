using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public sealed class Client
    {
        public uint ClientId { get; private set; }

        public Client(uint clientId)
        {
            ClientId = clientId;
        }
    }
}
