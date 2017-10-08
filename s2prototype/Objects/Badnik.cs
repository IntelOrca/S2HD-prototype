using System;

namespace IntelOrca.Sonic
{
	abstract class Badnik : LevelObject
	{
		public Badnik(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
		}

		public override void Touch(LevelObject obj)
		{
			Character character = obj as Character;
			if (character == null)
				return;

			// Is character is not invincible
			if ((character.StatusSecondary & 2) == 0) {
				if (character.Anim != CharacterAnimation.Spindash) {
					if (character.Anim != CharacterAnimation.Roll) {
						character.Hurt(this);
						return;
					}
				}
			}

			int[] ChainPoints = new int[] { 100, 200, 500, 1000 };
			int points = ChainPoints[Math.Min(character.Player.ChainBonusCounter, 3)];
			if (character.Player.ChainBonusCounter >= 16)
				points = 10000;

			character.Player.AddPoints(points);
			character.Player.ChainBonusCounter++;

			// Add animal
			Animal animalObject = new Animal(Game, Level);
			animalObject.DisplacementX = DisplacementX;
			animalObject.DisplacementY = DisplacementY;
			Level.Objects.Add(animalObject);

			// Add explosion object
			Explosion explosionObject = new Explosion(Game, Level);
			explosionObject.DisplacementX = DisplacementX;
			explosionObject.DisplacementY = DisplacementY;
			Level.Objects.Add(explosionObject);

			// This object is now finished
			Finished = true;

			// Character hit enemy from below
			if (character.VelocityY < 0) {
				character.VelocityY += 256;
				return;
			}

			// Character is below enemy but falling down
			if (character.DisplacementY >= DisplacementY) {
				character.VelocityY -= 256;
				return;
			}

			// Rebound the character 1 : 1
			character.VelocityY = -character.VelocityY;
		}
	}
}
