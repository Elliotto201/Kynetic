using System;
using System.IO;
using System.IO.Compression;

namespace Networking
{
    internal static class NetworkMessageSerializer
    {
        public static byte[] SerializeMessage(NetworkMessage packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(packet.PacketId);
            bw.Write((byte)packet.PacketFlags);
            bw.Write((byte)packet.PacketType);

            if (!packet.PacketFlags.HasFlag(PacketFlag.Compressed))
            {
                bw.Write(packet.Payload);
            }
            else
            {
                using var compressed = new MemoryStream();
                using (var deflate = new DeflateStream(compressed, CompressionLevel.Fastest, true))
                {
                    deflate.Write(packet.Payload, 0, packet.Payload.Length);
                }
                bw.Write(compressed.ToArray());
            }

            return ms.ToArray();
        }

        public static NetworkMessage DeserializeMessage(byte[] bytes)
        {
            var packet = new NetworkMessage();

            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);

            packet.PacketId = br.ReadUInt32();
            packet.PacketFlags = (PacketFlag)br.ReadByte();
            packet.PacketType = (PacketType)br.ReadByte();

            int payloadLength = (int)(ms.Length - ms.Position);
            byte[] payloadData = br.ReadBytes(payloadLength);

            if (!packet.PacketFlags.HasFlag(PacketFlag.Compressed))
            {
                packet.Payload = payloadData;
            }
            else
            {
                using var input = new MemoryStream(payloadData);
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflate.CopyTo(output);
                packet.Payload = output.ToArray();
            }

            return packet;
        }
    }
}
