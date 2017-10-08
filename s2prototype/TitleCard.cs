using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class TitleCard
	{
		private SonicGame mGame;
		private Level mLevel;

		private int mUpdateCount;
		private bool mAllowStart;
		private bool mFinished;

		private bool mShowGame;

		private int mBlueRectangleStatus;
		private float mBlueRectanglePosition;

		private int mYellowRectangleStatus;
		private float mYellowRectanglePosition;

		private int mRedRectangleStatus;
		private float mRedRectanglePosition;

		private int mInitialisedWidth;
		private int mInitialisedHeight;
		private Texture2D mRectangleTexture;

		public TitleCard(SonicGame game, Level level)
		{
			mGame = game;
			mLevel = level;
		}

		public void Start()
		{
			mUpdateCount = 0;
			mAllowStart = false;
			mFinished = false;
		}

		public void Update()
		{
			if (mInitialisedWidth == 0 || mInitialisedHeight == 0)
				return;

			if (mUpdateCount > 60)
				mBlueRectangleStatus = 1;

			if (mUpdateCount > 68)
				mYellowRectangleStatus = 1;

			if (mUpdateCount > 78)
				mRedRectangleStatus = 1;

			if (mUpdateCount > 170)
				mRedRectangleStatus = 2;

			if (mUpdateCount > 180) {
				mShowGame = true;
				mYellowRectangleStatus = 2;
			}

			if (mUpdateCount > 190)
				mBlueRectangleStatus = 2;

			if (mUpdateCount > 195)
				mAllowStart = true;

			if (mBlueRectangleStatus == 1 && mBlueRectanglePosition < 1.0f)
				mBlueRectanglePosition += 0.08f;
			else if (mBlueRectangleStatus == 2 && mBlueRectanglePosition > 0.0f)
				mBlueRectanglePosition -= 0.1f;

			if (mYellowRectangleStatus == 1 && mYellowRectanglePosition < 1.0f)
				mYellowRectanglePosition += 0.08f;
			else if (mYellowRectangleStatus == 2 && mYellowRectanglePosition > 0.0f)
				mYellowRectanglePosition -= 0.1f;

			if (mRedRectangleStatus == 1 && mRedRectanglePosition < 1.0f)
				mRedRectanglePosition += 0.15f;
			else if (mYellowRectangleStatus == 2 && mRedRectanglePosition > 0.0f)
				mRedRectanglePosition -= 0.25f;

			mBlueRectanglePosition = Math.Min(1.0f, Math.Max(0.0f, mBlueRectanglePosition));
			mYellowRectanglePosition = Math.Min(1.0f, Math.Max(0.0f, mYellowRectanglePosition));
			mRedRectanglePosition = Math.Min(1.0f, Math.Max(0.0f, mRedRectanglePosition));

			mUpdateCount++;
			if (mUpdateCount > 240)
				mFinished = true;
		}

		public void Draw(Graphics g, Rectangle view)
		{
			Init(g, view.Width, view.Height);

			if (!mShowGame)
				g.DrawImage(mRectangleTexture, new Vector2(0, 0), Color.Black);
			g.DrawImage(mRectangleTexture, new Vector2(0, -mInitialisedHeight + (mBlueRectanglePosition * mInitialisedHeight)), new Color(36, 72, 216));
			g.DrawImage(mRectangleTexture, new Vector2(mInitialisedWidth - (mYellowRectanglePosition * mInitialisedWidth), mInitialisedHeight * 0.65f), new Color(252, 252, 0));
			g.DrawImage(mRectangleTexture, new Vector2(-mInitialisedWidth + (mRedRectanglePosition * mInitialisedWidth * 0.3f), 0), new Color(252, 0, 0));
		}

		private void Init(Graphics g, int width, int height)
		{
			if (mInitialisedWidth == width && mInitialisedHeight == height)
				return;

			mInitialisedWidth = width;
			mInitialisedHeight = height;
			
			mRectangleTexture = new Texture2D(g.GraphicsDevice, width, height);
			Color[] bits = new Color[width * height];
			for (int i = 0; i < bits.Length; i++)
				bits[i] = new Color(255, 255, 255);
			mRectangleTexture.SetData(bits);
		}

		public bool AllowStart
		{
			get
			{
				return mAllowStart;
			}
		}

		public bool IsFinished
		{
			get
			{
				return mFinished;
			}
		}
	}
}
