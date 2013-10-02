// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using ALL11 = OpenTK.Graphics.ES11.All;
using ALL20 = OpenTK.Graphics.ES20.All;

using Microsoft.Xna.Framework;
using System.Text; // just for StringBuilder

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatcher
	{
		private const int InitialBatchSize = 256;
		private const int InitialVertexArraySize = 256;
		BetterList<SpriteBatchItem> _batchItemList; // use our own list just so that we can get access to internals.
		public BetterList<VertexPosition2ColorTexture> _vertexArray; // carefull with this.
		ushort[] _index;
		GCHandle _vertexHandle;
		GCHandle _indexHandle;
		
		// OpenGL ES2.0 Variables
		public int attributePosition = 0;
		public int attributeTexCoord = 1;
		public int attributeTint = 2;
		
		public SpriteBatcher ()
		{
			_batchItemList = new BetterList<SpriteBatchItem>();
            _vertexArray = new BetterList<VertexPosition2ColorTexture>();
            _vertexArray.resizeCallback = UnlinkVertexArray;// frees our Pin every time the allocated size changes. (we re-link when necessary on end)

			_index = new ushort[6*InitialVertexArraySize];
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < InitialVertexArraySize; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
			
		}
		
		private SpriteBatchItem CreateBatchItem(Color Tint, int texId, float Depth)
		{
            // allocate 4 vertices for this quad, and set the batch pointer to the right offset.
			var bi = new SpriteBatchItem() { Tint = Tint, TextureID = texId, Depth = Depth, VertexBase = _vertexArray.Count };
            _vertexArray.AddN(4); 
            
            return bi;
		}
        
        public void AddBatchItem ( int texId, float depth, float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR )
        {
            var item = CreateBatchItem(color, texId, depth);
            
            var b = item.VertexBase;
            _vertexArray.buffer[b].Position.X = x;
            _vertexArray.buffer[b].Position.Y = y;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordTL.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordTL.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x+w;
            _vertexArray.buffer[b].Position.Y = y;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordBR.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordTL.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x;
            _vertexArray.buffer[b].Position.Y = y+h;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordTL.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordBR.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x+w;
            _vertexArray.buffer[b].Position.Y = y+h;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordBR.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordBR.Y;
            
            _batchItemList.Add (item);
        }
        
        public void AddBatchItem ( int texId, float depth, float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR )
        {
            var item = CreateBatchItem(color, texId, depth);
            var b = item.VertexBase;
            
            _vertexArray.buffer[b].Position.X = x+dx*cos-dy*sin;
            _vertexArray.buffer[b].Position.Y = y+dx*sin+dy*cos;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordTL.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordTL.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x+(dx+w)*cos-dy*sin;
            _vertexArray.buffer[b].Position.Y = y+(dx+w)*sin+dy*cos;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordBR.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordTL.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x+dx*cos-(dy+h)*sin;
            _vertexArray.buffer[b].Position.Y = y+dx*sin+(dy+h)*cos;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordTL.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordBR.Y;
            
            b++;
            _vertexArray.buffer[b].Position.X = x+(dx+w)*cos-(dy+h)*sin;
            _vertexArray.buffer[b].Position.Y = y+(dx+w)*sin+(dy+h)*cos;
            _vertexArray.buffer[b].Color = color.GLPackedValue;
            _vertexArray.buffer[b].TextureCoordinate.X = texCoordBR.X;
            _vertexArray.buffer[b].TextureCoordinate.Y = texCoordBR.Y;
            
            _batchItemList.Add (item);
        }        
            
		
		int CompareTexture ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.TextureID.CompareTo(b.TextureID);
		}
		
		int CompareDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.Depth.CompareTo(b.Depth);
		}
		
		int CompareReverseDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return b.Depth.CompareTo(a.Depth);
		}
		
		public void DrawBatchGL20 ( SpriteSortMode sortMode )
		{
			// nothing to do
			if ( _batchItemList.Count == 0 )
				return;
			
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :
				//_batchItemList.Sort( CompareTexture );
                throw new NotSupportedException();
			case SpriteSortMode.FrontToBack :
				//_batchItemList.Sort ( CompareDepth );
                throw new NotSupportedException();
			case SpriteSortMode.BackToFront :
				//_batchItemList.Sort ( CompareReverseDepth );
                throw new NotSupportedException();
			}
			
			GL20.EnableVertexAttribArray(attributePosition);
			GL20.EnableVertexAttribArray(attributeTexCoord);
			
            // make sure the vertexArray has enough space
            if ( _batchItemList.Count*4 > _index.Length )
                ExpandIndexArray( _batchItemList.Count );
            
            LinkVertexArray();

            
            int size = VertexPosition2ColorTexture.GetSize();
			GL20.VertexAttribPointer(attributePosition,2,ALL20.Float,false,size,_vertexHandle.AddrOfPinnedObject());
			GL20.VertexAttribPointer(attributeTexCoord,2,ALL20.Float,false,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))));

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			int texID = -1;
			Color lastTint =  new Color(0.0f,0.0f,0.0f,0.0f);

            
            
			foreach ( SpriteBatchItem item in _batchItemList )
			{
				//Tint Color
				Vector4 vtint = item.Tint.ToVector4();
				//vtint /= 255;
				//GL20.VertexAttrib4(attributeTint, vtint.X, vtint.Y, vtint.Z, vtint.W);

				// if the texture changed, we need to flush and bind the new texture
				if ( item.TextureID != texID || item.Tint != lastTint)
				{
					FlushVertexArrayGL20( startIndex, index );
					startIndex = index;
					texID = item.TextureID;
					lastTint = item.Tint;
					GL20.ActiveTexture(ALL20.Texture0);
					GL20.BindTexture ( ALL20.Texture2D, texID );
					GL20.Uniform1(texID, 0);
					GL20.VertexAttrib4(attributeTint,vtint.X, vtint.Y, vtint.Z, vtint.W);
				}
                index += 4;
			}
			// flush the remaining vertexArray data
			FlushVertexArrayGL20(startIndex, index);
			
			_batchItemList.Clear();
            _vertexArray.Clear ();
		}
		
		public void DrawBatchGL11 ( SpriteSortMode sortMode, SamplerState samplerState )
		{
			// nothing to do
			if ( _batchItemList.Count == 0 )
				return;
			
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :
                throw new NotSupportedException();
				//_batchItemList.Sort( CompareTexture );
			case SpriteSortMode.FrontToBack :
                throw new NotSupportedException();
				//_batchItemList.Sort ( CompareDepth );
			case SpriteSortMode.BackToFront :
                throw new NotSupportedException();
				//_batchItemList.Sort ( CompareReverseDepth );
			}
			
			// make sure an old draw isn't still going on.
			// cross fingers, commenting this out!!
			//GL.Flush();

            // make sure the vertexArray has enough space
            if ( _batchItemList.Count*6 > _index.Length )
                ExpandIndexArray( _batchItemList.Count );
            
            LinkVertexArray();
            
			int size = sizeof(float)*4+sizeof(uint);
			GL11.VertexPointer(2,ALL11.Float,size,_vertexHandle.AddrOfPinnedObject() );
			GL11.ColorPointer(4, ALL11.UnsignedByte,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2)));
			GL11.TexCoordPointer(2, ALL11.Float,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))) );

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			int texID = -1;

			
			foreach ( SpriteBatchItem item in _batchItemList )
			{
				// if the texture changed, we need to flush and bind the new texture
				if ( item.TextureID != texID )
				{
					FlushVertexArrayGL11( startIndex, index );
					startIndex = index;
					texID = item.TextureID;
					GL11.BindTexture ( ALL11.Texture2D, texID );
					
					samplerState.Activate();
				}
				// store the SpriteBatchItem data in our vertexArray
				index += 4;
			}
			// flush the remaining vertexArray data
			FlushVertexArrayGL11(startIndex, index);
			
			_batchItemList.Clear();
            _vertexArray.Clear();
		}
		
		void ExpandIndexArray( int batchSize )
		{
			// increase the size of the vertexArray
			int newCount = _index.Length / 6;
			
			while ( batchSize > newCount )
				newCount += 128;
			
            if(_indexHandle.IsAllocated)
            {
			    _indexHandle.Free();			
            }
			    
			_index = new ushort[6*newCount];
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < newCount; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
		}
        
        void LinkVertexArray()
        {
            if(!_vertexHandle.IsAllocated)
            {
                _vertexHandle = GCHandle.Alloc(_vertexArray.buffer,GCHandleType.Pinned);
            }
        }
        
        void UnlinkVertexArray()
        {
            if(_vertexHandle.IsAllocated)
            {
                _vertexHandle.Free();
            }
        }
        
		void FlushVertexArrayGL11 ( int start, int end )
		{
			// draw stuff
			if ( start != end )
				GL11.DrawElements ( ALL11.Triangles, (end-start)/2*3, ALL11.UnsignedShort,(IntPtr)((uint)_indexHandle.AddrOfPinnedObject()+(uint)(start/2*3*sizeof(short))) );
		}
		
		void FlushVertexArrayGL20 ( int start, int end )
		{
			// draw stuff
			if ( start != end )
				GL20.DrawElements ( ALL20.Triangles, (end-start)/2*3, ALL20.UnsignedShort,(IntPtr)((uint)_indexHandle.AddrOfPinnedObject()+(uint)(start/2*3*sizeof(short))) );
		}
	}
}

