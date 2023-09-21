using SoftwareRender.Render.MaterialSupport;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRender.Render.ModelSupport
{
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
            OutUVVertices = new Vector4[vertices.Count];
            OutNormals = new Vector3[normals.Count];
        }

        public Vector4[] OutVertices { get; private set; }
        public Vector4[] OutUVVertices { get; private set; }
        public Vector3[] OutNormals { get; private set; }
        public List<Vector4> Vertices { get; private set; }
        public List<Vector3> TextureUVs { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public Matrix4x4 modelMatrix = Matrix4x4.Identity;
    }
}
