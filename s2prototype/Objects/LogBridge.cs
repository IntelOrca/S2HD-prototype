using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class LogBridge : Platform
	{
		private int mStatus;
		private int[] mLogOffsets;
		private int[] mLogDestOffsets;
		private int[] mLogTotalOffsets;

		private int mSubType;

		public LogBridge(SonicGame game, Level level, LevelObjectDefinition definition)
			: base(game, level, definition)
		{
			mSubType = definition.SubType;
		}

		public override void Draw(Graphics g)
		{
			if (mStatus == 0)
				return;

			int bridgeWidth = mSubType * 16;
			DrawPillar(g, -(bridgeWidth / 2) - 16, -12);
			DrawPillar(g, (bridgeWidth / 2), -12);

			for (int i = 0; i < mSubType; i++)
				DrawLog(g, i);
		}

		private void DrawPillar(Graphics g, int x, int y)
		{
			Rectangle dst = new Rectangle((x - 8) * Game.DisplayScale, (y - 8) * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			Rectangle src = new Rectangle(0, 0, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			g.DrawImage(ResourceManager.LogBridgeTexture, dst, src, Color.White);
		}

		private void DrawLog(Graphics g, int index)
		{
			int x = - RadiusX + (index * 16);
			DrawLog(g, x, mLogOffsets[index]);
		}

		private void DrawLog(Graphics g, int x, int y)
		{
			Rectangle dst = new Rectangle((x - 8) * Game.DisplayScale, (y - 8) * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			Rectangle src = new Rectangle(16 * Game.DisplayScale, 0 * Game.DisplayScale, 16 * Game.DisplayScale, 16 * Game.DisplayScale);
			g.DrawImage(ResourceManager.LogBridgeTexture, dst, src, Color.White);
		}

		private void Init()
		{
			RadiusX = (mSubType * 16) / 2;
			RadiusY = 8;

			mLogOffsets = new int[mSubType];
			mLogDestOffsets = new int[mSubType];
			mLogTotalOffsets = new int[mSubType];

			int offset = 2;
			for (int i = 0; i < mSubType / 2; i++) {
				mLogTotalOffsets[i] = offset;
				mLogTotalOffsets[mSubType - i - 1] = offset;
				offset += 2;
			}

			if (mSubType % 2 == 1)
				mLogTotalOffsets[mSubType / 2] = 2 * (mSubType / 2) + 2;

			mStatus = 1;
		}

		public override void Update()
		{
			if (mStatus == 0)
				Init();

			// Reset log offsets
			for (int i = 0; i < mSubType; i++)
				mLogDestOffsets[i] = 0;

			// Get character closest to centre of bridge and X of that
			int closestX = -1;
			foreach (Character character in InteractingCharacters)
				if (Math.Abs(character.DisplacementX - DisplacementX) < closestX || closestX == -1)
					closestX = character.DisplacementX;

			int logIndex = GetLogIndex(closestX);
			if (logIndex != -1)
				DepressAt(logIndex);

			// Update log offsets
			for (int i = 0; i < mSubType; i++) {
				if (mLogOffsets[i] < mLogDestOffsets[i])
					mLogOffsets[i]++;
				else if (mLogOffsets[i] > mLogDestOffsets[i])
					mLogOffsets[i]--;
			}
		}

		public override void Interact(LevelObject obj)
		{
			base.Interact(obj);

			Character character = obj as Character;
			if (character == null)
				return;

			if (InteractingCharacters.Contains(character)) {
				if ((character.Status & CharacterState.Airborne) != 0)
					return;

				int logIndex = GetLogIndex(character.DisplacementX);
				if (logIndex != -1)
					character.DisplacementY = DisplacementY + mLogOffsets[logIndex] - RadiusY - character.RadiusY;
			}
		}

		private void DepressAt(int logIndex)
		{
			int lowestOffset = mLogTotalOffsets[logIndex];
			mLogDestOffsets[logIndex] = lowestOffset;

			int logsLeft = logIndex;
			float yInterval = lowestOffset / (logsLeft + 1.0f);
			float y = lowestOffset - yInterval;

			for (int i = logIndex - 1; i >= 0; i--) {
				mLogDestOffsets[i] = (int)y;
				y -= yInterval;
			}

			int logsRight = mSubType - logIndex - 1;
			yInterval = lowestOffset / (logsRight + 1.0f);
			y = lowestOffset - yInterval;

			for (int i = logIndex + 1; i < mSubType; i++) {
				mLogDestOffsets[i] = (int)y;
				y -= yInterval;
			}


			//int lowestOffset = mLogTotalOffsets[logIndex];
			//mLogOffsets[logIndex] = lowestOffset;

			//int logsLeft = logIndex;
			//double angleInterval = (Math.PI / 4) / (logsLeft + 1);
			//double angle = angleInterval;
			//for (int i = 0; i < logIndex; i++) {
			//	int offset = (int)(Math.Sin(angle) * lowestOffset);
			//	mLogOffsets[i] = offset;
			//	angle += angleInterval;
			//}

			//int logsRight = SubType - logIndex - 1;
			//angleInterval = (Math.PI / 4) / (logsRight + 1);
			//angle = (Math.PI / 4) - angleInterval;
			//for (int i = logIndex + 1; i < SubType; i++) {
			//	int offset = (int)(Math.Sin(angle) * lowestOffset);
			//	mLogOffsets[i] = offset;
			//	angle -= angleInterval;
			//}
		}

		private int GetLogIndex(int x)
		{
			int xoffset = x - (DisplacementX - RadiusX);
			int logIndex = xoffset / 16;
			if (logIndex >= 0 && logIndex < mSubType)
				return logIndex;
			else
				return -1;
		}

		public override int Id
		{
			get { return 17; }
		}
	}
}
