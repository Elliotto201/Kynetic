using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendering
{
    internal sealed class MeshObject
    {
        public MeshInfo Mesh { get; internal set; }
        public Material Material { get; internal set; }

        internal MeshObject(MeshInfo mesh, Material material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}
