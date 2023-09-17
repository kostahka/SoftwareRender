using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    internal class Material
    {
        public MaterialProperty AmbientColor { get; private set; }
        public MaterialProperty DiffuseColor { get; private set; }
        public MaterialProperty SpecullarColor { get; private set; }
        public MaterialProperty Normal { get; private set; }
        public float specNs { get; private set; }
        public Material(MaterialProperty ambient, MaterialProperty diffuse, MaterialProperty specullar, MaterialProperty normal, float specNs = 0.0f)
        {
            AmbientColor = ambient;
            DiffuseColor = diffuse;
            SpecullarColor = specullar;
            Normal = normal;
            this.specNs = specNs;
        }
        public Material()
        {
            AmbientColor = new ConstMaterialProperty(new(0));
            DiffuseColor = new ConstMaterialProperty(new(0));
            SpecullarColor = new ConstMaterialProperty(new(0));
            Normal = new ConstMaterialProperty(new(1));
            this.specNs = 0;
        }
        public Material Clone()
        {
            return new(AmbientColor, DiffuseColor, SpecullarColor, Normal.Clone(), specNs);
        }
    }
}
