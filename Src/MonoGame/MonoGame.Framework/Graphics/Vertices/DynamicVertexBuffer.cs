// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicVertexBuffer : VertexBuffer
    {
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;
        public int SizeInBytes;
        public EventHandler ContentLost;

        public bool IsContentLost { get { return false; } }

        public DynamicVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
            : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, true)
        {
        }
		
		public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage)
            : base(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, true)
        {
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride, options);
        }
    }
}

