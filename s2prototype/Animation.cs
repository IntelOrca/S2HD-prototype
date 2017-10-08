
namespace IntelOrca.Sonic
{
	class Animation
	{
		private byte[][] mScripts;
		private int mIndex;
		private int mNextIndex;
		private int mFrame;
		private int mFrameDuration;
		private int mFrameValue;

		public Animation()
		{
		}

		public Animation(byte[][] scripts)
		{
			mScripts = scripts;
		}
		
		public void Update()
		{
			if (mIndex != mNextIndex) {
				mNextIndex = mIndex;
				mFrame = 0;
				mFrameDuration = 0;
			}

			if (mScripts == null)
				return;

			if (mIndex >= mScripts.Length)
				return;

			byte[] animationScript = mScripts[mIndex];
			mFrameDuration--;
			if (mFrameDuration >= 0)
				return;

			mFrameDuration = animationScript[0];
			ProcessScript(animationScript);
		}

		private void ProcessScript(byte[] animationScript)
		{
			byte asf = animationScript[mFrame + 1];
			switch (asf) {
				case 0xFF:
					mFrameValue = animationScript[1];
					mFrame = 0;
					break;
				case 0xFE:
					mFrame -= animationScript[mFrame + 2];
					mFrameValue = animationScript[mFrame + 1];
					mFrame++;
					break;
				case 0xFD:
					mIndex = animationScript[mFrame + 2];
					break;
				default:
					mFrameValue = asf;
					mFrame++;
					break;
			}
		}

		public byte[][] Scripts
		{
			get
			{
				return mScripts;
			}
			set
			{
				mScripts = value;
			}
		}

		public int Index
		{
			get
			{
				return mIndex;
			}
			set
			{
				mIndex = value;
			}
		}

		public int NextIndex
		{
			get
			{
				return mNextIndex;
			}
			set
			{
				mNextIndex = value;
			}
		}

		public int Frame
		{
			get
			{
				return mFrame;
			}
			set
			{
				mFrame = value;
			}
		}

		public int FrameDuration
		{
			get
			{
				return mFrameDuration;
			}
			set
			{
				mFrameDuration = value;
			}
		}

		public int FrameValue
		{
			get
			{
				return mFrameValue;
			}
			set
			{
				mFrameValue = value;
			}
		}
	}
}
