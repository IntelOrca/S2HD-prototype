using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class Signpost : LevelObject
	{
		private int mRoutine;

		private int mSpinAnimation;
		private int mPersonShowing;

		private bool mWaitingForPlayer;
		private int mSpinWait;
		private int mSpinsRemaining;

		private int mSparkleOffsetIndex;
		private int mNextSparkleDuration;

		private static int[] SparkleOffsets = new int[] {
			-24,-16,
			8,  8,
			-16,  0,
			24, -8,
			0, -8,
			16,  0,
			-24,  8,
			24, 16,
		};

		public Signpost(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
		}

		public override void Draw(Graphics g)
		{
			int mappingFrameX = mSpinAnimation / 2;
			int mappingFrameY = mPersonShowing;
			if (mappingFrameX >= 3) {
				mappingFrameX = 1;
				mappingFrameY = 4;
			}

			Rectangle dst = new Rectangle(-24 * Game.DisplayScale, -24 * Game.DisplayScale, 48 * Game.DisplayScale, 48 * Game.DisplayScale);
			Rectangle src = new Rectangle(mappingFrameX * 48 * Game.DisplayScale, mappingFrameY * 48 * Game.DisplayScale, 48 * Game.DisplayScale, 48 * Game.DisplayScale);

			dst.Y += 8 * Game.DisplayScale;

			g.DrawImage(ResourceManager.SignpostTexture, dst, src, Color.White);
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 1:
					UpdateWait();
					break;
				case 2:
					UpdateSpin();
					break;
				case 3:
					UpdateSettled();
					break;
			}
		}

		private void Init()
		{
			mWaitingForPlayer = true;
			mSpinWait = 8;
			mSpinAnimation = 2;
			mPersonShowing = 0;
			mRoutine = 1;
			mSpinsRemaining = 14;
		}

		private void UpdateWait()
		{
			if (mWaitingForPlayer) {
				if (Game.Players[0].MainCharacter.DisplacementX < DisplacementX)
					return;
				Game.Players[0].Status = PlayerStatus.Finished;
				mWaitingForPlayer = false;
				Level.AddSound(ResourceManager.SignpostSound, DisplacementX, DisplacementY);
			} else {
				mSpinWait--;
				if (mSpinWait <= 0) {
					mRoutine = 2;
					mSpinAnimation = 4;
				}
			}

			UpdateSparkles();
		}

		private void UpdateSpin()
		{
			mSpinAnimation++;
			if (mSpinsRemaining <= 0 && mSpinAnimation == 2) {
				mRoutine = 3;
				return;
			}

			if (mSpinAnimation > 7) {
				mSpinsRemaining--;
				if (mSpinsRemaining <= 0) {
					// Show main character
					mSpinAnimation = 0;
					mPersonShowing = 2;
				} else {
					// Next person to show
					mSpinAnimation = 0;
					mPersonShowing = (mPersonShowing + 1) % 3;
				}
			}

			UpdateSparkles();
		}

		private void UpdateSettled()
		{
			// Show level score breakdown
		}

		private void UpdateSparkles()
		{
			mNextSparkleDuration--;
			if (mNextSparkleDuration <= 0) {
				mNextSparkleDuration = 12;
				mSparkleOffsetIndex = (mSparkleOffsetIndex + 1) % 8;

				Sparkle sparkleObject = new Sparkle(Game, Level);
				sparkleObject.DisplacementX = DisplacementX + SparkleOffsets[mSparkleOffsetIndex * 2];
				sparkleObject.DisplacementY = DisplacementY + SparkleOffsets[mSparkleOffsetIndex * 2 + 1];
				Level.Objects.Add(sparkleObject);
			}
		}

		public override int Id
		{
			get { return 13; }
		}

		class Sparkle : LevelObject
		{
			private int mAnimationFrameDuration = 6;
			private int mMappingFrame;

			public Sparkle(SonicGame game, Level level)
				: base(game, level)
			{
			}

			public override void Draw(Graphics g)
			{
				Rectangle dst = new Rectangle(-8 * Game.DisplayScale, -8 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
				Rectangle src = new Rectangle(mMappingFrame * 16 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
				g.DrawImage(ResourceManager.RingTexture, dst, src, Color.White);
			}

			public override void Update()
			{
				mAnimationFrameDuration--;
				if (mAnimationFrameDuration <= 0) {
					mMappingFrame++;
					if (mMappingFrame == 4) {
						Finished = true;
						return;
					}
				}
			}

			public override int Id
			{
				get { return 13; }
			}
		}
	}
}
