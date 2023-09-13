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
                    Dictionary<PrimitiveType, List<int>> verticesIndexes,
                    Dictionary<PrimitiveType, List<int>> texturesIndexes,
                    Dictionary<PrimitiveType, List<int>> normalsIndexes)
        {
            foreach (var primitive in verticesIndexes.Keys)
            {
                // Generate vertex buffers
                VertexElementsBuffer<Vector4> verticesBuffer = new(vertices, verticesIndexes[primitive]);

                // Sum buffers
                List<IVertexBuffer> vertexBuffers = new();
                vertexBuffers.Add(verticesBuffer);

                // Generate VAO
                vaos[primitive] = new VertexArrayObject<ModelVertexInput>(vertexBuffers);
                vertexCounts[primitive] = verticesIndexes[primitive].Count;
            }
        }

        public Dictionary<PrimitiveType, int> vertexCounts = new();
        public Dictionary<PrimitiveType, VertexArrayObject<ModelVertexInput>> vaos = new();
        public Matrix4x4 modelMatrix = Matrix4x4.Identity;
    }
}
