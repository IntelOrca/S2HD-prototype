using System;
using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class Starpost : LevelObject
	{
		private int mIndex;

		private int mRoutine;
		private int mStarOffsetDisplacementX;
		private int mStarOffsetDisplacementY;
		private int mAngle;
		private int mDongleDuration;

		private int mMappingFrame;
		private int mAnimationFrameDuration;

		public Starpost(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mIndex = definition.SubType;
		}

		public override void Draw(Graphics g)
		{
			Rectangle dst = new Rectangle(-8 * Game.DisplayScale, -24 * Game.DisplayScale, 16 * Game.DisplayScale, 48 * Game.DisplayScale);
			Rectangle src = new Rectangle(0 * Game.DisplayScale, 32 * Game.DisplayScale, 16 * Game.DisplayScale, 48 * Game.DisplayScale);
			g.DrawImage(ResourceManager.StarpostTexture, dst, src, Color.White);

			dst = new Rectangle((-8 + mStarOffsetDisplacementX) * Game.DisplayScale, (-24 - 5 + mStarOffsetDisplacementY) * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			src = new Rectangle(0 * Game.DisplayScale, mMappingFrame * 16 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			g.DrawImage(ResourceManager.StarpostTexture, dst, src, Color.White);
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					foreach (Player player in Game.Players)
						if (CheckActivation(player.MainCharacter))
							Activate(player);
					break;
				case 4:
					UpdateDongle();
					break;
				case 6:
					mAnimationFrameDuration--;
					if (mAnimationFrameDuration <= 0) {
						mMappingFrame = (mMappingFrame + 1) % 2;
						mAnimationFrameDuration = 4;
					}
					break;
			}

			UpdateStarLocation();
		}

		private void Init()
		{
			mDongleDuration = 32;
			mRoutine = 2;

			if (Game.Players[0].LastStarpostIndex >= mIndex)
				mRoutine = 6;
		}

		private void UpdateDongle()
		{
			mDongleDuration--;
			if (mDongleDuration < 0) {
				mAngle = 0;
				mRoutine = 6;
				return;
			}

			UpdateStarLocation();

			mAngle -= 16;
		}

		private void UpdateStarLocation()
		{
			mStarOffsetDisplacementY = SonicMaths.Sin(mAngle - 64) * 11 >> 8;
			mStarOffsetDisplacementX = SonicMaths.Cos(mAngle - 64) * 11 >> 8;
		}

		private void Activate(Player player)
		{
			mRoutine = 4;
			Level.AddSound(ResourceManager.StarpostSound, DisplacementX, DisplacementY);

			player.LastStarpostIndex = mIndex;
			player.LastStarpostTime = player.Time;
		}

		private bool CheckActivation(Character character)
		{
			if (Math.Abs(character.DisplacementX - DisplacementX) >= 8)
				return false;
			if (Math.Abs(character.DisplacementY - DisplacementY) >= 40)
				return false;
			return true;
		}

		public int Index
		{
			get
			{
				return mIndex;
			}
		}

		public override int Id
		{
			get { return 121; }
		}
	}
}
