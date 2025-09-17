using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    internal struct NetworkMessage
    {
        public MessageType Type;
        public byte[] Payload;
    }

    internal enum MessageType : byte
    {
        Spawn,
        Destroy,
        Rpc,
        NetworkVariable
    }
}
