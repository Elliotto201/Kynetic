using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering
{
    public static class MeshObjectHandler
    {
        private static HashSet<int> usedMeshIds = new();
        private static Dictionary<MeshObjectHandle, int> meshHandleToIndexLookup = new();
        private static List<MeshObject> sortedMeshObjects = new();

        public static MeshObjectHandle CreateMeshObject(Window window, Mesh mesh, Material material)
        {
            var meshInfo = CreateMeshInfo(mesh, window.GetGL());
            var meshObject = new MeshObject(meshInfo, material);

            int id = Random.Shared.Next(-1000000, 1000000);
            while (usedMeshIds.Contains(id))
            {
                id = Random.Shared.Next(-1000000, 1000000);
            }

            var meshObjectHandle = new MeshObjectHandle { Id = id };

            sortedMeshObjects.Add(meshObject);
            meshHandleToIndexLookup.Add(meshObjectHandle, sortedMeshObjects.Count - 1);

            return meshObjectHandle;
        }

        public static void DeleteMeshObject(MeshObjectHandle handle)
        {
            sortedMeshObjects.RemoveAt(meshHandleToIndexLookup[handle]);
            meshHandleToIndexLookup.Remove(handle);
        }

        internal static List<MeshObject> GetMeshObjects()
        {
            return sortedMeshObjects;
        }

        private static MeshInfo CreateMeshInfo(Mesh mesh, GL gl)
        {
            int vao = (int)gl.GenVertexArray();
            gl.BindVertexArray((uint)vao);

            int vbo = (int)gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<Vector3>(mesh.Vertices), BufferUsageARB.StaticDraw);

            int normalVbo = (int)gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)normalVbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<Vector3>(mesh.Normals), BufferUsageARB.StaticDraw);

            int uvVbo = (int)gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)uvVbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<Vector2>(mesh.UVs), BufferUsageARB.StaticDraw);

            int ebo = (int)gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, (uint)ebo);
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(mesh.Indices), BufferUsageARB.StaticDraw);

            return new MeshInfo(vao, vbo, ebo, normalVbo, uvVbo, (uint)mesh.Indices.Length);
        }
    }
}
