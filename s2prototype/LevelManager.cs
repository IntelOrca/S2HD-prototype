using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	class LevelManager
	{
		private SonicGame mGame;
		private Level mLevel;

		public LevelManager(SonicGame game)
		{
			mGame = game;
		}

		public void InitialiseLevel(int zone, int act)
		{
			mLevel = new Level(mGame);

			// Emerald Hill Zone, act 1
			mLevel.Load("data\\levels\\ehz");
		}
	}
}
