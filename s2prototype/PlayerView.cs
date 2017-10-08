using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class PlayerView
	{
		private SonicGame mGame;
		private Level mLevel;
		private Player mPlayer;
		private Camera mCamera;
		private TitleCard mTitleCard;
		private Rectangle mBounds;

		private int mUpdateCount;

		private Texture2D mCollisionTexture;

		public PlayerView(SonicGame game, Player player)
		{
			mGame = game;
			mPlayer = player;
			mCamera = mPlayer.MainCharacter.Camera;
			mLevel = mGame.Level;
			mTitleCard = new TitleCard(mGame, mLevel);
		}

		public void Update()
		{
			if (mTitleCard != null) {
				mTitleCard.Update();
				if (mTitleCard.AllowStart)
					mPlayer.Status = PlayerStatus.Playing;
			}

			PlaySoundsInView();

			mUpdateCount++;
		}

		private void PlaySoundsInView()
		{
			// Get camera view
			Rectangle view = mCamera.GetViewBounds(mLevel.VisibleBoundary, mBounds.Width / mGame.DisplayScale, mBounds.Height / mGame.DisplayScale);

			foreach (Level.Sound sound in mLevel.Sounds)
				if (view.Contains(sound.DisplacementX, sound.DisplacementY))
					sound.SoundEffect.Play();
		}

		public void Draw(Graphics g)
		{
			g.DrawImage(ResourceManager.PlainTexture, mBounds, Color.Blue);

			// Get camera view
			Rectangle view = mCamera.GetViewBounds(mLevel.VisibleBoundary, mBounds.Width / mGame.DisplayScale, mBounds.Height / mGame.DisplayScale);
			DrawChunks(g, view, false);
			// DrawCollision(g, view);
			mLevel.Objects.Draw(g, view, 0, 999);
			DrawChunks(g, view, true);
			mLevel.Objects.Draw(g, view, 1000, 4999);
			DrawHUD(g);

			if (!mTitleCard.IsFinished)
				mTitleCard.Draw(g, mBounds);
		}

		private void DrawChunks(Graphics g, Rectangle view, bool front)
		{
			int leftChunk = view.X / 128;
			int topChunk = view.Y / 128;
			int leftOffset = view.X % 128;
			int topOffset = view.Y % 128;
			int chunksWide = (view.Width / 128) + 2;
			int chunksHigh = (view.Height / 128) + 2;

			for (int y = 0; y < chunksHigh; y++) {
				for (int x = 0; x < chunksWide; x++) {
					if (leftChunk + x >= mLevel.Width || topChunk + y >= 8 || leftChunk + x < 0 || topChunk + y < 0)
						continue;

					int chunkId = mLevel.LevelLayout[leftChunk + x, topChunk + y];
					Texture2D tex = (!front ? ResourceManager.ChunkTexturesBack[chunkId] : ResourceManager.ChunkTexturesFront[chunkId]);
					g.DrawImage(tex, new Rectangle((x * 128 - leftOffset) * 4, (y * 128 - topOffset) * 4, 128 * 4, 128 * 4), Color.White);
				}
			}
		}

		private void DrawCollision(Graphics g, Rectangle view)
		{
			if (mCollisionTexture != null)
				mCollisionTexture.Dispose();

			mCollisionTexture = new Texture2D(g.GraphicsDevice, view.Width, view.Height);
			Color[] bits = new Color[view.Width * view.Height];
			for (int y = 0; y < view.Height; y++) {
				for (int x = 0; x < view.Width; x++) {
					bool t = mLevel.IsSolid(x + view.X, y + view.Y, mPlayer.MainCharacter.Layer, false, true);
					bool lrb = mLevel.IsSolid(x + view.X, y + view.Y, mPlayer.MainCharacter.Layer, true, false);
					if (t && lrb)
						bits[y * view.Width + x] = Color.Black;
					else if (t && !lrb)
						bits[y * view.Width + x] = Color.White;
					else if (!t && lrb)
						bits[y * view.Width + x] = Color.Yellow;
					else
						bits[y * view.Width + x] = Color.Transparent;
				}
			}
			mCollisionTexture.SetData(bits);

			g.DrawImage(mCollisionTexture, mBounds, Color.White);
		}

		private void DrawHUD(Graphics g)
		{
			Color timeColour = Color.Yellow;
			Color ringsColour = Color.Yellow;
			if (mUpdateCount % 16 >= 8) {
				if (mPlayer.Time >= 60 * 60 * 9)
					timeColour = Color.Red;
				if (mPlayer.Rings == 0)
					ringsColour = Color.Red;
			}

			ResourceManager.NormalFont.DrawString(g, "SCORE", 16 * 4, 9 * 4, Color.Yellow);
			ResourceManager.NormalFont.DrawString(g, "TIME", 17 * 4, 25 * 4, timeColour);
			ResourceManager.NormalFont.DrawString(g, "RINGS", 16 * 4, 41 * 4, ringsColour);

			string scoreText = mPlayer.Score.ToString();
			string timeText = String.Format("{0}:{1:00}", mPlayer.Time / 60 / 60, (mPlayer.Time / 60) % 60);
			string ringsText = mPlayer.Rings.ToString();

			ResourceManager.NormalFont.DrawString(g, scoreText, 111 * 4 - ResourceManager.NormalFont.MeasureStringWidth(scoreText), 9 * 4);
			ResourceManager.NormalFont.DrawString(g, timeText, 87 * 4 - ResourceManager.NormalFont.MeasureStringWidth(timeText), 25 * 4);
			ResourceManager.NormalFont.DrawString(g, ringsText, 87 * 4 - ResourceManager.NormalFont.MeasureStringWidth(ringsText), 41 * 4);
		}

		public Rectangle Bounds
		{
			get
			{
				return mBounds;
			}
			set
			{
				mBounds = value;
			}
		}
	}
}
