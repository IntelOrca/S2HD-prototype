using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Buzzer : Badnik
	{
		private Animation mAnimation;
		private int mStatus;
		private int mFlightDuration;
		private int mTurnWaitDuration;
		private int mFireDuration;
		private bool mFiredThisRound;

		private static byte[][] AnimationData = new byte[][] {
			new byte[] { 0, 0, 0xFF },
			new byte[] { 2, 1, 2, 0xFF },
			new byte[] { 3, 3, 4, 0xFF },
			new byte[] { 2, 5, 6, 0xFF },
		};

		public Buzzer(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mAnimation = new Animation(AnimationData);

			RadiusX = 16;
			RadiusY = 8;
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(mAnimation.FrameValue * 48 * Game.DisplayScale, 0 * Game.DisplayScale, 48 * Game.DisplayScale, 46 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-24 * Game.DisplayScale, -23 * Game.DisplayScale, 48 * Game.DisplayScale, 46 * Game.DisplayScale);

			SpriteEffects fx = (VelocityX >= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			g.DrawImage(ResourceManager.BuzzerTexture, dst, src, Color.White, fx);
		}

		public override void Update()
		{
			if (!IsCloseToCharacter) {
				// Reset object
				mStatus = 0;
				return;
			}

			if (mStatus == 0) {
				Init();
				return;
			} else if (mStatus == 2) {
				UpdateFireProjectile();
			} else {
				if (CheckForCharacter())
					return;

				if (mTurnWaitDuration > 0) {
					mTurnWaitDuration--;
					if (mTurnWaitDuration == 15) {
						VelocityX = -VelocityX;
						mFiredThisRound = false;
					}
				} else {
					mAnimation.Index = 1;
					mFlightDuration--;
					if (mFlightDuration > 0) {
						UpdatePosition();
					} else {
						mFlightDuration = 256;
						mTurnWaitDuration = 30;
						mAnimation.Index = 0;
					}
				}
			}

			mAnimation.Update();
		}

		private void Init()
		{
			mFlightDuration = 256;
			mStatus = 1;
			VelocityX = -256;
			DisplacementX = Definition.DisplacementX;
		}

		private bool CheckForCharacter()
		{
			if (mFiredThisRound)
				return false;

			int directionX, directionY, distX, distY;
			Character character = Level.GetClosestCharacter(this, out directionX, out directionY, out distX, out distY);

			int d0 = DisplacementX - character.DisplacementX;
			int d1 = d0;
			if (d1 < 0)
				d0 = -d0;
			if (d0 < 40 || d0 > 48)
				return false;

			mStatus = 2;
			mAnimation.Index = 2;
			mFireDuration = 50;
			mFiredThisRound = true;
			return true;
		}

		private void UpdateFireProjectile()
		{
			mFireDuration--;
			if (mFireDuration < 0) {
				mStatus = 1;
				return;
			}

			if (mFireDuration == 20)
				FireProjectile();
		}

		private void FireProjectile()
		{
			BuzzerProjectile projectile = new BuzzerProjectile(Game, Level);
			projectile.DisplacementX = DisplacementX;
			projectile.DisplacementY = DisplacementY + 24;
			projectile.VelocityY = 384;

			if (VelocityX >= 0) {
				projectile.DisplacementX -= 13;
				projectile.VelocityX = 384;
			} else {
				projectile.VelocityX = -384;
				projectile.DisplacementX += 13;
			}

			Level.Objects.Add(projectile);
		}

		public override int Id
		{
			get { return 75; }
		}

		class BuzzerProjectile : LevelObject
		{
			private Animation mAnimation;

			public BuzzerProjectile(SonicGame game, Level level)
				: base(game, level)
			{
				mAnimation = new Animation(AnimationData);
				mAnimation.Index = 3;
				RadiusX = 4;
				RadiusY = 4;
				mAnimation.Update();
			}

			public override void Draw(Graphics g)
			{
				Rectangle src = new Rectangle(mAnimation.FrameValue * 48 * Game.DisplayScale, 0 * Game.DisplayScale, 48 * Game.DisplayScale, 46 * Game.DisplayScale);
				Rectangle dst = new Rectangle(-24 * Game.DisplayScale, -23 * Game.DisplayScale, 48 * Game.DisplayScale, 46 * Game.DisplayScale);

				SpriteEffects fx = (VelocityX >= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
				g.DrawImage(ResourceManager.BuzzerTexture, dst, src, Color.White, fx);
			}

			public override void Update()
			{
				if (!IsCloseToCharacter) {
					Finished = true;
					return;
				}

				UpdatePosition();
				mAnimation.Update();
			}

			public override void Touch(LevelObject obj)
			{
				Character character = obj as Character;
				if (character == null)
					return;

				character.Hurt(this);
			}

			public override int Id
			{
				get { return 75; }
			}
		}
	}
}
