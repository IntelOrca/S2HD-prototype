using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	class LevelObjectDefinition
	{
		private int mId;
		private int mSubType;
		private int mDisplacementX;
		private int mDisplacementY;
		private bool mRespawn;
		private bool mFlipX;
		private bool mFlipY;

		public int Id
		{
			get
			{
				return mId;
			}
			set
			{
				mId = value;
			}
		}

		public int SubType
		{
			get
			{
				return mSubType;
			}
			set
			{
				mSubType = value;
			}
		}

		public int DisplacementX
		{
			get
			{
				return mDisplacementX;
			}
			set
			{
				mDisplacementX = value;
			}
		}

		public int DisplacementY
		{
			get
			{
				return mDisplacementY;
			}
			set
			{
				mDisplacementY = value;
			}
		}

		public bool Respawn
		{
			get
			{
				return mRespawn;
			}
			set
			{
				mRespawn = value;
			}
		}

		public bool FlipX
		{
			get
			{
				return mFlipX;
			}
			set
			{
				mFlipX = value;
			}
		}

		public bool FlipY
		{
			get
			{
				return mFlipY;
			}
			set
			{
				mFlipY = value;
			}
		}
	}
}
