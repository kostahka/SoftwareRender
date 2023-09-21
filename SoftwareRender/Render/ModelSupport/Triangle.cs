using SoftwareRender.Render.MaterialSupport;

namespace SoftwareRender.Render.ModelSupport
{
    internal struct Triangle
    {
        public VertexIndexes[] vertexIndexes;
        public Material primitiveMaterial;
        public Triangle(Material material, VertexIndexes[] vertexIndexes)
        {
            primitiveMaterial = material;
            this.vertexIndexes = vertexIndexes;
        }
    }
}
