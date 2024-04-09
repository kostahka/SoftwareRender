using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    internal class RenderUtils
    {
        public static Vector3 Saturate(Vector3 v)
        {
            return Vector3.Clamp(v, Vector3.Zero, Vector3.One);
        }
        public static Vector3 Pow(Vector3 v, float p)
        {
            return new Vector3(MathF.Pow(v.X, p), MathF.Pow(v.Y, p), MathF.Pow(v.Z, p));
        }
    }
}
