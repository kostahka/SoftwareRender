using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.RenderConveyor
{
    internal interface IFragmentData<T>
        where T : IFragmentData<T>
    {
        Vector4 GetVertexPos();
        T Interpolate(T other, float t);
    }
}
