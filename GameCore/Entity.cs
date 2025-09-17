using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public abstract class Entity
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public virtual void Spawn() { }
        public virtual void Destroy() { }
        public virtual void Tick(int tick) { }
        public virtual void Update(float dt) { }
    }
}
