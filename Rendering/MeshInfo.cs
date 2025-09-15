using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendering
{
    internal struct MeshInfo
    {
        internal int vao;
        internal int vbo;
        internal int ebo;
        internal int normalVbo;
        internal int uvVbo;

        internal uint IndecesCount;

        internal MeshInfo(int vao, int vbo, int ebo, int normalVbo, int uvVbo, uint indecesCount)
        {
            this.vao = vao;
            this.vbo = vbo;
            this.ebo = ebo;
            this.normalVbo = normalVbo;
            this.uvVbo = uvVbo;
            this.IndecesCount = indecesCount;
        }
    }
}
