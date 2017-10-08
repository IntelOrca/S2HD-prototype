
namespace IntelOrca.Sonic
{
	abstract class GameScreen
	{
		private SonicGame mGame;

		public GameScreen(SonicGame game)
		{
			mGame = game;
		}

		public virtual void Update() { }
		public virtual void Draw(Graphics g) { }

		public SonicGame Game
		{
			get
			{
				return mGame;
			}
		}
	}
}
