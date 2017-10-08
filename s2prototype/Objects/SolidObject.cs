using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	[Flags]
	enum SolidObjectTouch
	{
		NoTouch = 0,
		Standing = 1,
		Pushing = 2,
		Top = 4,
		Bottom = 8,
		Side = 16,
	}

	abstract class SolidObject : LevelObject
	{
		private Dictionary<Character, SolidObjectTouch> mCharacterTouchStatus = new Dictionary<Character, SolidObjectTouch>();

		public SolidObject(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
		}

		protected void ClearCharacterTouchStatus()
		{
			foreach (KeyValuePair<Character, SolidObjectTouch> kvp in mCharacterTouchStatus) {
				if ((kvp.Value & SolidObjectTouch.Standing) != 0) {
					kvp.Key.Status |= CharacterState.Airborne;
					kvp.Key.Status &= ~CharacterState.OnObject;
				}
			}
		}

		protected void DoCharacterTouchResponse(int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			foreach (LevelObject obj in Level.Objects) {
				Character character = obj as Character;
				if (character != null)
					DoCharacterTouchResponse(character, width, jumpRadiusY, walkRadiusY, x);
			}
		}

		protected void DoCharacterTouchResponse(Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			SolidObjectTouch touchStatus = SolidObjectTouch.NoTouch;
			if (mCharacterTouchStatus.ContainsKey(character))
				touchStatus = mCharacterTouchStatus[character];

			// Clear these as these will be re-worked out if necessary
			touchStatus &= ~SolidObjectTouch.Top;
			touchStatus &= ~SolidObjectTouch.Bottom;
			touchStatus &= ~SolidObjectTouch.Side;
				
			mCharacterTouchStatus[character] = GetCharacterTouch(touchStatus, character, width, jumpRadiusY, walkRadiusY, x);
		}

		protected virtual SolidObjectTouch GetCharacterTouch(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			int d0, d2;

			if ((currentTouchStatus & SolidObjectTouch.Standing) != 0) {
				d2 = width * 2;
				if ((character.Status & CharacterState.Airborne) != 0) {
					character.Status &= ~CharacterState.OnObject;
					currentTouchStatus &= ~SolidObjectTouch.Standing;
					// d4 = 0;
					return currentTouchStatus;
				}

				d0 = character.DisplacementX - DisplacementX + width;
				if (d0 < 0) {
					character.Status |= CharacterState.Airborne;
					character.Status &= ~CharacterState.OnObject;
					currentTouchStatus &= ~SolidObjectTouch.Standing;
					// d4 = 0;
					return currentTouchStatus;
				}

				if (d0 < d2) {
					MoveCharacterOnPlatform(character, x, walkRadiusY);
					// d4 = 0;
					return currentTouchStatus;
				}
			}

			// SolidObject_cont:

			return loc_199F0(currentTouchStatus, character, width, jumpRadiusY, walkRadiusY, x);
		}

		protected SolidObjectTouch loc_199F0(SolidObjectTouch currentTouchStatus, Character character, int width, int jumpRadiusY, int walkRadiusY, int x)
		{
			int d0, d1, d3, d4, d5;

			d0 = character.DisplacementX - DisplacementX + width;
			if (d0 < 0)
				return loc_19AC4(currentTouchStatus, character);

			d3 = width * 2;
			if (d0 > d3)
				return loc_19AC4(currentTouchStatus, character);

			jumpRadiusY += character.RadiusY;
			d3 = character.DisplacementY - DisplacementY + 4 + jumpRadiusY;
			if (d3 < 0)
				return loc_19AC4(currentTouchStatus, character);

			d3 &= 0x7FF;
			d4 = jumpRadiusY * 2;
			if (d3 >= d4)
				return loc_19AC4(currentTouchStatus, character);


			// loc_19A2E:
			if (character.ObjectControl < 0)
				return loc_19AC4(currentTouchStatus, character);

			if (character.Routine >= 6) {
				// d4 = 0;
				return currentTouchStatus;
			}

			d5 = d0;
			if (width < d0) {
				width *= 2;
				d0 -= width;
				d5 = -d0;
			}

			width = d3;
			if (jumpRadiusY < d3) {
				d3 -= 4;
				d3 -= d4;
				width = -d3;
			}

			if (d5 > width) {
				if (d3 < 0) {
					if (character.VelocityY == 0) {
						if ((character.Status & CharacterState.Airborne) != 0) {
							currentTouchStatus |= SolidObjectTouch.Bottom;
							// d4 = -2
							return currentTouchStatus;
						}

						d0 = Math.Abs(d4);
						if (d4 < 16)
							return loc_19A6A(currentTouchStatus, character, width, d0);

						character.Kill();
						currentTouchStatus |= SolidObjectTouch.Bottom;
						// d4 = -2
						return currentTouchStatus;
					}

					if (character.VelocityY < 0 && d3 < 0) {
						character.DisplacementY -= d3;
						character.VelocityY = 0;
					}

					currentTouchStatus |= SolidObjectTouch.Bottom;
					// d4 = -2
					return currentTouchStatus;
				}

				if (d3 < 16) {
					d3 -= 4;
					d1 = RadiusX + character.DisplacementX - DisplacementX;		// width_pixels
					if (d1 < 0) {
						// d4 = 0;
						return currentTouchStatus;
					}

					if (d1 >= RadiusX * 2) {									// width_pixels
						// d4 = 0;
						return currentTouchStatus;
					}

					if (character.VelocityY < 0) {
						// d4 = 0;
						return currentTouchStatus;
					}

					character.DisplacementY -= d3 - 1;
					currentTouchStatus = loc_19E14(currentTouchStatus, character);
					currentTouchStatus |= SolidObjectTouch.Top;
					d4 = -1;
					return currentTouchStatus;
				}

				// if (id == LauncherSpring) {
				//     if (d3 < 20)
				//         goto loc_19B56
				// }

				return loc_19AC4(currentTouchStatus, character);
			}

			return loc_19A6A(currentTouchStatus, character, width, d0);
		}

		private SolidObjectTouch loc_19A6A(SolidObjectTouch currentTouchStatus, Character character, int width, int d0)
		{
			if (width <= 4)
				return loc_19AB6(currentTouchStatus, character);

			if (d0 == 0)
				return loc_19A90(currentTouchStatus, character, d0);

			if (d0 < 0) {
				if (character.VelocityX >= 0)
					return loc_19A90(currentTouchStatus, character, d0);
			} else {
				if (character.VelocityX < 0)
					return loc_19A90(currentTouchStatus, character, d0);
			}

			character.GroundVelocity = 0;
			character.VelocityX = 0;

			return loc_19A90(currentTouchStatus, character, d0);
		}

		private SolidObjectTouch loc_19AB6(SolidObjectTouch currentTouchStatus, Character character)
		{
			currentTouchStatus &= ~SolidObjectTouch.Pushing;
			character.Status &= ~CharacterState.Pushing;
			currentTouchStatus |= SolidObjectTouch.Side;
			// d4 = 1;
			return currentTouchStatus;
		}

		private SolidObjectTouch loc_19A90(SolidObjectTouch currentTouchStatus, Character character, int d0)
		{
			character.DisplacementX -= d0;
			if ((character.Status & CharacterState.Airborne) != 0)
				return loc_19AB6(currentTouchStatus, character);

			currentTouchStatus |= SolidObjectTouch.Pushing;
			currentTouchStatus |= SolidObjectTouch.Side;
			character.Status |= CharacterState.Pushing;
			// d4 = 1;
			return currentTouchStatus;
		}

		private SolidObjectTouch loc_19AC4(SolidObjectTouch currentTouchStatus, Character character)
		{
			if ((currentTouchStatus & SolidObjectTouch.Pushing) == 0) {
				// d4 = 0;
				return currentTouchStatus;
			}

			// Is character not rolling?
			if (character.Anim != CharacterAnimation.Roll) {
				// Set character animation to running
				character.Anim = CharacterAnimation.Run;
			}

			currentTouchStatus &= ~SolidObjectTouch.Pushing;
			character.Status &= ~CharacterState.Pushing;
			// d4 = 0;
			return currentTouchStatus;
		}

		private SolidObjectTouch loc_19E14(SolidObjectTouch currentTouchStatus, Character character)
		{
			if ((character.Status & CharacterState.OnObject) != 0) {
				// character.InteractionObject
			}

			character.InteractionObject = this;
			character.Angle = 0;
			character.VelocityY = 0;
			character.GroundVelocity = character.VelocityX;
			if ((character.Status & CharacterState.Airborne) != 0)
				character.ResetOnFloor();

			character.Status |= CharacterState.OnObject;
			character.Status &= ~CharacterState.Airborne;
			currentTouchStatus |= SolidObjectTouch.Standing;
			return currentTouchStatus;
		}

		protected void MoveCharacterOnPlatform(Character character, int x, int radiusY)
		{
			if (character.ObjectControl < 0)
				return;
			if (character.Routine >= 6)
				return;
			character.DisplacementY = DisplacementY - radiusY - character.RadiusY;
			character.DisplacementX -= DisplacementX - x;
		}

		protected Dictionary<Character, SolidObjectTouch> CharacterTouchStatus
		{
			get
			{
				return mCharacterTouchStatus;
			}
			set
			{
				mCharacterTouchStatus = value;
			}
		}
	}
}
