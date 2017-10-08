using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Spikes : SolidObject
	{
		private int mRoutine;
		private int mMappingFrame;

		private int mInitialDisplacementX;
		private int mInitialDisplacementY;
		private int mMovementOffset;
		private int mMoveDirection;
		private int mMoveWaitDuration;

		private int mSubType;
		private bool mFlipX;
		private bool mFlipY;

		private int[] SizeData = new int[] {
			16, 16,
			32, 32,
			48, 16,
			64, 16,
			16, 16,
			16, 32,
			16, 48,
			16, 64,
		};

		public Spikes(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mSubType = definition.SubType;
			mFlipX = definition.FlipX;
			mFlipY = definition.FlipY;
		}

		public override void Draw(Graphics g)
		{
			Rectangle src = new Rectangle(0, 0, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
			Rectangle dst = new Rectangle(-RadiusX * Game.DisplayScale, -RadiusY * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);

			SpriteEffects fx = SpriteEffects.None;
			if (mFlipX)
				fx |= SpriteEffects.FlipHorizontally;
			if (mFlipY)
				fx |= SpriteEffects.FlipVertically;

			for (int y = 0; y < RadiusY / 16; y++) {
				for (int x = 0; x < RadiusX / 16; x++) {
					dst = new Rectangle((-RadiusX + (x * 32)) * Game.DisplayScale, (-RadiusY + (y * 32)) * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
					g.DrawImage(ResourceManager.SpikesTexture, dst, src, Color.White, fx);
				}
			}
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					UpdatePointingUp();
					break;
				case 4:
					UpdatePointingLeftRight();
					break;
				case 6:
					UpdatePointingDown();
					break;
			}
		}

		private void Init()
		{
			int d0;

			mRoutine += 2;
			d0 = mSubType & 0xF0;
			mSubType &= 0x0F;

			RadiusX = SizeData[d0 >> 3];
			RadiusY = SizeData[d0 >> 3 + 1];
			mMappingFrame = d0 >> 4;
			if (d0 >> 4 >= 4)
				mRoutine = 4;
			if (mFlipY)
				mRoutine = 6;
			mInitialDisplacementX = DisplacementX;
			mInitialDisplacementY = DisplacementY;
		}

		private void UpdatePointingUp()
		{
			UpdateMovement();

			DoCharacterTouchResponse(RadiusX + 11, RadiusY, RadiusY + 1, DisplacementX);
			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus)
				if ((kvp.Value & SolidObjectTouch.Standing) != 0)
					HurtCharacter(kvp.Key);
		}

		private void UpdatePointingLeftRight()
		{
			UpdateMovement();

			DoCharacterTouchResponse(RadiusX + 11, RadiusY, RadiusY + 1, DisplacementX);
			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus)
				if ((kvp.Value & SolidObjectTouch.Pushing) != 0)
					HurtCharacter(kvp.Key);
		}

		private void UpdatePointingDown()
		{
			UpdateMovement();

			DoCharacterTouchResponse(RadiusX + 11, RadiusY, RadiusY + 1, DisplacementX);
			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus)
				if ((kvp.Value & SolidObjectTouch.Bottom) != 0)
					HurtCharacter(kvp.Key);
		}

		private void HurtCharacter(Character character)
		{
			if ((character.StatusSecondary & 2) != 0)
				return;
			if (character.Invulnerable)
				return;
			if (character.Routine >= 4)
				return;

			character.DisplacementY = character.DisplacementY - (character.VelocityY << 8);
			character.Hurt(this);
		}

		private void UpdateMovement()
		{
			switch (mSubType) {
				case 1:
					UpdateMovement2();
					DisplacementY = (mMovementOffset >> 8) + mInitialDisplacementY;
					break;
				case 2:
					UpdateMovement2();
					DisplacementX = (mMovementOffset >> 8) + mInitialDisplacementY;
					break;
			}
		}

		private void UpdateMovement2()
		{
			if (mMoveWaitDuration == 0) {
				if (mMoveDirection != 0) {
					mMovementOffset -= 2048;
					if (mMovementOffset >= 0)
						return;
					mMovementOffset = 0;
					mMoveDirection = 0;
					mMoveWaitDuration = 60;
				} else {
					mMovementOffset += 2048;
					if (mMovementOffset < 8192)
						return;
					mMovementOffset = 8192;
					mMoveDirection = 1;
					mMoveWaitDuration = 60;
				}
			} else {
				mMoveWaitDuration--;
				if (mMoveWaitDuration != 0)
					return;

				// test render flags

				Level.AddSound(ResourceManager.SpikesMoveSound, DisplacementX, DisplacementY);
			}
		}

		public override int Id
		{
			get { return 54; }
		}
	}
}
