// #region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
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
using System.Text;
using System.Collections.Generic;

using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using ALL11 = OpenTK.Graphics.ES11.All;
using ALL20 = OpenTK.Graphics.ES20.All;

using Microsoft.Xna.Framework;
using OpenTK;

namespace Microsoft.Xna.Framework.Graphics
{
	public class SpriteBatch : GraphicsResource
	{
		SpriteBatcher _batcher;
		
		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		Matrix _matrix;
		
		Rectangle tempRect = new Rectangle(0,0,0,0);
		Vector2 texCoordTL = new Vector2(0,0);
		Vector2 texCoordBR = new Vector2(0,0);
		
		//OpenGLES2 variables
		int program;
		
        public SpriteBatch ( GraphicsDevice graphicsDevice )
		{
			if (graphicsDevice == null )
			{
				throw new ArgumentException("graphicsDevice");
			}	
			
			this.graphicsDevice = graphicsDevice;
			
			_batcher = new SpriteBatcher();

		}
		

	
		
		private void SetUniformMatrix4(int location, bool transpose, ref Matrix4 matrix)
		{
			unsafe
			{
				fixed (float* matrix_ptr = &matrix.Row0.X)
				{
					GL20.UniformMatrix4(location,1,transpose,matrix_ptr);
				}
			}
		}
		
		public void Begin()
		{
			Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity );			
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState)
		{
			Begin( sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity );			
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState )
		{	
			Begin( sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity );	
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			Begin( sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity );			
		}
		

        private bool begun = false;

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{
			_sortMode = sortMode;

			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? SamplerState.LinearClamp;
			
			_matrix = transformMatrix;


            if (begun)
                throw new Exception ("batch already started");
            begun = true;
		}
		
		public void End()
		{
            if (!begun)
                throw new Exception ("ended batch not started");
            begun = false;
              
			EndGL11();
		}
		
	
		public void EndGL11()
		{					
			// Disable Blending by default = BlendState.Opaque
			GL11.Disable(ALL11.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL11.BlendFunc(ALL11.SrcAlpha, ALL11.OneMinusSrcAlpha);
				GL11.Enable(ALL11.Blend);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL11.BlendFunc(ALL11.One, ALL11.OneMinusSrcAlpha);
				GL11.Enable(ALL11.Blend);				
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL11.BlendFunc(ALL11.SrcAlpha,ALL11.One);
				GL11.Enable(ALL11.Blend);	
			}
            
            if( _blendState == BlendState.Multiply )
            {
                GL11.BlendFunc(ALL11.DstColor, ALL11.Zero);
                GL11.Enable(ALL11.Blend);
            }

            if( _blendState == BlendState.Multiplyx2 )
            {
                GL11.BlendFunc(ALL11.DstColor, ALL11.SrcColor);
                GL11.Enable(ALL11.Blend);
            }

			// set camera
			GL11.MatrixMode(ALL11.Projection);
			GL11.LoadIdentity();							
			
            GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
	
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL11.Enable(ALL11.ScissorTest);
			}
			
			
			GL11.MatrixMode(ALL11.Modelview);			
							
            GL11.Viewport(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height);
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL11.Scissor(this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height );
			}			
			
			GL11.LoadMatrix( ref _matrix.M11 );	
			
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL11.Disable(ALL11.DepthTest);
			GL11.TexEnv(ALL11.TextureEnv, ALL11.TextureEnvMode,(int) ALL11.BlendSrc);
			GL11.Enable(ALL11.Texture2D);
			GL11.EnableClientState(ALL11.VertexArray);
			GL11.EnableClientState(ALL11.ColorArray);
			GL11.EnableClientState(ALL11.TextureCoordArray);
			
			// No need to cull sprites. they will all be same-facing by construction.
            // Plus, setting frontface to Clockwise is a troll move.
            GLStateManager.Cull(CullMode.None);
			GL11.Color4(1.0f, 1.0f, 1.0f, 1.0f);						
			
			_batcher.DrawBatchGL11(_sortMode, _samplerState);
			
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable)
            {
               GL11.Disable(ALL11.ScissorTest);
            }
		}
        
        public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 Vector2 scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}

            if (!begun)
                throw new ArgumentException (" drawn in batch not begun");
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0 )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0 )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
			_batcher.AddBatchItem (
                 (int) texture.ID,
                 depth,
				 position.X,
				 position.Y,
				 -origin.X*scale.X,
				 -origin.Y*scale.Y,
				 tempRect.Width*scale.X,
				 tempRect.Height*scale.Y,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 float scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;
			}
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0)
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0)
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 depth,
				 position.X,
				 position.Y,
				 -origin.X*scale,
				 -origin.Y*scale,
				 tempRect.Width*scale,
				 tempRect.Height*scale,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
            
		}
		
		public void Draw (
         	Texture2D texture,
         	Rectangle destinationRectangle,
         	Nullable<Rectangle> sourceRectangle,
         	Color color,
         	float rotation,
         	Vector2 origin,
         	SpriteEffects effect,
         	float depth
			)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;
			}
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0)
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0)
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 depth,
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 -origin.X, 
				 -origin.Y, 
				 destinationRectangle.Width,
				 destinationRectangle.Height,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR );	
            
		}
		
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;
			}
			
			
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 0.0f,
                 position.X, position.Y, tempRect.Width, tempRect.Height, color, texCoordTL, texCoordBR );
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;
			}		
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 0.0f,
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 destinationRectangle.Width, 
				 destinationRectangle.Height, 
				 color, 
				 texCoordTL, 
				 texCoordBR );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Color color
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 0.0f,
				 position.X,
			     position.Y,
				 tempRect.Width,
				 tempRect.Height,
				 color,
				 texCoordTL,
				 texCoordBR
				 );
            
		}
		
		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;			
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
            _batcher.AddBatchItem (
                 (int) texture.ID,
                 0.0f,
				 rectangle.X,
				 rectangle.Y,
				 rectangle.Width,
				 rectangle.Height,
				 color,
				 texCoordTL,
				 texCoordBR
			    );
		}
		
		
		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = position;
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = position.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );

                _batcher.AddBatchItem (
                     (int) spriteFont._texture.ID,
                     0.0f,
					 p.X,
					 p.Y+g.Cropping.Y,
					 g.Glyph.Width,
					 g.Glyph.Height,
					 color,
					 texCoordTL,
					 texCoordBR
					 );
		                
				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
                _batcher.AddBatchItem (
                     (int) spriteFont._texture.ID,
                     depth,
					 position.X,
					 position.Y,
					 p.X*scale,
					 (p.Y+g.Cropping.Y)*scale,
					 g.Glyph.Width*scale,
					 g.Glyph.Height*scale,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{			
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
                _batcher.AddBatchItem (
                     (int) spriteFont._texture.ID,
                     depth,
					 position.X,
					 position.Y,
					 p.X*scale.X,
					 (p.Y+g.Cropping.Y)*scale.Y,
					 g.Glyph.Width*scale.X,
					 g.Glyph.Height*scale.Y,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			DrawString ( spriteFont, text.ToString(), position, color );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
	}
}

