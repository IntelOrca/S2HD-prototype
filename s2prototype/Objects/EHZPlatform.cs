using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class EHZPlatform : Platform
	{
		private int mStatus;
		private int mInitialDisplacementX;
		private int mInitialDisplacementY;
		private int mSubType;

		public EHZPlatform(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			OscillateNumInit();

			RadiusX = 32;
			RadiusY = 8;

			mSubType = definition.SubType;
		}

		public override void Draw(Graphics g)
		{
			g.DrawImage(ResourceManager.EHZPlatformTexture, new Vector2(-32 * Game.DisplayScale, -12 * Game.DisplayScale), Color.White);
			// sb.Draw(ResourceManager.MarkerTexture, new Vector2(DisplacementX - 7 - Game.Camera.X, DisplacementY - 7 - Game.Camera.Y), Color.White);
		}

		public override void Update()
		{
			if (mStatus == 0) {
				mInitialDisplacementX = DisplacementX;
				mInitialDisplacementY = DisplacementY;
				mStatus = 1;
				return;
			}

			OscillateNumDo();

			if (mSubType == 1) {
				int targetX = mInitialDisplacementX + ((mOscillationData[18] >> 8) - 64);
				int targetY;
				VelocityX = (targetX - DisplacementX) * 256;

				if (InteractingCharacters.Count > 0)
					targetY = mInitialDisplacementY + 4;
				else
					targetY = mInitialDisplacementY;

				if (targetY < DisplacementY)
					VelocityY = -84;
				else if (targetY > DisplacementY)
					VelocityY = 84;
				else
					VelocityY = 0;
			} else if (mSubType == 2) {
				int targetY = mInitialDisplacementY + ((mOscillationData[18] >> 8) - 64);
				VelocityY = (targetY - DisplacementY) * 256;
			}

			UpdatePosition();
		}

		public override int Id
		{
			get { return 24; }
		}

		public override bool AllowBalancing
		{
			get
			{
				return true;
			}
		}

		private void OscillateNumInit()
		{
			Array.Copy(OscData, mOscillationData, OscData.Length);
		}

		private void OscillateNumDo()
		{
			ushort newValue;
			int numElements;

			numElements = OscData2.Length / 2;

			for (int i = 0; i < numElements; i++) {
				int j = i * 2;
				int k = i * 3;

				if (mOscillationData[k + 2] == 0) {
					newValue = (ushort)(mOscillationData[k + 1] + OscData2[j]);
					mOscillationData[k] += newValue;
					mOscillationData[k + 1] = newValue;
					if ((OscData2[j + 1] & 0xFF) <= (mOscillationData[k] >> 8))
						mOscillationData[k + 2] = 1;
				} else {
					newValue = (ushort)(mOscillationData[k + 1] - OscData2[j]);
					mOscillationData[k] += newValue;
					mOscillationData[k + 1] = newValue;
					if ((OscData2[j + 1] & 0xFF) > (mOscillationData[k] >> 8))
						mOscillationData[k + 2] = 0;
				}
			}
		}

		private ushort[] mOscillationData = new ushort[16 * 3];
		private static ushort[] OscData = new ushort[] {
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x80, 0, 0,
			0x3848, 0xEE, 1,
			0x2080, 0xB4, 1,
			0x3080, 0x10E, 1,
			0x5080, 0x1C2, 1,
			0x7080, 0x276, 1,
			0x80, 0, 0,
			0x4000, 0xFE, 1,
		};

		private static ushort[] OscData2 = new ushort[] {
			2, 0x10,
			2, 0x18,
			2, 0x20,
			2, 0x30,
			4, 0x20,
			8, 8,
			8, 0x40,
			4, 0x40,
			2, 0x38,
			2, 0x28,
			2, 0x20,
			3, 0x30,
			5, 0x50,
			7, 0x70,
			2, 0x40,
			2, 0x40,
		};
	}
}
