using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SoftwareRender.RenderConveyor
{
    internal interface IShaderProgram
    {
        public Vector4 vertex(Vector4 pos);
        public Vector3 normal(Vector3 pos);
        public Vector4 fragment();
    }
}
