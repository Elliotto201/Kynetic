using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Network
    {
        private static NetworkState State = NetworkState.Dead;

        private static HashSet<int> UsedNetworkEntityIds;

        public static void StartServer()
        {
            NetworkManager.StartServer();

            State = NetworkState.Server;
            UsedNetworkEntityIds = new();
        }

        public static void StartClient(IPEndPoint server)
        {
            NetworkManager.StartClient(server);

            State = NetworkState.Client;
        }

        public static void Stop()
        {
            NetworkManager.Stop();

            State = NetworkState.Dead;
        }

        public static T SpawnNetworkEntity<T>() where T : NetworkEntity, new()
        {
            if (State != NetworkState.Server) throw new InvalidOperationException("Tried to spawn entity on client");

            string entityTypeName = typeof(T).FullName;
            byte[] entityPayload = Encoding.ASCII.GetBytes(entityTypeName);

            int entityId = Random.Shared.Next(int.MinValue, int.MaxValue);
            while (UsedNetworkEntityIds.Contains(entityId))
            {
                entityId = Random.Shared.Next(int.MinValue, int.MaxValue);
            }
            UsedNetworkEntityIds.Add(entityId);

            var finalPayload = entityPayload.Concat(BitConverter.GetBytes(entityId)).ToArray();

            var message = new NetworkMessage
            {
                Type = MessageType.Spawn,
                Payload = finalPayload,
            };

            var messageBytes = NetworkMessageSerializer.SerializeMessage(message);

            foreach(var client in NetworkManager.NetworkClientList)
            {
                NetworkManager.SendMessageServer(client.ClientId, messageBytes);
            }

            var entity = new T();
            entity.Init(true, false, entityId, false, uint.MinValue);

            return entity;
        }

        public static void DestroyNetworkEntity(NetworkEntity entity)
        {
            if (State != NetworkState.Server) throw new InvalidOperationException("Tried to destroy entity on client");

            int entityId = entity.NetworkId;

            byte[] entityDestroyPayload = BitConverter.GetBytes(entityId);

            var message = new NetworkMessage
            {
                Type = MessageType.Destroy,
                Payload = entityDestroyPayload,
            };

            var messageBytes = NetworkMessageSerializer.SerializeMessage(message);

            foreach(var client in NetworkManager.NetworkClientList)
            {
                NetworkManager.SendMessageServer(client.ClientId, messageBytes);
            }

            //TODO: Destroy the entity locally
        }

        internal static void CallRpc(NetworkEntity entity, string rpcName)
        {
            int entityId = entity.NetworkId;
            var rpcAttribute = entity.GetType().GetMethod(rpcName)?.GetCustomAttribute<RpcAttribute>();

            if(rpcAttribute == null)
            {
                throw new InvalidOperationException("Method was not a rpc");
            }
            if(rpcAttribute.Target != RpcTarget.Server && State != NetworkState.Server)
            {
                throw new InvalidOperationException("Tried to call non server rpc on a client or dead connection");
            }
            var rpcTarget = rpcAttribute.Target;

            var payload = NetworkMessageSerializer.Combine(entityId, rpcName);
            var message = new NetworkMessage
            {
                Type = MessageType.Rpc,
                Payload = payload,
            };
            var messageBytes = NetworkMessageSerializer.SerializeMessage(message);

            if(rpcTarget == RpcTarget.Server)
            {
                NetworkManager.SendMessageClient(messageBytes);
            }
            else if(rpcTarget == RpcTarget.Clients)
            {
                foreach(var client in NetworkManager.NetworkClientList)
                {
                    NetworkManager.SendMessageServer(client.ClientId, messageBytes);
                }
            }
            else if(entity.HasOwner)
            {
                NetworkManager.SendMessageServer(entity.OwnerId, messageBytes);
            }
            else
            {
                throw new Exception("Entity had no owner but a owner rpc was called on it");
            }
        }

        private enum NetworkState : byte
        {
            Dead,
            Server,
            Client,
        }
    }
}
