
namespace IntelOrca.Sonic
{
	enum PlayerStatus
	{
		NotReady,
		Playing,
		Finished,
		Dead,
	}

	class Player
	{
		private SonicGame mGame;
		private Character mMainCharacter;
		private Character mSideKick;
		private int mScore;
		private int mTime;
		private int mRings;
		private int mLifeCount;
		private int mNextExtraLifeScore = 5000;
		private int mNextExtraLifeRings = 100;
		private int mChainBonusCounter;
		private PlayerStatus mStatus;

		private int mLastStarpostTime;
		private int mLastStarpostIndex;

		private MusicManager mMusicManager = new MusicManager();

		public Player(SonicGame game)
		{
			mGame = game;
		}

		public void Update()
		{
			if (mStatus == PlayerStatus.Playing)
				mTime++;
			mMainCharacter.ControllerState = mGame.ControllerA;

			mMusicManager.Update();
		}

		public void AddPoints(int points)
		{
			mScore += points;
			if (mScore > 9999999)
				mScore = 9999999;
			if (mScore < mNextExtraLifeScore)
				return;
			mNextExtraLifeScore += 50000;
			AddLife();
		}

		public void AddRings(int rings)
		{
			mRings += rings;

			if (mRings > 999)
				mRings = 999;

			if (mRings < mNextExtraLifeRings)
				return;

			mNextExtraLifeRings += 100;
			AddLife();
		}

		public void AddLife()
		{
			mLifeCount++;

			mMusicManager.PlayJingle(ResourceManager.LifeMusic);
		}

		public void Restart()
		{
			mStatus = PlayerStatus.NotReady;
			mRings = 0;
			mTime = mLastStarpostTime;
		}

		public void SetupCharacter()
		{
			mMainCharacter = new Sonic(mGame, mGame.Level);
			mMainCharacter.Player = this;

			if (mLastStarpostIndex == 0) {
				mMainCharacter.DisplacementX = mGame.Level.StartX;
				mMainCharacter.DisplacementY = mGame.Level.StartY;
			} else {
				bool mFoundStarpost = false;
				foreach (LevelObject obj in mGame.Level.Objects) {
					if (!(obj is Starpost))
						continue;

					Starpost starpost = (Starpost)obj;
					if (starpost.Index != mLastStarpostIndex)
						continue;

					mMainCharacter.DisplacementX = starpost.DisplacementX;
					mMainCharacter.DisplacementY = starpost.DisplacementY;
					mFoundStarpost = true;
					break;
				}

				if (!mFoundStarpost) {
					mMainCharacter.DisplacementX = mGame.Level.StartX;
					mMainCharacter.DisplacementY = mGame.Level.StartY;
				}
			}
		}

		public void LoseRings()
		{
			mRings = 0;
			mNextExtraLifeRings = 100;
		}

		public Character MainCharacter
		{
			get
			{
				return mMainCharacter;
			}
			set
			{
				mMainCharacter = value;
			}
		}

		public Character SideKick
		{
			get
			{
				return mSideKick;
			}
			set
			{
				mSideKick = value;
			}
		}

		public int Score
		{
			get
			{
				return mScore;
			}
			set
			{
				mScore = value;
			}
		}

		public int Time
		{
			get
			{
				return mTime;
			}
			set
			{
				mTime = value;
			}
		}

		public int Rings
		{
			get
			{
				return mRings;
			}
			set
			{
				mRings = value;
			}
		}

		public int LifeCount
		{
			get
			{
				return mLifeCount;
			}
			set
			{
				mLifeCount = value;
			}
		}

		public int NextExtraLifeScore
		{
			get
			{
				return mNextExtraLifeScore;
			}
			set
			{
				mNextExtraLifeScore = value;
			}
		}

		public int ChainBonusCounter
		{
			get
			{
				return mChainBonusCounter;
			}
			set
			{
				mChainBonusCounter = value;
			}
		}

		public PlayerStatus Status
		{
			get
			{
				return mStatus;
			}
			set
			{
				mStatus = value;
			}
		}

		public MusicManager MusicManager
		{
			get
			{
				return mMusicManager;
			}
		}

		public int LastStarpostIndex
		{
			get
			{
				return mLastStarpostIndex;
			}
			set
			{
				mLastStarpostIndex = value;
			}
		}

		public int LastStarpostTime
		{
			get
			{
				return mLastStarpostTime;				
			}
			set
			{
				mLastStarpostTime = value;
			}
		}
	}
}
