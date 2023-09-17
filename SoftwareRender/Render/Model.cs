using SoftwareRender.Render;
using SoftwareRender.RenderConveyor;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRender
{
    internal struct VertexIndexes
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
    internal struct Triangle
    {
        public VertexIndexes[] vertexIndexes;
        public Material primitiveMaterial;
        public Triangle(Material material, VertexIndexes[] vertexIndexes) 
        {
            primitiveMaterial = material;
            this.vertexIndexes = vertexIndexes;
        }
    }
    internal class Model
    {
        public Model(List<Vector4> vertices,
                    List<Vector3> texture_uv,
                    List<Vector3> normals,
                    List<Triangle> triangles)
        {
            Vertices = vertices;
            TextureUVs = texture_uv;
            Normals = normals;
            Triangles = triangles;
            OutVertices = new Vector4[vertices.Count];
            OutNormalizedVertices = new Vector4[vertices.Count];
            OutNormals = new Vector3[normals.Count];
        }

        public Vector4[] OutVertices { get; private set; }
        public Vector4[] OutNormalizedVertices { get; private set; }
        public Vector3[] OutNormals { get; private set; }
        public List<Vector4> Vertices { get; private set; }
        public List<Vector3> TextureUVs { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public Matrix4x4 modelMatrix = Matrix4x4.Identity;
    }
}
