using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class LevelScreen : GameScreen
	{
		private PlayerView mPlayerView;

		public LevelScreen(SonicGame game)
			: base(game)
		{
			mPlayerView = new PlayerView(game, game.Players[0]);
			mPlayerView.Bounds = new Rectangle(0, 0, game.DisplayWidth, game.DisplayHeight);
		}

		public override void Update()
		{
			mPlayerView.Update();
		}

		public override void Draw(Graphics g)
		{
			mPlayerView.Draw(g);
		}
	}
}
