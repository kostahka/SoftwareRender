using SoftwareRender.RenderConveyor;
using System.Numerics;

namespace SoftwareRender.Render
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public Vector4 fragment()
        {
            return new(1, 1, 1, 1);
        }

        public Vector3 normal(Vector3 norm)
        {
            return norm;
        }
        public Vector4 vertex(Vector4 pos)
        {
            Matrix4x4 mvpMatrix = model * view * proj;
            Vector4 endPos = Vector4.Transform(pos, mvpMatrix);

            return endPos;
        }
    }
}
