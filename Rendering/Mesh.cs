using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rendering
{
    public sealed class Mesh
    {
        public Vector3[] Vertices { get; private set; }
        public uint[] Indices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[] UVs { get; private set; }

        public Mesh(Vector3[] vertices, uint[] indices, Vector3[] normals, Vector2[] uvs)
        {
            Vertices = vertices;
            Indices = indices;
            Normals = normals;
            UVs = uvs;
        }
    }
}
