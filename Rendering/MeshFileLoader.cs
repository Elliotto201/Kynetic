using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rendering
{
    public static class MeshFileLoader
    {
        public static Mesh LoadMeshFromObjFileBytes(byte[] data)
        {
            string objData = Encoding.UTF8.GetString(data);
            string[] lines = objData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<Vector3> positions = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<uint> indices = new();

            List<Vector3> finalPositions = new();
            List<Vector3> finalNormals = new();
            List<Vector2> finalUVs = new();

            Dictionary<string, uint> vertexCache = new();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#') continue;

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "v":
                        positions.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                        break;

                    case "vn":
                        normals.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                        break;

                    case "vt":
                        uvs.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                        break;

                    case "f":
                        for (int j = 1; j < parts.Length; j++)
                        {
                            if (!vertexCache.TryGetValue(parts[j], out uint index))
                            {
                                string[] comps = parts[j].Split('/');
                                int vi = int.Parse(comps[0]) - 1;
                                int ti = comps.Length > 1 && comps[1].Length > 0 ? int.Parse(comps[1]) - 1 : -1;
                                int ni = comps.Length > 2 && comps[2].Length > 0 ? int.Parse(comps[2]) - 1 : -1;

                                finalPositions.Add(positions[vi]);
                                finalUVs.Add(ti >= 0 ? uvs[ti] : Vector2.Zero);
                                finalNormals.Add(ni >= 0 ? normals[ni] : Vector3.UnitY);

                                index = (uint)(finalPositions.Count - 1);
                                vertexCache[parts[j]] = index;
                            }
                            indices.Add(index);
                        }
                        break;
                }
            }

            Mesh mesh = new Mesh(finalPositions.ToArray(), indices.ToArray(), normals.ToArray(), uvs.ToArray());
            return mesh;
        }

        private static float ParseFloat(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

        public static Mesh LoadMeshFromFbxFileBytes(byte[] data)
        {

        }
    }
}
