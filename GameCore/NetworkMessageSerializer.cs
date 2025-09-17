using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    internal static class NetworkMessageSerializer
    {
        public static byte[] SerializeMessage(NetworkMessage message)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((byte)message.Type);
            bw.Write(message.Payload);

            return ms.ToArray();
        }

        public static NetworkMessage DeserializeMessage(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            MessageType type = (MessageType)br.ReadByte();
            byte[] payload = br.ReadBytes((int)(ms.Length - ms.Position));

            return new NetworkMessage
            {
                Type = type,
                Payload = payload
            };
        }

        public static byte[] Combine(params object[] values)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            foreach (var v in values)
            {
                switch (Type.GetTypeCode(v.GetType()))
                {
                    case TypeCode.Boolean: bw.Write((bool)v); break;
                    case TypeCode.Byte: bw.Write((byte)v); break;
                    case TypeCode.SByte: bw.Write((sbyte)v); break;
                    case TypeCode.Int16: bw.Write((short)v); break;
                    case TypeCode.UInt16: bw.Write((ushort)v); break;
                    case TypeCode.Int32: bw.Write((int)v); break;
                    case TypeCode.UInt32: bw.Write((uint)v); break;
                    case TypeCode.Int64: bw.Write((long)v); break;
                    case TypeCode.UInt64: bw.Write((ulong)v); break;
                    case TypeCode.Single: bw.Write((float)v); break;
                    case TypeCode.Double: bw.Write((double)v); break;
                    case TypeCode.Char: bw.Write((char)v); break;
                    case TypeCode.String: bw.Write((string)v); break;
                    default: throw new NotSupportedException(v.GetType().FullName);
                }
            }
            return ms.ToArray();
        }

        public static object[] Decombine<T1>(byte[] data)
            => DecombineInternal(data, typeof(T1));

        public static object[] Decombine<T1, T2>(byte[] data)
            => DecombineInternal(data, typeof(T1), typeof(T2));

        public static object[] Decombine<T1, T2, T3>(byte[] data)
            => DecombineInternal(data, typeof(T1), typeof(T2), typeof(T3));

        public static object[] Decombine<T1, T2, T3, T4>(byte[] data)
            => DecombineInternal(data, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static object[] Decombine<T1, T2, T3, T4, T5>(byte[] data)
            => DecombineInternal(data, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        static object[] DecombineInternal(byte[] data, params Type[] types)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            object[] results = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Boolean: results[i] = br.ReadBoolean(); break;
                    case TypeCode.Byte: results[i] = br.ReadByte(); break;
                    case TypeCode.SByte: results[i] = br.ReadSByte(); break;
                    case TypeCode.Int16: results[i] = br.ReadInt16(); break;
                    case TypeCode.UInt16: results[i] = br.ReadUInt16(); break;
                    case TypeCode.Int32: results[i] = br.ReadInt32(); break;
                    case TypeCode.UInt32: results[i] = br.ReadUInt32(); break;
                    case TypeCode.Int64: results[i] = br.ReadInt64(); break;
                    case TypeCode.UInt64: results[i] = br.ReadUInt64(); break;
                    case TypeCode.Single: results[i] = br.ReadSingle(); break;
                    case TypeCode.Double: results[i] = br.ReadDouble(); break;
                    case TypeCode.Char: results[i] = br.ReadChar(); break;
                    case TypeCode.String: results[i] = br.ReadString(); break;
                    default: throw new NotSupportedException(t.FullName);
                }
            }
            return results;
        }
    }
}
