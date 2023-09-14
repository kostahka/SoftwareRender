using SoftwareRender.RenderConveyor;
using System;
using System.Numerics;

namespace SoftwareRender.Render
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public Vector3 lightPos { get; set; } = new();
        public Vector3 eyePos { get; set; } = new();
        Vector3 difColor = new(0.5f, 0.3f, 0.3f);
        Vector3 ambColor = new(0.4f, 0.2f, 0.2f);
        Vector3 specColor = new(0.7f, 0.4f, 0.4f);
        float ambient = 0.2f;
        float diffuse = 0.5f;
        float specullar = 0.3f;
        int kSpec = 8;

        public Vector3 fragment(Vector4 pos, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            Vector3 lightDir = Vector3.Normalize(lightPos - p);
            normal = Vector3.Normalize(normal);
            float dif = Vector3.Dot(lightDir, normal);
            if (dif < 0)
                dif = 0;

            Vector3 eyeDir = Vector3.Normalize(eyePos - p);
            Vector3 reflectDir = (lightDir - 2 * Vector3.Dot(lightDir, normal) * normal);

            float spec = MathF.Pow(Vector3.Dot(eyeDir, reflectDir), kSpec);

            return ambColor * ambient + diffuse * dif * difColor + spec * specullar * specColor;
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
