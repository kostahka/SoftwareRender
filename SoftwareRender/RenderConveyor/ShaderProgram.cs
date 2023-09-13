using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SoftwareRender.RenderConveyor
{
    internal interface IShaderProgram
    {
    }

    internal abstract class ShaderProgram<T> : IShaderProgram
        where T : IFragmentData<T>
    {
        public ShaderProgram() { }
        abstract public T vertex(ref List<GCHandle?> dataPtrs);
        abstract public Vector4 fragment(T data);
    }
}
