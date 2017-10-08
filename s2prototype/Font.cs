using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	class Font
	{
		private Texture2D mTexture;

		private char[] mCharacters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ':' };
		private int[] mCharacterMapX = new int[] { 3, 13, 23, 33, 43, 53, 63, 73, 83, 93, 3, 13, 23, 33, 43, 53, 63, 73, 86, 93, 103, 113, 122, 3, 13, 23, 33, 43, 53, 63, 73, 83, 93, 103, 113, 122, 103 };
		private int[] mCharacterMapY = new int[] { 379, 379, 379, 379, 379, 379, 379, 379, 379, 379, 395, 395, 395, 395, 395, 395, 395, 395, 395, 395, 395, 395, 395, 407, 407, 407, 407, 407, 407, 407, 407, 407, 407, 407, 407, 407, 379 };
		private int[] mCharacterMapW = new int[] { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 3, 7, 8, 6, 10, 9, 7, 7, 7, 7, 7, 7, 7, 7, 10, 8, 7, 8, 7 };
		private int mHeight = 11;

		private int mLetterSpacing = 1;

		public Font(Texture2D texture)
		{
			mTexture = texture;
		}

		public int MeasureStringWidth(string text)
		{
			int x = 0;
			foreach (char c in text) {
				int charMapIndex = GetCharacterMapIndex(c);
				if (charMapIndex == -1) {
					x += 7 + mLetterSpacing;
					continue;
				}

				x += (mCharacterMapW[charMapIndex] * 4) + (mLetterSpacing * 4);
			}
			return x;
		}

		public void DrawString(Graphics g, string text, int x, int y)
		{
			DrawString(g, text, x, y, Color.White);
		}

		public void DrawString(Graphics g, string text, int x, int y, Color colour)
		{
			foreach (char c in text) {
				int charMapIndex = GetCharacterMapIndex(c);
				if (charMapIndex == -1) {
					x += 7 + mLetterSpacing;
					continue;
				}

				DrawCharacter(g, charMapIndex, x, y, colour);
				x += (mCharacterMapW[charMapIndex] * 4) + (mLetterSpacing * 4);
			}
		}

		private void DrawCharacter(Graphics g, int index, int x, int y, Color colour)
		{
			Rectangle src = new Rectangle(mCharacterMapX[index], mCharacterMapY[index], mCharacterMapW[index], mHeight);
			Rectangle dst = new Rectangle(x, y, mCharacterMapW[index] * 4, mHeight * 4);
			g.DrawImage(mTexture, dst, src, colour);
		}

		private int GetCharacterMapIndex(char c)
		{
			for (int i = 0; i < mCharacters.Length; i++)
				if (mCharacters[i] == c)
					return i;
			return -1;
		}
	}
}
