using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    internal interface MaterialProperty
    {
        Vector3 getValue();
        Vector3 getValue(Vector2 uv);
        void setValue(Vector3 v);
        MaterialProperty Clone();
    }
    internal class ConstMaterialProperty : MaterialProperty
    {
        private Vector3 value;
        public ConstMaterialProperty(Vector3 val) { value = val; }

        public MaterialProperty Clone()
        {
            return new ConstMaterialProperty(value);
        }

        public Vector3 getValue()
        {
            return value;
        }

        public Vector3 getValue(Vector2 uv)
        {
            return value;
        }

        public void setValue(Vector3 v)
        {
            value = v;
        }
    }
    internal class TexturedMaterialProperty : MaterialProperty
    {
        private Vector3 value;
        private Texture text;
        public TexturedMaterialProperty(Texture t, Vector3 val) 
        {
            value = val; 
            text = t; 
        }

        public MaterialProperty Clone()
        {
            return new TexturedMaterialProperty(text, value);
        }

        public Vector3 getValue()
        {
            return value;
        }

        public Vector3 getValue(Vector2 uv)
        {
            return text.GetPixel(uv.X, uv.Y);
        }

        public void setValue(Vector3 v)
        {
            value = v;
        }
    }
}
