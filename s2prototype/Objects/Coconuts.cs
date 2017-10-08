using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Coconuts : Badnik
	{
		enum CoconutsState
		{
			Init,
			Resting,
			Climbing,
			Throwing,
		}

		private Animation mAnimation;
		private CoconutsState mStatus;
		private int mThrowingStatus;
		private bool mFacingRight;
		private int mStateDuration;
		private int mClimbIndex;
		private int mThrowWaitDuration;

		private static int[] ClimbData = new int[] {
			-256, 0x20,
			 256, 0x18,
			-256, 0x10,
			 256, 0x28,
			-256, 0x20,
			 256, 0x10,
		};

		private static byte[][] AnimationData = new byte[][] {
			new byte[] { 5, 0, 1, 0xFF },
			new byte[] { 9, 1, 2, 1, 0xFF },
		};

		public Coconuts(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mAnimation = new Animation(AnimationData);

			RadiusX = 6;
			RadiusY = 8;
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(mAnimation.FrameValue * 32 * Game.DisplayScale, 0 * Game.DisplayScale, 32 * Game.DisplayScale, 48 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-16 * Game.DisplayScale, -24 * Game.DisplayScale, 32 * Game.DisplayScale, 48 * Game.DisplayScale);

			SpriteEffects effects = SpriteEffects.None;
			if (mFacingRight)
				effects = SpriteEffects.FlipHorizontally;

			g.DrawImage(ResourceManager.CoconutsTexture, dst, src, Color.White, effects);
		}

		public override void Update()
		{
			if (!IsCloseToCharacter)
				return;

			switch (mStatus) {
				case CoconutsState.Init:
					Init();
					break;
				case CoconutsState.Resting:
					Rest();
					break;
				case CoconutsState.Climbing:
					Climbing();
					break;
				case CoconutsState.Throwing:
					Throwing();
					break;
			}
		}

		private void Init()
		{
			// Start rest
			mStateDuration = 16;
			mStatus = CoconutsState.Resting;
		}

		private void Rest()
		{
			//  a1 = address of closest player character
			//  d0 = 0 if right from player, 2 if left
			//  d1 = 0 if above player, 2 if under
			//  d2 = horizontal distance to closest character
			//  d3 = vertical distance to closest character

			int directionX = 0;
			int directionY = 0;
			int horizDist = 12;
			int vertDist = 80;
			Level.GetClosestCharacter(this, out directionX, out directionY, out horizDist, out vertDist);

			// Set direction coconuts is facing
			mFacingRight = (directionX != 0);

			// Is player in range
			if (horizDist + 96 < 192 && Math.Abs(vertDist) < 100) {
				if (mThrowWaitDuration == 0) {
					mStatus = CoconutsState.Throwing;
					mAnimation.FrameValue = 1;
					mStateDuration = 8;
					mThrowWaitDuration = 32;
					return;
				}
				mThrowWaitDuration--;
			}

			mStateDuration--;

			// If rest has finished, start climb
			if (mStateDuration < 0) {
				mStatus = CoconutsState.Climbing;
				SetClimbVelocity();
			}
		}

		private void Climbing()
		{
			mStateDuration--;

			// Has climb finished
			if (mStateDuration == 0) {
				mStatus = CoconutsState.Resting;
				mStateDuration = 16;
				return;
			}

			// Update climb position
			UpdatePosition();
			mAnimation.Update();
		}

		private void Throwing()
		{
			if (mThrowingStatus == 0) {
				mStateDuration--;
				if (mStateDuration < 0) {
					mThrowingStatus += 2;
					mStateDuration = 8;
					mAnimation.FrameValue = 2;
					FireCoconut();
				}
			} else if (mThrowingStatus == 2) {
				mStateDuration--;
				if (mStateDuration < 0) {
					mThrowingStatus = 0;
					mStatus = CoconutsState.Climbing;
					mStateDuration = 8;
					SetClimbVelocity();
				}
			}
		}

		private void SetClimbVelocity()
		{
			if (mClimbIndex >= 12)
				mClimbIndex = 0;
			VelocityY = ClimbData[mClimbIndex];
			mStateDuration = ClimbData[mClimbIndex + 1];
			mClimbIndex += 2;
		}

		private void FireCoconut()
		{
			CoconutProjectile coconut = new CoconutProjectile(Game, Level);
			coconut.DisplacementX = DisplacementX;
			coconut.DisplacementY = DisplacementY - 13;

			if (mFacingRight) {
				coconut.DisplacementX -= 11;
				coconut.VelocityX = 256;
			} else {
				coconut.DisplacementX += 11;
				coconut.VelocityX = -256;
			}

			coconut.VelocityY = -256;
			Level.Objects.Add(coconut);
		}

		public override int Id
		{
			get { return 157; }
		}

		class CoconutProjectile : LevelObject
		{
			public CoconutProjectile(SonicGame game, Level level)
				: base(game, level)
			{
				RadiusX = 4;
				RadiusY = 4;
			}

			public override void Draw(Graphics g)
			{
				Rectangle src = new Rectangle(96 * Game.DisplayScale, 0 * Game.DisplayScale, 12 * Game.DisplayScale, 13 * Game.DisplayScale);
				Rectangle dst = new Rectangle(-6 * Game.DisplayScale, -6 * Game.DisplayScale, 12 * Game.DisplayScale, 13 * Game.DisplayScale);

				g.DrawImage(ResourceManager.CoconutsTexture, dst, src, Color.White);
			}

			public override void Update()
			{
				if (!IsCloseToCharacter) {
					Finished = true;
					return;
				}

				VelocityY += 32;
				UpdatePosition();
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
				get { return 152; }
			}
		}
	}
}
