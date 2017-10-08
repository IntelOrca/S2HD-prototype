using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	abstract class LevelObject
	{
		private SonicGame mGame;
		private Level mLevel;
		private LevelObjectDefinition mDefinition;

		private int mDisplacementX;
		private int mDisplacementY;

		private int mPartialDisplacementX;
		private int mPartialDisplacementY;

		private int mVelocityX;
		private int mVelocityY;

		private int mRadiusX;
		private int mRadiusY;

		private int mWidthPixels;

		private int mDrawPriority;
		private bool mFinished;

		public static LevelObject Create(SonicGame game, Level level, LevelObjectDefinition definition)
		{
			LevelObject obj;
			switch (definition.Id) {
				case 3:
					obj = new CollisionPlaneSwitcher(game, level, definition);
					break;
				case 6:
					obj = new EHZSpiralPathway(game, level, definition);
					break;
				case 13:
					obj = new Signpost(game, level, definition);
					break;
				case 17:
					obj = new LogBridge(game, level, definition);
					break;
				case 24:
					obj = new EHZPlatform(game, level, definition);
					break;
				case 37:
					obj = new Ring(game, level, definition);
					break;
				case 38:
					obj = new Monitor(game, level, definition);
					break;
				case 54:
					obj = new Spikes(game, level, definition);
					break;
				case 65:
					obj = new Spring(game, level, definition);
					break;
				case 75:
					obj = new Buzzer(game, level, definition);
					break;
				case 92:
					obj = new Masher(game, level, definition);
					break;
				case 121:
					obj = new Starpost(game, level, definition);
					break;
				case 157:
					obj = new Coconuts(game, level, definition);
					break;
				default:
					return null;
			}

			return obj;
		}

		public LevelObject(SonicGame game, Level level)
		{
			mGame = game;
			mLevel = level;
		}

		public LevelObject(SonicGame game, Level level, LevelObjectDefinition definition)
			: this(game, level)
		{
			mDefinition = definition;
			mDisplacementX = definition.DisplacementX;
			mDisplacementY = definition.DisplacementY;
		}

		public virtual void Update() { }
		public virtual void Draw(Graphics g) { }
		public virtual void Touch(LevelObject obj) { }
		public virtual void Interact(LevelObject obj) { }

		/// <summary>
		/// Calculates the next X position by adding the velocity to the current displacement.
		/// </summary>
		/// <returns></returns>
		protected int GetNextDisplacementX()
		{
			long xpos = ((long)mDisplacementX << 32) | (mPartialDisplacementX & 0xFFFFFFFF);
			xpos += ((long)mVelocityX << 24);
			return (int)(xpos >> 32);
		}

		/// <summary>
		/// Calculates the next Y position by adding the velocity to the current displacement.
		/// </summary>
		/// <returns></returns>
		protected int GetNextDisplacementY()
		{
			long ypos = ((long)mDisplacementY << 32) | (mPartialDisplacementY & 0xFFFFFFFF);
			ypos += ((long)mVelocityY << 24);
			return (int)(ypos >> 32);
		}

		/// <summary>
		/// Originally named 'ObjectMove' which updates the displacement by the velocity correctly.
		/// </summary>
		protected virtual void UpdatePosition()
		{
			long xpos = ((long)mDisplacementX << 32) | (mPartialDisplacementX & 0xFFFFFFFF);
			xpos += ((long)mVelocityX << 24);
			mDisplacementX = (int)(xpos >> 32);
			mPartialDisplacementX = (int)(xpos & 0xFFFFFFFF);

			long ypos = ((long)mDisplacementY << 32) | (mPartialDisplacementY & 0xFFFFFFFF);
			ypos += ((long)mVelocityY << 24);
			mDisplacementY = (int)(ypos >> 32);
			mPartialDisplacementY = (int)(ypos & 0xFFFFFFFF);
		}

		/// <summary>
		/// Originally named 'ObjectMoveAndFall' which updates the position and then applies gravity to the Y velocity.
		/// </summary>
		protected void UpdatePositionWithGravity()
		{
			UpdatePosition();
			mVelocityY += 56;
		}

		public bool IsCloseToCharacter
		{
			get
			{
				int directionX, directionY, distX, distY;
				mLevel.GetClosestCharacter(this, out directionX, out directionY, out distX, out distY);

				return (Math.Abs(distX) <= 640 &&
					Math.Abs(distY) <= 420);
			}
		}

		public abstract int Id
		{
			get;
		}

		public SonicGame Game
		{
			get
			{
				return mGame;
			}
		}

		public Level Level
		{
			get
			{
				return mLevel;
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

		public int PartialDisplacementX
		{
			get
			{
				return mPartialDisplacementX;
			}
			set
			{
				mPartialDisplacementX = value;
			}
		}

		public int PartialDisplacementY
		{
			get
			{
				return mPartialDisplacementY;
			}
			set
			{
				mPartialDisplacementY = value;
			}
		}

		public int VelocityX
		{
			get
			{
				return mVelocityX;
			}
			set
			{
				mVelocityX = value;
			}
		}

		public int VelocityY
		{
			get
			{
				return mVelocityY;
			}
			set
			{
				mVelocityY = value;
			}
		}

		public int RadiusX
		{
			get
			{
				return mRadiusX;
			}
			set
			{
				mRadiusX = value;
			}
		}

		public int RadiusY
		{
			get
			{
				return mRadiusY;
			}
			set
			{
				mRadiusY = value;
			}
		}

		public int WidthPixels
		{
			get
			{
				return mWidthPixels;
			}
			set
			{
				mWidthPixels = value;
			}
		}

		public int DrawPriority
		{
			get
			{
				return mDrawPriority;
			}
			set
			{
				mDrawPriority = value;
			}
		}

		public bool Finished
		{
			get
			{
				return mFinished;
			}
			set
			{
				mFinished = value;
			}
		}

		public virtual bool AllowBalancing
		{
			get
			{
				return false;
			}
		}

		public LevelObjectDefinition Definition
		{
			get
			{
				return mDefinition;
			}
		}

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(mDisplacementX - mRadiusX, mDisplacementY - mRadiusY, mRadiusX * 2, mRadiusY * 2);
			}
		}
	}
}
