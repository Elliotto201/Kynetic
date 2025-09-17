using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RpcAttribute : Attribute
    {
        internal RpcTarget Target { get; private set; }

        public RpcAttribute(RpcTarget target)
        {
            Target = target;
        }
    }

    public enum RpcTarget
    {
        Clients,
        Server,
        Owner,
    }
}
