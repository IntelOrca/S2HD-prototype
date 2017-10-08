using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class Camera
	{
		private int mX;
		private int mY;
		// private int mPartialX;
		// private int mPartialY;
		private int mBiasY;
		private int mScrollDelayX;

		public Camera()
		{
		}

		public Rectangle GetViewBounds(Rectangle visibleRegion, int width, int height)
		{
			Rectangle cameraView = new Rectangle(mX - (width / 2), mY - (height / 2), width, height);

			// Ensure minimum position
			if (cameraView.X < visibleRegion.X)
				cameraView.X = visibleRegion.X;
			if (cameraView.Y < visibleRegion.Y)
				cameraView.Y = visibleRegion.Y;

			// Ensure maximum position
			if (cameraView.X + cameraView.Width > visibleRegion.X + visibleRegion.Width)
				cameraView.X = visibleRegion.X + visibleRegion.Width - width;
			if (cameraView.Y + cameraView.Height > visibleRegion.Y + visibleRegion.Height)
				cameraView.Y = visibleRegion.Y + visibleRegion.Height - height;

			// Check if still unhandled
			if (cameraView.Width > visibleRegion.Width)
				cameraView.X = -((visibleRegion.Width - cameraView.Width) / 2);
			if (cameraView.Height > visibleRegion.Height)
				cameraView.Y = -((visibleRegion.Height - cameraView.Height) / 2);

			return cameraView;
		}

		public int X
		{
			get
			{
				return mX;
			}
			set
			{
				mX = value;
			}
		}

		public int Y
		{
			get
			{
				return mY;
			}
			set
			{
				mY = value;
			}
		}

		public int BiasY
		{
			get
			{
				return mBiasY;
			}
			set
			{
				mBiasY = value;
			}
		}

		public int ScrollDelayX
		{
			get
			{
				return mScrollDelayX;
			}
			set
			{
				mScrollDelayX = value;
			}
		}
	}
}
