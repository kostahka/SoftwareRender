using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SoftwareRender.RenderConveyor
{
    internal interface IShaderProgram
    {
        public Vector4 vertexToWorld(Vector4 pos);
        public Vector4 vertexNormilized(Vector4 pos);
        public Vector3 normal(Vector3 pos);
        public Vector3 fragment(Vector4 pos, Vector3 normal);
    }
}
