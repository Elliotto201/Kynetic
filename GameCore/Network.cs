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
        private static Dictionary<int, NetworkEntity> spawnedEntities = new();

        public static void StartServer()
        {
            NetworkManager.StartServer();

            State = NetworkState.Server;
            UsedNetworkEntityIds = new();
            NetworkManager.OnMessageServer += OnMessageServer;
        }

        public static void StartClient(IPEndPoint server)
        {
            NetworkManager.StartClient(server);

            State = NetworkState.Client;
            NetworkManager.OnMessageClient += OnMessageClient;
        }

        private static void OnMessageClient(byte[] data)
        {
            try
            {
                var message = NetworkMessageSerializer.DeserializeMessage(data);

                if (message.Type == MessageType.Spawn)
                {
                    var entityId = BitConverter.ToInt32(message.Payload[^4..]);
                    var entityType = Type.GetType(Encoding.UTF8.GetString(message.Payload[..^4]));

                    var entity = (NetworkEntity)Activator.CreateInstance(entityType);
                    entity.Init(false, false, entityId, false, uint.MinValue);

                    spawnedEntities.Add(entityId, entity);
                }
                else if (message.Type == MessageType.Destroy)
                {
                    var entityId = BitConverter.ToInt32(message.Payload);

                    if (!spawnedEntities.Remove(entityId))
                    {
                        Console.WriteLine("Client tried to remove entity that was not spawned yet");
                    }
                }
                else if (message.Type == MessageType.Rpc)
                {
                    var rpcData = NetworkMessageSerializer.Decombine<int, string>(message.Payload);
                    var entityId = (int)rpcData[0];
                    var rpcName = (string)rpcData[1];

                    if (spawnedEntities.TryGetValue(entityId, out var entity))
                    {
                        var method = entity.GetType().GetMethod(rpcName);

                        method.Invoke(entity, null);
                    }
                    else
                    {
                        Console.WriteLine("Client tried to execute a rpc on a network entity that does not exist or is not in the spawnedEntities collection");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in OnMessageClient in Network.cs: \n {ex}");
            }
        }

        private static void OnMessageServer((uint clientId, byte[] data) messageData)
        {
            try
            {
                uint clientId = messageData.clientId;
                var message = NetworkMessageSerializer.DeserializeMessage(messageData.data);

                if(message.Type == MessageType.Rpc)
                {
                    var rpcData = NetworkMessageSerializer.Decombine<int, string>(message.Payload);
                    var entityId = (int)rpcData[0];
                    var rpcName = (string)rpcData[1];

                    if(spawnedEntities.TryGetValue(entityId, out var entity))
                    {
                        var method = entity.GetType().GetMethod(rpcName);

                        if(method != null)
                        {
                            var attribute = method.GetCustomAttribute<RpcAttribute>();

                            if (attribute != null && attribute.Target == RpcTarget.Server)
                            {
                                method.Invoke(entity, null);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Server tried to execute a rpc from a client that its correspodning entity did not exist \n Client is most likeley cheating!");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in OnMessageServer in Network.cs: \n {ex}");
            }
        }

        public static void Stop()
        {
            NetworkManager.Stop();
            UsedNetworkEntityIds?.Clear();
            spawnedEntities?.Clear();

            State = NetworkState.Dead;
        }

        public static T SpawnNetworkEntity<T>() where T : NetworkEntity, new()
        {
            if (State != NetworkState.Server) throw new InvalidOperationException("Tried to spawn entity on client");

            string entityTypeName = typeof(T).AssemblyQualifiedName;

            byte[] entityPayload = Encoding.UTF8.GetBytes(entityTypeName);

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
            spawnedEntities.Add(entityId, entity);

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

            spawnedEntities.Remove(entityId);
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
