using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    internal struct NetworkMessage
    {
        public uint PacketId;
        public PacketFlag PacketFlags;
        public PacketType PacketType;

        public byte[] Payload;
    }

    [Flags]
    internal enum PacketFlag : byte
    {
        Reliable = 1,
        Ordered = 2,
        Compressed = 4,
    }

    internal enum PacketType : byte
    {
        Data,
        Ack,
        ClientConnectRequest,
        ClientDisconnectRequest,
        ClientKick,
    }
}
