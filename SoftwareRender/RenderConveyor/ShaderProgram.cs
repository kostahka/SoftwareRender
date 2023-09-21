using SoftwareRender.Render.MaterialSupport;
using System.Numerics;

namespace SoftwareRender.RenderConveyor
{
    internal interface IShaderProgram
    {
        public Vector4 vertexToWorld(Vector4 pos);
        public Vector4 vertexNormilized(Vector4 pos);
        public Vector3 normal(Vector3 pos);
        public Vector3 fragmentP(Material material, Vector4 pos);
        public Vector3 fragmentPT(Material material, Vector4 pos, Vector3 textUV);
        public Vector3 fragmentPN(Material material, Vector4 pos, Vector3 normal);
        public Vector3 fragmentPTN(Material material, Vector4 pos, Vector3 textUV, Vector3 normal);
    }
}
