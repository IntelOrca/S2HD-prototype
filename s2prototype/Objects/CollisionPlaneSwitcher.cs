using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	class CollisionPlaneSwitcher : LevelObject
	{
		private int mRoutine;
		private int mHeight;
		private int mSubType;
		private bool mFlipX;
		private Dictionary<Character, int> mCurrentSides = new Dictionary<Character, int>();

		private static int[] Heights = new int[] { 32, 64, 128, 256 };

		public CollisionPlaneSwitcher(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mSubType = definition.SubType;
			mFlipX = definition.FlipX;
		}

		public override void Update()
		{
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					MainX();
					break;
				case 4:
					MainY();
					break;
			}
		}

		private void Init()
		{
			if ((mSubType & 4) == 0)
				InitCheckX();
			else
				InitCheckY();
		}

		private void InitCheckX()
		{
			mRoutine = 2;
			mHeight = Heights[mSubType & 3];
			MainX();
		}

		private void InitCheckY()
		{
			mRoutine = 4;
			mHeight = Heights[mSubType & 3];
			MainY();
		}

		private void MainX()
		{
			CharacterCheck();

			foreach (KeyValuePair<Character, int> kvp in mCurrentSides.ToArray()) {
				if (kvp.Value == 0) {
					// Check if character is still left of the layer switcher
					if (DisplacementX > kvp.Key.DisplacementX)
						return;

					// Character is now right of the layer switcher
					mCurrentSides[kvp.Key] = 1;

					// Check if character is between the top and bottom of the layer switcher
					if (kvp.Key.DisplacementY < DisplacementY - mHeight)
						return;
					if (kvp.Key.DisplacementY >= DisplacementY + mHeight)
						return;

					// Make sure character isn't airborne
					if ((mSubType & 128) != 0)
						if ((kvp.Key.Status & CharacterState.Airborne) != 0)
							return;

					if (!mFlipX) {
						if ((mSubType & 8) != 0) {
							kvp.Key.Layer = 0;
						} else {
							kvp.Key.Layer = 1;
						}
					}
				} else {
					// Check if character is still right of the layer switcher
					if (DisplacementX < kvp.Key.DisplacementX)
						return;

					// Character is now left of the layer switcher
					mCurrentSides[kvp.Key] = 0;

					int top = DisplacementY - mHeight;
					int bottom = DisplacementY + mHeight;
					if (kvp.Key.DisplacementY < top)
						return;
					if (kvp.Key.DisplacementY >= bottom)
						return;

					// Make sure character isn't airborne
					if ((mSubType & 128) != 0)
						if ((kvp.Key.Status & CharacterState.Airborne) != 0)
							return;

					if (!mFlipX) {
						if ((mSubType & 16) != 0) {
							kvp.Key.Layer = 0;
						} else {
							kvp.Key.Layer = 1;
						}
					}
				}
			}
		}

		private void MainY()
		{

		}

		private void CharacterCheck()
		{
			foreach (LevelObject obj in Level.Objects) {
				Character character = obj as Character;
				if (character == null)
					continue;

				if (!mCurrentSides.ContainsKey(character)) {
					if (DisplacementX < character.DisplacementX)
						mCurrentSides.Add(character, 1);
					else
						mCurrentSides.Add(character, 0);
				}
			}

		}

		public override int Id
		{
			get { return 3; }
		}
	}
}
