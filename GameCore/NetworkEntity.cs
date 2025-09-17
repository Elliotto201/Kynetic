using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Networking;

namespace Core
{
    public abstract class NetworkEntity : Entity
    {
        private uint _OwnerId;

        internal bool HasOwner { get; private set; }
        internal int NetworkId { get; private set; }
        internal uint OwnerId 
        { 
            get 
            {
                if (HasOwner)
                {
                    return _OwnerId;
                }

                throw new InvalidOperationException("NetworkEntity had no owner but OwnerId was accessed on it");
            } 
        }

        protected bool IsServer { get; private set; }
        protected bool IsOwner { get; private set; }

        internal NetworkEntity()
        {

        }

        internal void Init(bool isServer, bool isOwner, int networkId, bool hasOwner, uint ownerId)
        {
            IsServer = isServer;
            IsOwner = isOwner;
            NetworkId = networkId;

            HasOwner = hasOwner;
            if (hasOwner)
            {
                _OwnerId = ownerId;
            }
        }

        public override sealed void Tick(int tick)
        {
            NetTick(tick);
        }

        public virtual void NetTick(int tick) { }

        protected void CallRpc(string methodName, params object[] paramaters)
        {

        }
    }
}
