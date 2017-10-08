using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Masher : Badnik
	{
		private Animation mAnimation;

		private int mStatus;
		private int mInitialY;

		private static byte[][] AnimationData = new byte[][] {
			new byte[] { 7, 0, 1, 0xFF },
			new byte[] { 3, 0, 1, 0xFF },	
			new byte[] { 7, 0, 0xFF },
		};

		public Masher(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mAnimation = new Animation(AnimationData);

			RadiusX = 12;
			RadiusY = 16;
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(mAnimation.FrameValue * 32 * Game.DisplayScale, 0 * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-15 * Game.DisplayScale, -15 * Game.DisplayScale, 30 * Game.DisplayScale, 30 * Game.DisplayScale);

			g.DrawImage(ResourceManager.MasherTexture, dst, src, Color.White);
		}

		private void Init()
		{
			VelocityY = -400;
			mInitialY = DisplacementY;
			mStatus = 1;
		}

		public override void Update()
		{
			if (!IsCloseToCharacter)
				return;

			if (mStatus == 0)
				Init();

			mAnimation.Update();
			UpdatePosition();
			VelocityY += 24;
			if (mInitialY < DisplacementY) {
				DisplacementY = mInitialY;
				VelocityY = -1280;
			}
			mAnimation.Index = 1;
			if (DisplacementY - 192 < DisplacementY) {
				mAnimation.Index = 0;
				if (VelocityY >= 0)
					mAnimation.Index = 2;
			}
		}

		public override int Id
		{
			get { return 92; }
		}
	}
}
