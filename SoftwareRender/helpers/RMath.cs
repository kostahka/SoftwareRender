using SoftwareRender.RenderConveyor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.helpers
{
    internal class RMath
    {
        static public float Interpolate(float a, float b, float t)
        {
            return a * t + b * (1 - t);
        }
    }
}
