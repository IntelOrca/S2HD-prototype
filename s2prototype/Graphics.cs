using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Graphics
	{
		private SpriteBatch mSpriteBatch;
		private int mTranslationX;
		private int mTranslationY;

		public Graphics(SpriteBatch spriteBatch)
		{
			mSpriteBatch = spriteBatch;
		}

		public void DrawImage(Texture2D texture, Rectangle destination, Color colour)
		{
			mSpriteBatch.Draw(texture, GetClientDestination(destination), colour);
		}

		public void DrawImage(Texture2D texture, Vector2 destination, Color colour)
		{
			mSpriteBatch.Draw(texture, GetClientDestination(new Rectangle((int)destination.X, (int)destination.Y, texture.Width, texture.Height)), colour);
		}

		public void DrawImage(Texture2D texture, Rectangle destination, Rectangle source, Color colour)
		{
			mSpriteBatch.Draw(texture, GetClientDestination(destination), source, colour);
		}

		public void DrawImage(Texture2D texture, Rectangle destination, Rectangle source, Color colour, SpriteEffects effects)
		{
			mSpriteBatch.Draw(texture, GetClientDestination(destination), source, colour, 0.0f, new Vector2(0, 0), effects, 0.0f);
		}

		public void DrawImage(Texture2D texture, Rectangle destination, Rectangle source, Color colour, float rotation, Vector2 origin, SpriteEffects effects)
		{
			mSpriteBatch.Draw(texture, GetClientDestination(destination), source, colour, rotation, origin, effects, 0.0f);
		}

		public void DrawMarker()
		{
			DrawImage(ResourceManager.MarkerTexture, new Vector2(-30, -30), Color.White);
		}

		public void Translate(int x, int y)
		{
			mTranslationX += x;
			mTranslationY += y;
		}

		private Rectangle GetClientDestination(Rectangle destination)
		{
			destination.X += mTranslationX;
			destination.Y += mTranslationY;
			return destination;
		}

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return mSpriteBatch.GraphicsDevice;
			}
		}
	}
}
