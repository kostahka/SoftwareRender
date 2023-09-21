using System.Numerics;

namespace SoftwareRender.Render
{
    internal struct NormalMap
    {
        Texture normalMap;
        public NormalMap(Texture normalMap)
        {
            this.normalMap = normalMap;
        }
        public Vector3 GetNormal(Vector3 uv)
        {
            var n = Vector3.Normalize(normalMap.GetPixel(uv.X, uv.Y) * 2 - new Vector3(1));
            return n;
        }
    }
}
