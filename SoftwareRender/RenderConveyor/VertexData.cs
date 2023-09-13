using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoftwareRender.RenderConveyor
{
    internal interface IVertexBuffer
    {
        Type GetVertexType();
        GCHandle GetValPtr(int index);
    }
    internal unsafe class VertexBuffer<T> : IVertexBuffer
        where T : unmanaged
    { 
        private List<T> vertexBuffer;
        private int valCount;
        int typeSize = sizeof(T);
        public VertexBuffer(List<T> values)
        {
            valCount = values.Count;
            vertexBuffer = values;
        }
        public GCHandle GetValPtr(int index)
        {
            return GCHandle.Alloc(vertexBuffer[index], GCHandleType.Pinned);
        }
        Type IVertexBuffer.GetVertexType()
        {
            return typeof(T);
        }
    }

    internal unsafe class VertexElementsBuffer<T> : IVertexBuffer
        where T : unmanaged
    {
        private List<T> vertexBuffer;
        private List<int> indexes;
        private int valCount;
        int typeSize = sizeof(T);
        public VertexElementsBuffer(List<T> values, List<int> indexes)
        {
            this.indexes = indexes;
            valCount = values.Count;
            vertexBuffer = values;
        }
        public GCHandle GetValPtr(int index)
        {
            index = indexes[index];
            return GCHandle.Alloc(vertexBuffer[index - 1], GCHandleType.Pinned);
        }

        Type IVertexBuffer.GetVertexType()
        {
            return typeof(T);
        }
    }

    internal interface IVertexArrayObject<T> 
        where T : IVertexInputInfo<T>, new()
    {
        void GetInParametrPtrs(int index, ref List<GCHandle?> parPtrs);
    }
    internal class VertexArrayObject<T> : IVertexArrayObject<T>
        where T : IVertexInputInfo<T>, new()
    {
        List<IVertexBuffer> buffers;
        public VertexArrayObject(List<IVertexBuffer> vertexBuffers)
        {
            T vertexInputInfo = new();
            var vertInTypes = vertexInputInfo.getInParametrs();
            if (vertInTypes.Count != vertexBuffers.Count)
                throw new Exception("Mismatch between count of buffers and vertex input");

            buffers = vertexBuffers;
            
            for (int i = 0; i < vertInTypes.Count; i++)
            {
                if (!vertexBuffers[i].GetVertexType().Equals(vertInTypes[i]))
                    throw new Exception("Mismatch between types of buffers and vertex input");
            }
        }

        public void GetInParametrPtrs(int index, ref List<GCHandle?> parPtrs)
        {
            for (int i = 0; i < buffers.Count; i++)
            {
                if (parPtrs[i] != null)
                {
                    parPtrs[i].Value.Free();
                }
                parPtrs[i] = buffers[i].GetValPtr(index);
                if (parPtrs[i].Value.Target == null)
                    i = 0;
            }
        }
    }
    internal interface IVertexInputInfo<T>
        where T : IVertexInputInfo<T>, new()
    {
        List<Type> getInParametrs();
    }
}
