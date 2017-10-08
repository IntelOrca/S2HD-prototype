using System.Collections.Generic;

namespace IntelOrca.Sonic
{
	abstract class Platform : LevelObject
	{
		private List<Character> mInteractingCharacters = new List<Character>();
		private int mDiffX;
		private int mDiffY;

		public Platform(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
		}

		public override void Touch(LevelObject obj)
		{
			Character character = obj as Character;
			if (character == null)
				return;

			if (mInteractingCharacters.Contains(character))
				return;

			if (character.VelocityY < 0)
				return;

			// Top
			if (character.DisplacementY + character.RadiusY > DisplacementY - RadiusY &&
				character.DisplacementY + character.RadiusY < DisplacementY) {
				LandCharacter(character);
			}
		}

		public override void Interact(LevelObject obj)
		{
			Character character = obj as Character;
			if (character == null)
				return;

			// Check if in the air
			if ((character.Status & CharacterState.Airborne) == 0) {
				// Check if still on object
				int d0 = character.DisplacementX - DisplacementX + character.RadiusX + RadiusX;
				if (d0 >= 0 && d0 < (character.RadiusX + RadiusX) * 2) {
					UpdateCharacter(character);
					return;
				}
			}

			UnlandCharacter(character);
		}

		protected override void UpdatePosition()
		{
			mDiffX = DisplacementX;
			mDiffY = DisplacementY;
			base.UpdatePosition();
			mDiffX = DisplacementX - mDiffX;
			mDiffY = DisplacementY - mDiffY;
		}

		protected void UpdateCharacter(Character character)
		{
			character.DisplacementX += mDiffX;
			character.DisplacementY += mDiffY;
		}

		protected void LandCharacter(Character character)
		{
			// character.ResetOnFloor();
			character.VelocityY = 0;
			character.DisplacementY = DisplacementY - RadiusY - character.RadiusY;
			character.GroundVelocity = character.VelocityX;

			if ((character.Status & CharacterState.Airborne) != 0)
				character.ResetOnFloor();

			character.Status &= ~CharacterState.Airborne;
			character.Status |= CharacterState.OnObject;
			character.InteractionObject = this;
			mInteractingCharacters.Add(character);
		}

		protected void UnlandCharacter(Character character)
		{
			character.InteractionObject = null;
			character.Status &= ~CharacterState.OnObject;
			mInteractingCharacters.Remove(character);
		}

		protected List<Character> InteractingCharacters
		{
			get
			{
				return mInteractingCharacters;
			}
		}
	}
}
