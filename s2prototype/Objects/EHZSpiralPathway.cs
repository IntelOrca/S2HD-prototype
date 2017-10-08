using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	class EHZSpiralPathway : LevelObject
	{
		#region Constant data

		private static int[] FlipAngleTable = new int[] {
			0x00, 0x00, 0x01, 0x01,
			0x16, 0x16, 0x16, 0x16,
			0x2C, 0x2C, 0x2C, 0x2C,
			0x42, 0x42, 0x42, 0x42,
			0x58, 0x58, 0x58, 0x58,
			0x6E, 0x6E, 0x6E, 0x6E,
			0x84, 0x84, 0x84, 0x84,
			0x9A, 0x9A, 0x9A, 0x9A,
			0xB0, 0xB0, 0xB0, 0xB0,
			0xC6, 0xC6, 0xC6, 0xC6,
			0xDC, 0xDC, 0xDC, 0xDC,
			0xF2, 0xF2, 0xF2, 0xF2,
			0x01, 0x01, 0x00, 0x00,
		};

		private static int[] CosineTable = new int[] {
			32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
			32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 31, 31,
			31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 30, 30, 30,
			30, 30, 30, 30, 30, 30, 29, 29, 29, 29, 29, 28, 28, 28, 28, 27,
			27, 27, 27, 26, 26, 26, 25, 25, 25, 24, 24, 24, 23, 23, 22, 22,
			21, 21, 20, 20, 19, 18, 18, 17, 16, 16, 15, 14, 14, 13, 12, 12,
			11, 10, 10, 9, 8, 8, 7, 6, 6, 5, 4, 4, 3, 2, 2, 1,
			0, -1, -2, -2, -3, -4, -4, -5, -6, -7, -7, -8, -9, -9,-10,-10,
			-11,-11,-12,-12,-13,-14,-14,-15,-15,-16,-16,-17,-17,-18,-18,-19,
			-19,-19,-20,-21,-21,-22,-22,-23,-23,-24,-24,-25,-25,-26,-26,-27,
			-27,-28,-28,-28,-29,-29,-30,-30,-30,-31,-31,-31,-32,-32,-32,-33,
			-33,-33,-33,-34,-34,-34,-35,-35,-35,-35,-35,-35,-35,-35,-36,-36,
			-36,-36,-36,-36,-36,-36,-36,-37,-37,-37,-37,-37,-37,-37,-37,-37,
			-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,-37,
			-37,-37,-37,-37,-36,-36,-36,-36,-36,-36,-36,-35,-35,-35,-35,-35,
			-35,-35,-35,-34,-34,-34,-33,-33,-33,-33,-32,-32,-32,-31,-31,-31,
			-30,-30,-30,-29,-29,-28,-28,-28,-27,-27,-26,-26,-25,-25,-24,-24,
			-23,-23,-22,-22,-21,-21,-20,-19,-19,-18,-18,-17,-16,-16,-15,-14,
			-14,-13,-12,-11,-11,-10, -9, -8, -7, -7, -6, -5, -4, -3, -2, -1,
			0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 9, 10, 10, 11, 12, 13,
			13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21,
			21, 22, 22, 23, 23, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26,
			27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29,
			29, 30, 30, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31, 31, 31, 31,
			31, 31, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
			32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
		};

		#endregion

		public EHZSpiralPathway(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
		}

		public override void Update()
		{
			foreach (LevelObject obj in Level.Objects) {
				Character character = obj as Character;
				if (character == null)
					continue;

				UpdateCharacter(character);
			}
		}

		private void UpdateCharacter(Character character)
		{
			if (character.InteractionObject == this) {
				CharacterContinueProgress(character);
				return;
			}

			// Ignore if character is airborne
			if ((character.Status & CharacterState.Airborne) != 0)
				return;

			// Check if character is on object
			int x = character.DisplacementX - DisplacementX;
			if (character.VelocityX < 0) {
				if (x < 192)
					return;
				if (x > 208)
					return;
			} else {
				if (x > -192)
					return;
				if (x < -208)
					return;
			}

			int y = character.DisplacementY - DisplacementY - 10;
			if (y >= 48)
				return;

			CharacterStart(character);
		}

		private void CharacterStart(Character character)
		{
			character.InteractionObject = this;
			character.Angle = 0;
			character.VelocityY = 0;
			character.GroundVelocity = character.VelocityX;
			if ((character.Status & CharacterState.Airborne) != 0) {
				character.ResetOnFloor();
			}

			character.Status |= CharacterState.OnObject;
			character.Status &= ~CharacterState.Airborne;
		}

		private void CharacterContinueProgress(Character character)
		{
			int d0;

			if (Math.Abs(character.GroundVelocity) < 1536) {
				CharacterFallOff(character);
				return;
			}

			if ((character.Status & CharacterState.Airborne) != 0) {
				CharacterFallOff(character);
				return;
			}

			d0 = character.DisplacementX - DisplacementX + 208;
			if (d0 < 0) {
				CharacterFallOff(character);
				return;
			}

			if (d0 >= 416) {
				CharacterFallOff(character);
				return;
			}

			CharacterMove(character);
		}

		private void CharacterFallOff(Character character)
		{
			character.InteractionObject = null;
			character.Status &= ~CharacterState.OnObject;
			character.FlipsRemaining = 0;
			character.FlipSpeed = 4;
		}

		private void CharacterMove(Character character)
		{
			if ((character.Status & CharacterState.OnObject) == 0)
				return;

			int d0 = character.DisplacementX - DisplacementX + 208;
			character.DisplacementY = DisplacementY + CosineTable[d0] - character.RadiusY + 19;
			character.FlipAngle = FlipAngleTable[(d0 >> 3) & 63];
		}

		public override int Id
		{
			get { return 6; }
		}
	}
}
