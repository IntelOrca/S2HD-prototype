using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class Monitor : SolidObject
	{
		private int mRoutine;
		private int mRoutineSecondary;

		private bool mBroken;
		private Character mBraker;

		private int mAnimationFrame;
		private int mAnimationFrameDuration;

		private int mSubType;

		public Monitor(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			RadiusX = 16;
			RadiusY = 16;

			mRoutine = 2;
			mSubType = definition.SubType;
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(0 * Game.DisplayScale, 0 * Game.DisplayScale, 30 * Game.DisplayScale, 30 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-15 * Game.DisplayScale, -15 * Game.DisplayScale, 30 * Game.DisplayScale, 30 * Game.DisplayScale);

			if (mBroken)
				src.X = 60 * Game.DisplayScale;
			else if (mAnimationFrame == 1)
				src.X = 30 * Game.DisplayScale;

			g.DrawImage(ResourceManager.MonitorTexture, dst, src, Color.White);

			if (!mBroken && mAnimationFrame == 0) {
				int[] typePositions = new int[] { 0, 4, 4, 0, 1, 2, 0, 3, 0, 0, 0 };
				src.X = 14 * typePositions[mSubType] * Game.DisplayScale;
				src.Y = 30 * Game.DisplayScale;
				src.Width = 14 * Game.DisplayScale;
				src.Height = 12 * Game.DisplayScale;

				dst.X += 8 * Game.DisplayScale;
				dst.Y += 6 * Game.DisplayScale;
				dst.Width = 14 * Game.DisplayScale;
				dst.Height = 12 * Game.DisplayScale;

				g.DrawImage(ResourceManager.MonitorTexture, dst, src, Color.White);
			}
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 2:
					UpdateMain();
					break;
				case 4:
					UpdateBreak();
					break;
			}

			UpdateAnimation();
		}

		private void UpdateAnimation()
		{
			mAnimationFrameDuration--;
			if (mAnimationFrameDuration < 0) {
				mAnimationFrame = (mAnimationFrame + 1) % 2;
				if (mAnimationFrame == 0)
					mAnimationFrameDuration = 3;
				else
					mAnimationFrameDuration = 1;
			}
		}

		private void UpdateMain()
		{
			if (mRoutineSecondary != 0) {
				UpdatePositionWithGravity();

				int dist, angle = 0;
				Level.FindFloor(DisplacementX, DisplacementY + RadiusY, 1, false, true, out dist, ref angle);
				if (dist < 0) {
					DisplacementY += dist;
					VelocityY = 0;
					mRoutineSecondary = 0;
				}
			}

			DoCharacterTouchResponse(26, 15, 16, DisplacementX);
		}

		private void UpdateBreak()
		{
			mRoutine = 6;

			// Clear standing status for any other characters on monitor
			ClearCharacterTouchStatus();

			// Spawn icon
			MonitorContents iconObject = new MonitorContents(Game, Level);
			iconObject.DisplacementX = DisplacementX;
			iconObject.DisplacementY = DisplacementY;
			iconObject.Character = mBraker;
			iconObject.SubType = mSubType;
			Level.Objects.Add(iconObject);

			// Spawn smoke
			Explosion explosion = new Explosion(Game, Level);
			explosion.DisplacementX = DisplacementX;
			explosion.DisplacementY = DisplacementY;
			Level.Objects.Add(explosion);

			// Monitor is now broken
			mBroken = true;
		}

		protected override SolidObjectTouch GetCharacterTouch(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			if ((currentTouchStatus & SolidObjectTouch.Standing) != 0)
				return CheckOverEdge(currentTouchStatus, character, width, jumpRadiusY, walkRadiusY, x);

			if (character.Anim != CharacterAnimation.Roll)
				return loc_199F0(currentTouchStatus, character, width, jumpRadiusY, walkRadiusY, x);

			return currentTouchStatus;
		}

		private SolidObjectTouch CheckOverEdge(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			int d0;
			int d2 = width * 2;
			if ((character.Status & CharacterState.Airborne) == 0) {
				d0 = character.DisplacementX - DisplacementX + width;
				if (d0 >= 0)
					if (d0 < d2)
						return CharStandOn(currentTouchStatus, character, width, d2, walkRadiusY, x);
			}

			character.Status &= ~CharacterState.OnObject;
			character.Status |= CharacterState.Airborne;
			currentTouchStatus &= ~SolidObjectTouch.Standing;
			// d4 = 0;
			return currentTouchStatus;
		}

		private SolidObjectTouch CharStandOn(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			MoveCharacterOnPlatform(character, x, walkRadiusY);
			// d4 = 0;
			return currentTouchStatus;
		}

		public override void Touch(LevelObject obj)
		{
			if (mBroken)
				return;

			Character character = obj as Character;
			if (character == null)
				return;

			if (character.VelocityY < 0) {
				if (character.DisplacementY - 16 < DisplacementY)
					return;
				character.VelocityY = -character.VelocityY;
				VelocityY = -384;
				if (mRoutineSecondary != 0)
					return;
				mRoutineSecondary = 4;
			} else {
				// if sidekick, return

				if (character.Anim != CharacterAnimation.Roll)
					return;

				character.VelocityY = -character.VelocityY;
				if (mRoutine != 4) {
					mRoutine = 4;
					mBraker = character;
				}
			}
		}

		public override bool AllowBalancing
		{
			get
			{
				return true;
			}
		}

		public override int Id
		{
			get { return 38; }
		}

		class MonitorContents : LevelObject
		{
			private int mRoutine;
			private int mAnimFrameDuration;
			private Character mCharacter;
			private int mSubType;

			public MonitorContents(SonicGame game, Level level)
				: base(game, level)
			{
			}

			public override void Draw(Graphics g)
			{
				// Draw box
				Rectangle src = new Rectangle(7 * Game.DisplayScale, 5 * Game.DisplayScale, 16 * Game.DisplayScale, 14 * Game.DisplayScale);
				Rectangle dst = new Rectangle(-8 * Game.DisplayScale, -7 * Game.DisplayScale, 14 * Game.DisplayScale, 12 * Game.DisplayScale);

				g.DrawImage(ResourceManager.MonitorTexture, dst, src, Color.White);

				// Draw contents
				dst.X += Game.DisplayScale;
				dst.Y += Game.DisplayScale;
				dst.Width -= 2 * Game.DisplayScale;
				dst.Height -= 2 * Game.DisplayScale;

				int[] typePositions = new int[] { 0, 4, 4, 0, 1, 2, 0, 3, 0, 0, 0 };
				src.X = 14 * typePositions[mSubType] * Game.DisplayScale;
				src.Y = 30 * Game.DisplayScale;
				src.Width = 14 * Game.DisplayScale;
				src.Height = 12 * Game.DisplayScale;

				g.DrawImage(ResourceManager.MonitorTexture, dst, src, Color.White);
			}

			public override void Update()
			{
				switch (mRoutine) {
					case 0:
						Init();
						break;
					case 2:
						UpdateRaise();
						break;
					case 4:
						UpdateWait();
						break;
				}
			}

			private void Init()
			{
				mRoutine = 2;
				VelocityY = -768;
			}

			private void UpdateRaise()
			{
				if (VelocityY <= 0) {
					UpdatePosition();
					VelocityY += 24;
					return;
				}

				mRoutine = 4;
				mAnimFrameDuration = 29;
				DoAction();
			}

			private void UpdateWait()
			{
				mAnimFrameDuration--;
				if (mAnimFrameDuration < 0)
					Finished = true;
			}

			private void DoAction()
			{
				switch (mSubType) {
					case 1:
					case 2:
						mCharacter.Player.AddLife();
						break;
					case 4:
						Level.AddSound(ResourceManager.RingSound, DisplacementX, DisplacementY);
						mCharacter.Player.AddRings(10);
						break;
					case 5:
						mCharacter.GiveSpeedShoes();
						break;
					case 6:
						mCharacter.GiveShield();
						break;
					case 7:
						mCharacter.GiveInvincibility();
						break;
				}
			}

			public override int Id
			{
				get { return 39; }
			}

			public Character Character
			{
				get
				{
					return mCharacter;
				}
				set
				{
					mCharacter = value;
				}
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
}
