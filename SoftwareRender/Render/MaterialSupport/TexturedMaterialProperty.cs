using System.Numerics;

namespace SoftwareRender.Render.MaterialSupport
{
    internal struct TexturedMaterialProperty : MaterialProperty
    {
        private Vector3 value;
        private Texture text;
        public TexturedMaterialProperty(Texture t, Vector3 val)
        {
            value = val;
            text = t;
        }
        public Vector3 getValue()
        {
            return value;
        }

        public Vector3 getValue(Vector2 uv)
        {
            return text.GetPixel(uv.X, uv.Y);
        }

        public Vector3 getValue(Vector3 uv)
        {
            return text.GetPixel(uv.X, uv.Y);
        }
    }
}
