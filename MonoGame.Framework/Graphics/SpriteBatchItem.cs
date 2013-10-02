using System;

namespace Microsoft.Xna.Framework.Graphics
{
	internal struct SpriteBatchItem
	{
        public Color Tint;
		public int TextureID;
		public float Depth;
        public int VertexBase; // set by the spritebatcher.
		
		// Tint Color
		
		/*public SpriteBatchItem ()
		{
			vertexTL = new VertexPosition2ColorTexture();
            vertexTR = new VertexPosition2ColorTexture();
            vertexBL = new VertexPosition2ColorTexture();
            vertexBR = new VertexPosition2ColorTexture();            
		}*/
		

	}
}

