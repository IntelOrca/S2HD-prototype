using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Explosion : LevelObject
	{
		private int mStatus;
		private int mAnimFrameDuration;
		private int mMappingFrame;

		public Explosion(SonicGame game, Level level)
			: base(game, level)
		{
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(mMappingFrame * 32 * Game.DisplayScale, 0 * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-16 * Game.DisplayScale, -16 * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);

			g.DrawImage(ResourceManager.ExplosionTexture, dst, src, Color.White);
		}

		private void Init()
		{
			DrawPriority = 80;
			mAnimFrameDuration = 3;
			mStatus = 1;

			// Play explosion sound
			Level.AddSound(ResourceManager.BadnikExplosionSound, DisplacementX, DisplacementY);
		}

		public override void Update()
		{
			if (mStatus == 0)
				Init();

			mAnimFrameDuration--;
			if (mAnimFrameDuration < 0) {
				mAnimFrameDuration = 7;
				mMappingFrame++;
				if (mMappingFrame == 5)
					Finished = true;
			}
		}

		public override int Id
		{
			get { return 39; }
		}
	}
}
