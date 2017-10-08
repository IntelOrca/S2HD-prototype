using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	struct ControllerState
	{
		private int mButtons;

		public ControllerState GetNewDown(ControllerState mLastControlState)
		{
			ControllerState state = new ControllerState();
			state.mButtons = mButtons & ~mLastControlState.mButtons;
			return state;
		}

		public void Set(int alterMask, bool state)
		{
			if (state)
				mButtons |= alterMask;
			else
				mButtons &= ~alterMask;
		}

		public bool Up
		{
			get
			{
				return ((mButtons & 1) != 0);
			}
		}

		public bool Down
		{
			get
			{
				return ((mButtons & 2) != 0);
			}
		}

		public bool Left
		{
			get
			{
				return ((mButtons & 4) != 0);
			}
		}

		public bool Right
		{
			get
			{
				return ((mButtons & 8) != 0);
			}
		}

		public bool Start
		{
			get
			{
				return ((mButtons & 16) != 0);
			}
		}

		public bool A
		{
			get
			{
				return ((mButtons & 32) != 0);
			}
		}

		public bool B
		{
			get
			{
				return ((mButtons & 64) != 0);
			}
		}

		public bool C
		{
			get
			{
				return ((mButtons & 128) != 0);
			}
		}
	}
}
