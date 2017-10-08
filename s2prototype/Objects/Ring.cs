using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Ring : LevelObject
	{
		private Animation mAnimation;
		private bool mCollected;
		private bool mScattering;
		private int mTimeLeft;

		private static byte[][] AnimationData = new byte[][] {
			new byte[] { 8, 0, 1, 2, 3, 0xFF },
			new byte[] { 5, 4, 5, 6, 7, 0xFF },
		};

		public Ring(SonicGame game, Level level)
			: base(game, level)
		{
			mAnimation = new Animation(AnimationData);

			mTimeLeft = 256;

			// DrawPriority = 50;
			RadiusX = 6;
			RadiusY = 6;
		}

		public Ring(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mAnimation = new Animation(AnimationData);

			mTimeLeft = 256;

			// DrawPriority = 50;
			RadiusX = 6;
			RadiusY = 6;
		}

		public override void Draw(Graphics g)
		{
			Rectangle dst = new Rectangle(-8 * Game.DisplayScale, -8 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			Rectangle src = new Rectangle((mAnimation.FrameValue % 4) * 16 * Game.DisplayScale, 0 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);

			if (mAnimation.FrameValue > 3)
				src.Y += 16 * Game.DisplayScale;

			g.DrawImage(ResourceManager.RingTexture, dst, src, Color.White);
		}

		public override void Update()
		{
			if (mScattering) {
				UpdatePosition();
				VelocityY += 24;

				int dist, angle = 0;
				Level.FindFloor(DisplacementX, DisplacementY + 8, 1, false, true, out dist, ref angle);
				if (dist < 0) {
					// Bounce of ground
					DisplacementY += dist;
					VelocityY -= VelocityY >> 2;
					VelocityY = -VelocityY;
				}

				mTimeLeft--;
				if (mTimeLeft < 0)
					Finished = true;
			}

			mAnimation.Update();
			if (mCollected && mAnimation.Frame == 4)
				Finished = true;
		}

		public override void Touch(LevelObject obj)
		{
			Character character = obj as Character;
			if (character == null)
				return;

			if (mScattering)
				if (mTimeLeft > 256 - 64)
					return;

			if (!mCollected) {
				Level.AddSound(ResourceManager.RingSound, DisplacementX, DisplacementY);

				character.Player.AddRings(1);

				mCollected = true;
				mAnimation.Index = 1;
				mScattering = false;
				DrawPriority = 50;
			}
		}

		public override int Id
		{
			get { return 37; }
		}

		public bool Scattering
		{
			get
			{
				return mScattering;
			}
			set
			{
				mScattering = value;
			}
		}
	}
}
