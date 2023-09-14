using SoftwareRender.Render;
using SoftwareRender.RenderConveyor;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRender
{
    internal class Model
    {
        public struct VertexIndexes
        {
            public VertexIndexes(int v_i, int t_i = 0, int n_i = 0)
            {
                this.v_i = v_i;
                this.t_i = t_i;
                this.n_i = n_i;
            }
            public int v_i;
            public int t_i;
            public int n_i;
        }
        public Model(List<Vector4> vertices,
                    List<Vector3> texture_uv,
                    List<Vector3> normals,
                    List<int> verticesIndexes,
                    List<int> texturesIndexes,
                    List<int> normalsIndexes)
        {
            vertexCounts = verticesIndexes.Count;
            Vertices = vertices;
            TextureUVs = texture_uv;
            Normals = normals;
            VerticesIndexes = verticesIndexes;
            TextureUVIndexes = texturesIndexes;
            NormalIndexes = normalsIndexes;
            OutVertices = new Vector4[vertices.Count];
            OutNormalizedVertices = new Vector4[vertices.Count];
            OutNormals = new Vector3[normals.Count];
        }

        public int vertexCounts;
        public Vector4[] OutVertices { get; private set; }
        public Vector4[] OutNormalizedVertices { get; private set; }
        public Vector3[] OutNormals { get; private set; }
        public List<Vector4> Vertices { get; private set; }
        public List<Vector3> TextureUVs { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<int> VerticesIndexes { get; private set; }
        public List<int> TextureUVIndexes { get; private set; }
        public List<int> NormalIndexes { get; private set; }
        public Matrix4x4 modelMatrix = Matrix4x4.Identity;
    }
}
