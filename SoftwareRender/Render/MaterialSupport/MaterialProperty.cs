using System.Numerics;

namespace SoftwareRender.Render.MaterialSupport
{
    internal interface MaterialProperty
    {
        Vector3 getValue();
        Vector3 getValue(Vector2 uv);
        Vector3 getValue(Vector3 uv);
    }

    
}
