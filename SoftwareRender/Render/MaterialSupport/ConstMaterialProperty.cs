using System.Numerics;

namespace SoftwareRender.Render.MaterialSupport
{
    internal struct ConstMaterialProperty : MaterialProperty
    {
        private Vector3 value;
        public ConstMaterialProperty(Vector3 val) { value = val; }
        public Vector3 getValue()
        {
            return value;
        }

        public Vector3 getValue(Vector2 uv)
        {
            return value;
        }

        public Vector3 getValue(Vector3 uv)
        {
            return value;
        }
    }
}
