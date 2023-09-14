using SoftwareRender.RenderConveyor;
using System.Numerics;

namespace SoftwareRender.Render
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public Vector3 lightPos { get; set; } = new();
        Vector3 difColor = new(0.5f, 0.3f, 0.3f);
        Vector3 ambColor = new(0.4f, 0.2f, 0.2f);
        float ambient = 0.2f;
        float diffuse = 0.5f;

        public Vector3 fragment(Vector4 pos, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            Vector3 lightDir = Vector3.Normalize(lightPos - p);
            normal = Vector3.Normalize(normal);
            float dif = Vector3.Dot(lightDir, normal);
            if (dif < 0)
                dif = 0;

            return ambColor * ambient + diffuse * dif * difColor;
        }

        public Vector3 normal(Vector3 norm)
        {
            return norm;
        }
        public Vector4 vertexNormilized(Vector4 pos)
        {
            return Vector4.Transform(pos, model);
        }

        public Vector4 vertexToWorld(Vector4 pos)
        {
            return Vector4.Transform(pos, view * proj);
        }
    }
}
