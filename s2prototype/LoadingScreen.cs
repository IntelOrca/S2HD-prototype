using System;
using System.Threading;
using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class LoadingScreen : GameScreen
	{
		private Action mLoadingRoutine;
		private Action mFinishedRoutine;
		private Thread mLoadingThread;

		private int mUpdateCount;

		public LoadingScreen(SonicGame game)
			: base(game)
		{
		}

		public override void Update()
		{
			if (mLoadingThread != null) {
				if (!mLoadingThread.IsAlive) {
					mLoadingThread = null;
					if (mFinishedRoutine != null)
						mFinishedRoutine.Invoke();
				}
			}

			mUpdateCount++;
		}

		public override void Draw(Graphics g)
		{
			int yellow;
			if (mUpdateCount % 64 >= 32)
				yellow = 255 - ((mUpdateCount % 32) * 4);
			else
				yellow = (mUpdateCount % 32) * 4;

			Color textColour = new Color(255, (byte)yellow, 0);
			ResourceManager.NormalFont.DrawString(g, "LOADING", 10, 10, textColour);
		}

		public void Begin()
		{
			if (mLoadingRoutine == null)
				return;

			mLoadingThread = new Thread(new ThreadStart(mLoadingRoutine));
			mLoadingThread.Name = "Loading thread";
			mLoadingThread.Priority = ThreadPriority.Highest;
			mLoadingThread.Start();
		}

		public Action LoadingRoutine
		{
			get
			{
				return mLoadingRoutine;
			}
			set
			{
				mLoadingRoutine = value;
			}
		}

		public Action FinishedRoutine
		{
			get
			{
				return mFinishedRoutine;
			}
			set
			{
				mFinishedRoutine = value;
			}
		}
	}
}
