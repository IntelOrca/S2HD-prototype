using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Animal : LevelObject
	{
		private int mRoutine;
		private int mGroundVelocityX;
		private int mGroundVelocityY;

		private int mMappingFrame;
		private int mAnimationDuration;
		private bool mFacingLeft;

		private bool mJumping;

		private int mSubType;

		public Animal(SonicGame game, Level level)
			: base(game, level)
		{
		}

		public override void Draw(Graphics g)
		{
			int srcX = mSubType % 2 + mMappingFrame;
			int srcY = mSubType / 2;

			Rectangle dst = new Rectangle(-12 * Game.DisplayScale, -16 * Game.DisplayScale, 24 * Game.DisplayScale, 32 * Game.DisplayScale);
			Rectangle src = new Rectangle(srcX * 24 * Game.DisplayScale, srcY * 32 * Game.DisplayScale, 24 * Game.DisplayScale, 32 * Game.DisplayScale);

			SpriteEffects fx = SpriteEffects.None;
			if (VelocityX < 0)
				fx = SpriteEffects.FlipHorizontally;

			g.DrawImage(ResourceManager.AnimalsTexture, dst, src, Color.White, fx);
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					MainUpdate();
					break;
			}
		}

		private void Init()
		{
			mGroundVelocityX = -0x300;
			mGroundVelocityY = -0x400;

			VelocityX = 0;
			VelocityY = -0x400;

			mRoutine = 2;
		}

		private void MainUpdate()
		{
			if (!IsCloseToCharacter) {
				Finished = true;
				return;
			}

			if (mJumping) {
				UpdatePosition();
				VelocityY += 24;
			} else {
				UpdatePositionWithGravity();
			}

			if (VelocityY > 0) {
				int dist, angle = 0;
				Level.FindFloor(DisplacementX, DisplacementY + 8, 1, false, true, out dist, ref angle);
				if (dist < 0) {
					// Bounce of ground
					DisplacementY += dist;
					VelocityX = mGroundVelocityX;
					VelocityY = mGroundVelocityY;
					mJumping = true;
				}
			}

			if (VelocityX == 0) {
				mMappingFrame = 0;
			} else {
				mAnimationDuration--;
				if (mAnimationDuration < 0) {
					mAnimationDuration = 2;
					mMappingFrame++;
					if (mMappingFrame > 2 || mMappingFrame < 1)
						mMappingFrame = 1;
				}
			}
		}

		public override int Id
		{
			get { return 40; }
		}

		public int SubType
		{
			get
			{
				return mSubType;
			}
			set
			{
				mSubType = value;
			}
		}
	}
}
