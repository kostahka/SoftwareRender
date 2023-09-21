using System.Numerics;

namespace SoftwareRender.Render.MaterialSupport
{
    internal struct Material
    {
        public MaterialProperty AmbientColor { get; private set; }
        public MaterialProperty DiffuseColor { get; private set; }
        public MaterialProperty SpecullarColor { get; private set; }
        public float specNs { get; private set; }
        public NormalMap? NormalText { get; set; }
        public Vector3 normal = new(1);
        public Material(MaterialProperty ambient, MaterialProperty diffuse, MaterialProperty specullar, NormalMap? normalMap, float specNs = 0.0f)
        {
            AmbientColor = ambient;
            DiffuseColor = diffuse;
            SpecullarColor = specullar;
            NormalText = normalMap;
            this.specNs = specNs;
        }
        public Material()
        {
            AmbientColor = new ConstMaterialProperty(new(0.2f));
            DiffuseColor = new ConstMaterialProperty(new(0.5f));
            SpecullarColor = new ConstMaterialProperty(new(0.8f));
            NormalText = null;
            specNs = 4;
        }
    }
}
