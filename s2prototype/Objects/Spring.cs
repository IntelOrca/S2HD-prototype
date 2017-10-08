using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Spring : SolidObject
	{
		private int mRoutine;
		private int mMappingFrame;
		private int mUnk30;
		private int mAnimationDuration;

		private int mSubType;
		private bool mFlipX;

		public Spring(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mSubType = definition.SubType;
			mFlipX = definition.FlipX;
		}

		public override void Draw(Graphics g)
		{
			int type = (mSubType >> 3) & 0x0E;
			bool diagonal = (type == 6 || type == 8);
			int size = 32;
			if (diagonal)
				size = 44;

			Rectangle dst = new Rectangle(-(size / 2) * Game.DisplayScale, -(size / 2) * Game.DisplayScale, size * Game.DisplayScale, size * Game.DisplayScale);
			Rectangle src = new Rectangle(mMappingFrame * size * Game.DisplayScale, 0, size * Game.DisplayScale, size * Game.DisplayScale);

			SpriteEffects fx = SpriteEffects.None;

			if ((mSubType & 2) != 0)
				src.X += 3 * size * Game.DisplayScale;

			switch (type) {
				case 0:
					dst.Y -= 7 * Game.DisplayScale;
					break;
				case 2:
					dst.X += 7 * Game.DisplayScale;
					src.Y += 32 * Game.DisplayScale;
					break;
				case 4:
					dst.Y += 8 * Game.DisplayScale;
					fx = SpriteEffects.FlipVertically;
					break;
				case 6:
					src.Y += 64 * Game.DisplayScale;
					if (mFlipX)
						dst.X -= 5 * Game.DisplayScale;
					else
						dst.X += 5 * Game.DisplayScale;
					dst.Y -= 5 * Game.DisplayScale;
					break;
				case 8:
					fx = SpriteEffects.FlipVertically;
					break;
			}


			if (mFlipX)
				fx = SpriteEffects.FlipHorizontally;

			g.DrawImage(ResourceManager.SpringTexture, dst, src, Color.White, fx);
		}

		public override void Update()
		{
			if (mAnimationDuration >= 0) {
				if (mAnimationDuration > 8)
					mMappingFrame = 1;
				else
					mMappingFrame = 2;
				mAnimationDuration--;
			} else {
				mMappingFrame = 1;
			}

			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					UpdateUp();
					break;
				case 4:
					UpdateHorizontal();
					break;
				case 6:
					UpdateDown();
					break;
				case 8:
					UpdateDiagonallyUp();
					break;
				case 10:
					UpdateDiagonallyDown();
					break;
			}
		}

		private void Init()
		{
			mRoutine = 10;
			switch ((mSubType >> 3) & 0x0E) {
				case 0:
					mRoutine = 2;
					RadiusX = 16;
					break;
				case 2:
					mRoutine = 4;
					RadiusX = 8;
					break;
				case 4:
					mRoutine = 6;
					RadiusX = 16;
					break;
				case 6:
					mRoutine = 8;
					RadiusX = 16;
					break;
				case 8:
					mRoutine = 10;
					RadiusX = 16;
					break;
			}

			if ((mSubType & 2) == 0)
				mUnk30 = -4096;
			else
				mUnk30 = -2560;

			mMappingFrame = 1;
		}

		private void UpdateUp()
		{
			DoCharacterTouchResponse(27, 8, 16, DisplacementX);

			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus.ToArray()) {
				if ((kvp.Value & SolidObjectTouch.Standing) == 0)
					continue;

				SpringUp(kvp.Key);
			}
		}

		private void UpdateHorizontal()
		{
			DoCharacterTouchResponse(19, 14, 15, DisplacementX);

			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus.ToArray()) {
				if ((kvp.Value & SolidObjectTouch.Pushing) == 0)
					continue;

				SpringHorizontally(kvp.Key);
			}

			loc_18BC6();
		}

		private void UpdateDown()
		{
			DoCharacterTouchResponse(27, 8, 16, DisplacementX);

			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus.ToArray()) {
				if ((kvp.Value & SolidObjectTouch.Bottom) == 0)
					continue;

				SpringDown(kvp.Key);
			}
		}

		private void UpdateDiagonallyUp()
		{
			DoCharacterTouchResponse(27, 16, 16, DisplacementX);

			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in CharacterTouchStatus.ToArray()) {
				if ((kvp.Value & (SolidObjectTouch.Standing | SolidObjectTouch.Side)) == 0)
					continue;

				SpringDiagonallyUp(kvp.Key);
			}
		}

		private void UpdateDiagonallyDown()
		{

		}

		private void SpringUp(Character character)
		{
			mMappingFrame = 0;
			mAnimationDuration = 10;

			character.VelocityY = mUnk30;
			character.DisplacementY += 8;
			character.Status |= CharacterState.Airborne;
			character.Status &= ~CharacterState.OnObject;
			character.Anim = CharacterAnimation.Spring;
			character.Routine = 2;

			if (mSubType < 0)
				character.VelocityX = 0;

			if ((mSubType & 1) != 0) {
				character.GroundVelocity = 1;
				character.FlipAngle = 1;
				character.Anim = 0;
				character.FlipsRemaining = 0;
				character.FlipSpeed = 4;
				if ((mSubType & 2) == 0)
					character.FlipsRemaining = 1;
				if ((character.Status & CharacterState.FacingLeft) != 0) {
					character.FlipAngle = -character.FlipAngle;
					character.GroundVelocity = -character.GroundVelocity;
				}
			}

			if ((mSubType & 0x0C) == 4) {
				character.Layer = 12;
				character.LayerPlus = 13;
			}

			if ((mSubType & 0x0C) == 8) {
				character.Layer = 14;
				character.LayerPlus = 15;
			}

			Level.AddSound(ResourceManager.BounceSound, DisplacementX, DisplacementY);
		}

		private void SpringHorizontally(Character character)
		{
			mMappingFrame = 0;
			mAnimationDuration = 10;

			if (!mFlipX) {
				character.VelocityX = -mUnk30;
				character.DisplacementX -= 8;
				character.Status &= ~CharacterState.FacingLeft;
			} else {
				character.VelocityX = mUnk30;
				character.DisplacementX += 8;
				character.Status |= CharacterState.FacingLeft;
			}

			character.MoveLock = 15;
			character.GroundVelocity = character.VelocityX;
			if ((character.Status & CharacterState.Spinning) == 0)
				character.Anim = 0;

			if (mSubType < 0)
				character.VelocityY = 0;

			if ((mSubType & 1) != 0) {
				character.GroundVelocity = 1;
				character.FlipAngle = 1;
				character.Anim = 0;
				character.FlipsRemaining = 1;
				character.FlipSpeed = 8;
				if ((mSubType & 2) == 0)
					character.FlipsRemaining = 3;
				if ((character.Status & CharacterState.FacingLeft) != 0) {
					character.FlipAngle = -character.FlipAngle;
					character.GroundVelocity = -character.GroundVelocity;
				}
			}

			if ((mSubType & 0x0C) == 4) {
				character.Layer = 12;
				character.LayerPlus = 13;
			}

			if ((mSubType & 0x0C) == 8) {
				character.Layer = 14;
				character.LayerPlus = 15;
			}

			CharacterTouchStatus[character] &= ~SolidObjectTouch.Pushing;
			character.Status &= ~CharacterState.Pushing;
			Level.AddSound(ResourceManager.BounceSound, DisplacementX, DisplacementY);
		}

		private void SpringDown(Character character)
		{
			mMappingFrame = 0;
			mAnimationDuration = 10;

			character.VelocityY = -mUnk30;
			character.DisplacementY -= 8;
			character.Status |= CharacterState.Airborne;
			character.Status &= ~CharacterState.OnObject;
			character.Anim = CharacterAnimation.Spring;
			character.Routine = 2;

			if (mSubType < 0)
				character.VelocityX = 0;

			if ((mSubType & 1) != 0) {
				character.GroundVelocity = 1;
				character.FlipAngle = 1;
				character.Anim = 0;
				character.FlipsRemaining = 0;
				character.FlipSpeed = 4;
				if ((mSubType & 2) == 0)
					character.FlipsRemaining = 1;
				if ((character.Status & CharacterState.FacingLeft) != 0) {
					character.FlipAngle = -character.FlipAngle;
					character.GroundVelocity = -character.GroundVelocity;
				}
			}

			if ((mSubType & 0x0C) == 4) {
				character.Layer = 12;
				character.LayerPlus = 13;
			}

			if ((mSubType & 0x0C) == 8) {
				character.Layer = 14;
				character.LayerPlus = 15;
			}

			Level.AddSound(ResourceManager.BounceSound, DisplacementX, DisplacementY);
		}

		private void SpringDiagonallyUp(Character character)
		{
			// Check if on the spring and not just the ledge
			if (!mFlipX) {
				if (character.DisplacementX <= DisplacementX - 4)
					return;
			} else {
				if (character.DisplacementX > DisplacementX + 4)
					return;
			}

			if (character.DisplacementY > DisplacementY + 4)
				return;

			mMappingFrame = 0;
			mAnimationDuration = 10;

			if (!mFlipX) {
				character.VelocityX = -mUnk30;
				character.DisplacementX -= 6;
				character.Status &= ~CharacterState.FacingLeft;
			} else {
				character.VelocityX = mUnk30;
				character.DisplacementX += 6;
				character.Status |= CharacterState.FacingLeft;
			}

			character.VelocityY = mUnk30;

			character.Status |= CharacterState.Airborne;
			character.Status &= ~CharacterState.OnObject;
			character.Anim = CharacterAnimation.Spring;
			character.Routine = 2;

			if ((mSubType & 1) != 0) {
				character.GroundVelocity = 1;
				character.FlipAngle = 1;
				character.Anim = 0;
				character.FlipsRemaining = 1;
				character.FlipSpeed = 8;
				if ((mSubType & 2) == 0)
					character.FlipsRemaining = 3;
				if ((character.Status & CharacterState.FacingLeft) != 0) {
					character.FlipAngle = -character.FlipAngle;
					character.GroundVelocity = -character.GroundVelocity;
				}
			}

			if ((mSubType & 0x0C) == 4) {
				character.Layer = 12;
				character.LayerPlus = 13;
			}

			if ((mSubType & 0x0C) == 8) {
				character.Layer = 14;
				character.LayerPlus = 15;
			}

			Level.AddSound(ResourceManager.BounceSound, DisplacementX, DisplacementY);
		}

		private void SpringDiagonallyDown(Character character)
		{

		}

		private void loc_18BC6()
		{

		}

		protected override SolidObjectTouch GetCharacterTouch(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			if ((currentTouchStatus & SolidObjectTouch.Standing) == 0)
				return loc_199F0(currentTouchStatus, character, width, jumpRadiusY, walkRadiusY, x);

			if ((character.Status & CharacterState.Airborne) == 0) {
				int d0 = character.DisplacementX - DisplacementX + width;
				if (d0 >= 0 && d0 < width * 2) {
					MoveCharacterOnPlatform(character, x, walkRadiusY);
					// d4 = 0;
					return currentTouchStatus;
				}
			}

			character.Status &= ~CharacterState.OnObject;
			character.Status |= CharacterState.Airborne;
			currentTouchStatus &= ~SolidObjectTouch.Standing;
			// d4 = 0;
			return currentTouchStatus;
		}

		public override int Id
		{
			get { return 65; }
		}
	}
}
