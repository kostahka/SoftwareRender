using SoftwareRender.RenderConveyor;
using System.Numerics;

namespace SoftwareRender.Render
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;

        public Vector3 fragment(Vector4 pos, Vector3 normal)
        {
            return new(0, 0, 0);
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
