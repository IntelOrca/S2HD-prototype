using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	[Flags]
	enum CharacterState
	{
		FacingLeft = 1,
		Airborne = 2,
		Spinning = 4,
		OnObject = 8,
		RollJumping = 16,
		Pushing = 32,
		Underwater = 64,
	}

	enum CharacterAnimation
	{
		Walk,
		Run,
		Roll,
		Roll2,
		Push,
		Wait,
		Balance,
		LookUp,
		Duck,
		Spindash,
		Blink,
		GetUp,
		Balance2,
		Stop,
		Float,
		Float2,
		Spring,
		Hang,
		Dash2,
		Dash3,
		Hang2,
		Bubble,
		DeathBW,
		Drown,
		Death,
		Hurt,
		Hurt2,
		Slide,
		Blank,
		Balance3,
		Balance4,
		Transform,
		Lying,
		LieDown,
	}

	abstract class Character : LevelObject
	{
		private Player mPlayer;

		private ControllerState mControlState = new ControllerState();
		private ControllerState mLastControlState = new ControllerState();
		private ControllerState mNewDownControlState = new ControllerState();
		private bool mSuper;

		private CharacterState mStatus;
		private int mRoutine;
		private int mRoutineSecondary;
		private int mAngle;
		private int mGroundVelocity;
		private int mAirLeft;
		private int mObjControl;
		private int mStatusSecondary;
		private int mMoveLock;
		private int mNextTilt;
		private int mTilt;
		private int mStickToConvex;

		private LevelObject mInteract;
		private int mLayer;
		private bool mJumping;

		private int mSkidSoundDuration;

		private int mTopSpeed;
		private int mAcceleration;
		private int mDeceleration;

		private int mPrimaryAngle;
		private int mSecondaryAngle;

		private Camera mCamera = new Camera();

		#region Constant data

		private static int[] SpindashSpeeds = new int[] {
			2048, 2176, 2304, 2432, 2560, 2688, 2816, 2944, 3072 };

		private static byte[][] AnimationData = new byte[][] {
			//  0: Walk
			new byte[] { 0xFF, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0xD, 0xE, 0xFF },
			//  1: Run
			new byte[] { 0xFF, 0x2D, 0x2E, 0x2F, 0x30, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },	
			//  2: Roll
			new byte[] { 0xFE, 0x3D, 0x41, 0x3E, 0x41, 0x3F, 0x41, 0x40, 0x41, 0xFF },
			//  3: Roll 2
			new byte[] { 0xFE, 0x3D, 0x41, 0x3E, 0x41, 0x3F, 0x41, 0x40, 0x41, 0xFF },
			//  4: Push
			new byte[] { 0xFD, 0x48, 0x49, 0x4A, 0x4B, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
			//  5: Wait
			new byte[] {
				5,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
				1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  2,
				3,  3,  3,  3,  3,  4,  4,  4,  5,  5,  5,  4,  4,  4,  5,  5,
				5,  4,  4,  4,  5,  5,  5,  4,  4,  4,  5,  5,  5,  6,  6,  6,
				6,  6,  6,  6,  6,  6,  6,  4,  4,  4,  5,  5,  5,  4,  4,  4,
				5,  5,  5,  4,  4,  4,  5,  5,  5,  4,  4,  4,  5,  5,  5,  6,
				6,  6,  6,  6,  6,  6,  6,  6,  6,  4,  4,  4,  5,  5,  5,  4,
				4,  4,  5,  5,  5,  4,  4,  4,  5,  5,  5,  4,  4,  4,  5,  5,
				5,  6,  6,  6,  6,  6,  6,  6,  6,  6,  6,  4,  4,  4,  5,  5,
				5,  4,  4,  4,  5,  5,  5,  4,  4,  4,  5,  5,  5,  4,  4,  4,
				5,  5,  5,  6,  6,  6,  6,  6,  6,  6,  6,  6,  6,  7,  8,  8,
				8,  9,  9,  9, 0xFE,  6 },
			//  6: Balance
			new byte[] { 9, 0xCC, 0xCD, 0xCE, 0xCD, 0xFF },
			//  7: Look up
			new byte[] { 5, 0xB, 0xC, 0xFE, 1 },
			//  8: Duck
			new byte[] { 5, 0x4C, 0x4D, 0xFE, 1 },
			//  9: Spindash
			new byte[] { 0, 0x42, 0x43, 0x42, 0x44, 0x42, 0x45, 0x42, 0x46, 0x42, 0x47, 0xFF },
			// 10: Blink
			new byte[] { 1, 2, 0xFD, 0 },
			// 11: Get up
			new byte[] { 3, 0xA, 0xFD, 0 },
			// 12: Balance 2
			new byte[] { 3, 0xC8, 0xC9, 0xCA, 0xCB, 0xFF },
			// 13: Stop, halt / skidding animation
			new byte[] { 5, 0xD2, 0xD3, 0xD4, 0xD5, 0xFD, 0 },
			// 14: Float
			new byte[] { 7, 0x54, 0x59, 0xFF },
			// 15: Float 2
			new byte[] { 7, 0x54, 0x55, 0x56, 0x57, 0x58, 0xFF },
			// 16: Spring
			new byte[] { 0x2F, 0x5B, 0xFD, 0 },
			// 17: Hang
			new byte[] { 1, 0x50, 0x51, 0xFF },
			// 18: Dash 2
			new byte[] { 0xF, 0x43, 0x43, 0x43, 0xFE, 1 },
			// 19: Dash 3
			new byte[] { 0xF, 0x43, 0x44, 0xFE, 1 },
			// 20: Hang 2
			new byte[] { 0x13, 0x6B, 0x6C, 0xFF },
			// 21: Bubble, breathe
			new byte[] { 0xB, 0x5A, 0x5A, 0x11, 0x12, 0xFD, 0 },
			// 22: Death BW
			new byte[] { 0x20, 0x5E, 0xFF },
			// 23: Drown
			new byte[] { 0x20, 0x5D, 0xFF },
			// 24: Death
			new byte[] { 0x20, 0x5C, 0xFF },
			// 25: Hurt
			new byte[] { 0x40, 0x4E, 0xFF },
			// 26: Hurt2
			new byte[] { 0x40, 0x4E, 0xFF },
			// 27: Slide
			new byte[] { 9, 0x4E, 0x4F, 0xFF },
			// 28: Blank
			new byte[] { 0x77, 0, 0xFD, 0},
			// 29: Balance 3
			new byte[] { 0x13, 0xD0, 0xD1, 0xFF },
			// 30: Balance 4
			new byte[] { 3, 0xCF, 0xC8, 0xC9, 0xCA, 0xCB, 0xFE, 4 },
			// 31: Lying
			new byte[] { 9, 8, 9, 0xFF },
			// 32: Lie down
			new byte[] { 3, 7, 0xFD, 0},
		};

		#endregion

		public Character(SonicGame game, Level level)
			: base(game, level)
		{
			DrawPriority = 10;

			// DisplacementX = 0x0060;
			// DisplacementY = 0x028F;

			// DisplacementX = 1303;
			// DisplacementY = 625;
		}

		public override void Draw(Graphics g)
		{
			// Hide rapidly if we are invulnerable
			if (mInvulnerable)
				if ((mInvulnerableTime % 8) > 4)
					return;

			Texture2D[] spriteSheet = ResourceManager.SonicTextures;

			if (mMappingFrame == 0)
				return;

			int sidx = mMappingFrame - 1;
			Rectangle src = new Rectangle(0, 0, 64 * Game.DisplayScale, 70 * Game.DisplayScale);
			Rectangle dst = new Rectangle(0, 0, 64 * Game.DisplayScale, 70 * Game.DisplayScale);
			SpriteEffects fx = SpriteEffects.None;
			if ((mStatus & CharacterState.FacingLeft) != 0)
				fx = SpriteEffects.FlipHorizontally;

			if (mFlipAngle != 0) {
				if ((mStatus & CharacterState.FacingLeft) != 0 && mFlipTurned == 0)
					fx = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
			}

			float angle = (float)((double)mAngle / 128.0 * Math.PI);
			if (Math.Abs(mAngle) < 32 && (mStatus & CharacterState.Airborne) == 0)
				angle = 0.0f;
			if ((mStatus & CharacterState.Spinning) != 0)
				angle = 0.0f;
			if (mGroundVelocity == 0)
				angle = 0.0f;

			g.DrawImage(
				spriteSheet[sidx],
				dst,
				src,
				Color.White,
				angle,
				new Vector2(32 * Game.DisplayScale, 35 * Game.DisplayScale),
				fx);

			// Spindash dust
			if (mSpindashing)
				DrawSpindashDust(g);

			if ((mStatusSecondary & 1) != 0)
				DrawShield(g);
		}

		public override void Update()
		{
			if (mControlState.Left && mControlState.Right)
				mControlState.Set(4 | 8, false);
			if (mControlState.Up && mControlState.Down)
				mControlState.Set(1 | 2, false);

			mNewDownControlState = mControlState.GetNewDown(mLastControlState);

			// Normal:
			switch (mRoutine) {
				case 0:
					Init();
					break;
				case 2:
					UpdateControl();
					break;
				case 4:
					UpdateHurt();
					break;
				case 6:
					UpdateDying();
					break;
				case 8:
					Gone();
					break;
				case 10:
					Respawning();
					break;
			}

			mLastControlState = mControlState;
		}

		private void Init()
		{
			mCamera.X = DisplacementX;
			mCamera.Y = DisplacementY;

			mRoutine = 2;
			RadiusY = 19;
			RadiusX = 9;
			// mappings = Mapunc_Sonic
			mTopSpeed = 1536;
			mAcceleration = 12;
			mDeceleration = 128;
			//if (!last_star_pole_hit) {
			if (true) {
				// art tile
				// Ajust2PArtPointer
				mLayer = 1;
				// saved_x_pos = mXPos;
				// saved_y_pos = mYPos;
				// saved_art_tile = art_tile
				// saved_layer = layer;
			}

			mFlipsRemaining = 0;
			mFlipSpeed = 4;
			// super_sonic_flag = false
			mAirLeft = 30;
			DisplacementX -= 32;
			DisplacementY += 4;
			// Sonic_Pos_Record_Index = 0
			// initialise sonic record pos
			DisplacementX += 32;
			DisplacementY -= 4;

			UpdateControl();
		}

		/// <summary>
		/// Normal state for Sonic.
		/// </summary>
		private void UpdateControl()
		{
			// check if controls are locked

			if (mInteract != null)
				mInteract.Interact(this);

			if (mObjControl == 0) {
				switch (mStatus & (CharacterState.Spinning | CharacterState.Airborne)) {
					case 0:
						UpdateNormalState();
						break;
					case CharacterState.Airborne:
						UpdateAirborneState();
						break;
					case CharacterState.Spinning:
						UpdateRollState();
						break;
					case CharacterState.Airborne | CharacterState.Spinning:
						UpdateAirSpinState();
						break;
				}
			}

			if (mInvulnerable)
				UpdateInvulnerability();

			if ((mStatusSecondary & 2) != 0)
				UpdateInvincibility();

			if ((mStatusSecondary & 1) != 0)
				UpdateShield();

			if ((mStatusSecondary & 4) != 0) {
				UpdateSpeedShoes();
			}

			if (mSkidSoundDuration > 0)
				mSkidSoundDuration--;

			mNextTilt = mPrimaryAngle;
			mTilt = mSecondaryAngle;
			// if (WindTunnel_flag) {
			//     if (anim) anim = next_anim
			//}
			Animate();
			if (mObjControl >= 0)
				TouchResponse();

			CameraScroll();
		}

		private void Gone()
		{

		}

		private void Respawning()
		{

		}

		public override int Id
		{
			get
			{
				return 1;
			}
		}

		public int GroundVelocity
		{
			get
			{
				return mGroundVelocity;
			}
			set
			{
				mGroundVelocity = value;
			}
		}

		public ControllerState ControllerState
		{
			get
			{
				return mControlState;
			}
			set
			{
				mControlState = value;
			}
		}

		public int Layer
		{
			get
			{
				return mLayer;
			}
			set
			{
				mLayer = value;
			}
		}

		public int LayerPlus
		{
			get
			{
				return mLayer;
			}
			set
			{
				mLayer = value;
			}
		}

		public CharacterState Status
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

		public LevelObject InteractionObject
		{
			get
			{
				return mInteract;
			}
			set
			{
				mInteract = value;
			}
		}

		public int StatusSecondary
		{
			get
			{
				return mStatusSecondary;
			}
			set
			{
				mStatusSecondary = value;
			}
		}

		public CharacterAnimation Anim
		{
			get
			{
				return mAnim;
			}
			set
			{
				mAnim = value;
			}
		}

		public Player Player
		{
			get
			{
				return mPlayer;
			}
			set
			{
				mPlayer = value;
			}
		}

		public int Angle
		{
			get
			{
				return mAngle;
			}
			set
			{
				mAngle = value;
			}
		}

		public int ObjectControl
		{
			get
			{
				return mObjControl;
			}
			set
			{
				mObjControl = value;
			}
		}

		public int Routine
		{
			get
			{
				return mRoutine;
			}
			set
			{
				mRoutine = value;
			}
		}

		public int MoveLock
		{
			get
			{
				return mMoveLock;
			}
			set
			{
				mMoveLock = value;
			}
		}

		#region Normal

		#region Ground standing / walking / running

		/// <summary>
		/// Subroutine to reset Sonic's mode when he lands on the floor.
		/// </summary>
		public void ResetOnFloor()
		{
			if (!mSpindashing) {
				mAnim = 0;
				if ((mStatus & CharacterState.Spinning) != 0) {
					mStatus &= ~CharacterState.Spinning;
					RadiusY = 19;
					RadiusX = 9;
					mAnim = 0;
					DisplacementY -= 5;
				}
			}

			mStatus &= ~CharacterState.Airborne;
			mStatus &= ~CharacterState.Pushing;
			mStatus &= ~CharacterState.RollJumping;
			mJumping = false;
			mPlayer.ChainBonusCounter = 0;
			mFlipAngle = 0;
			mFlipTurned = 0;
			mFlipsRemaining = 0;
			mLookDelayCounter = 0;
			if (mAnim == CharacterAnimation.Hang2)
				mAnim = 0;
		}

		/// <summary>
		/// Orignally called 'MdNormalChecks' which updates movement when standing / walking or running
		/// </summary>
		private void UpdateNormalState()
		{
			if (!mControlState.A && !mControlState.B && !mControlState.C) {
				if (mAnim == CharacterAnimation.Blink || mAnim == CharacterAnimation.GetUp)
					return;
				if (mAnim == CharacterAnimation.Wait && mAnimFrame >= 30) {
					if (!mControlState.Up && !mControlState.Down && !mControlState.Left && !mControlState.Right && !mControlState.A && !mControlState.B && !mControlState.C)
						return;
					mAnim = CharacterAnimation.Blink;
					if (mAnimFrame < 172)
						return;
					mAnim = CharacterAnimation.GetUp;
					return;
				}
			}

			if (CheckSpindash())
				return;
			if (CheckForJump())
				return;
			ApplySlopeResistance();
			UpdateNormalMovement();
			CheckForRollStart();
			KeepInLevelBoundaries();
			UpdatePosition();
			UpdateAngleAndPositionToFloor();
			ApplySlopeLocking();
		}

		private void UpdateNormalMovement()
		{
			if (mStatusSecondary >= 0) {
				if (mMoveLock == 0) {
					if (mControlState.Left)
						MoveLeft();
					if (mControlState.Right)
						MoveRight();

					// Is sonic on a slope
					if (((mAngle + 32) & 192) == 0) {
						// Is sonic not moving
						if (mGroundVelocity == 0) {
							mStatus = mStatus & ~CharacterState.Pushing;

							// Use standing animation
							mAnim = CharacterAnimation.Wait;

							if (!UpdateBalancing())
								UpdateLooking();
						}
					}
				}

				if (!mLookingUpOrDucking)
					RestoreCameraBias();

				if (!mControlState.Left && !mControlState.Right) {
					if (mGroundVelocity != 0) {
						if (mGroundVelocity < 0) {
							if (mGroundVelocity + mAcceleration >= 0)
								mGroundVelocity = 0;
							else
								mGroundVelocity += mAcceleration;
						} else {
							if (mGroundVelocity - mAcceleration <= 0)
								mGroundVelocity = 0;
							else
								mGroundVelocity -= mAcceleration;
						}
					}
				}
			}

			// Traction
			VelocityX = (SonicMaths.Cos(mAngle) * mGroundVelocity) >> 8;
			VelocityY = (SonicMaths.Sin(mAngle) * mGroundVelocity) >> 8;

			CheckPushing();
		}

		/// <summary>
		/// Checks if we are on the edge of a platform. Applies balancing logic if so.
		/// </summary>
		/// <returns>True if we are balancing.</returns>
		private bool UpdateBalancing()
		{
			if ((mStatus & CharacterState.OnObject) != 0) {
				// Balance on object
				LevelObject obj = mInteract;
				if (obj.AllowBalancing) {
					if (!mSuper) {
						if (DisplacementX <= obj.DisplacementX) {
							// Left
							int gap = (obj.DisplacementX - obj.RadiusX) - (DisplacementX - RadiusX);
							if (gap >= 7) {
								if ((mStatus & CharacterState.FacingLeft) != 0) {
									if (gap >= 13)
										mAnim = CharacterAnimation.Balance2;
									else
										mAnim = CharacterAnimation.Balance;
								} else {
									if (gap >= 13) {
										mAnim = CharacterAnimation.Balance4;
										mStatus |= CharacterState.FacingLeft;
									} else {
										mAnim = CharacterAnimation.Balance3;
									}
								}
							}
						} else {
							// Right
							int gap = (DisplacementX + RadiusX) - (obj.DisplacementX + obj.RadiusX);
							if (gap >= 7) {
								if ((mStatus & CharacterState.FacingLeft) == 0) {
									if (gap >= 13)
										mAnim = CharacterAnimation.Balance2;
									else
										mAnim = CharacterAnimation.Balance;
								} else {
									if (gap >= 13) {
										mAnim = CharacterAnimation.Balance4;
										mStatus &= ~CharacterState.FacingLeft;
									} else {
										mAnim = CharacterAnimation.Balance3;
									}
								}
							}
						}
					} else {

					}
				}
			} else {
				// Balancing checks for when you're on the edge of part of the level
				int d1 = ChkFloorEdge(DisplacementX, DisplacementY);
				if (d1 >= 12) {
					if (mSuper) {
						// SuperSonic_Balance2
						if (mNextTilt == 3) {
							mStatus = mStatus & ~CharacterState.FacingLeft;
							mAnim = CharacterAnimation.Balance;
							mLookDelayCounter = 0;
							return true;
						} else if (mTilt == 3) {
							mStatus |= CharacterState.FacingLeft;
							mAnim = CharacterAnimation.Balance;
							mLookDelayCounter = 0;
							return true;
						}
					}

					if (mNextTilt != 3) {
						// Sonic_BalanceLeft
						if (mTilt == 3) {
							if ((mStatus & CharacterState.FacingLeft) != 0) {
								mAnim = CharacterAnimation.Balance;
								d1 = ChkFloorEdge(DisplacementX + 6, DisplacementY);
								if (d1 >= 12)
									mAnim = CharacterAnimation.Balance2;
							} else {
								mAnim = CharacterAnimation.Balance3;
								d1 = ChkFloorEdge(DisplacementX + 6, DisplacementY);
								if (d1 >= 12) {
									mAnim = CharacterAnimation.Balance4;
									mStatus |= CharacterState.FacingLeft;
								}
							}
							mLookDelayCounter = 0;
							return true;
						}
					} else {
						if ((mStatus & CharacterState.FacingLeft) == 0) {
							mAnim = CharacterAnimation.Balance;
							d1 = ChkFloorEdge(DisplacementX - 6, DisplacementY);
							if (d1 >= 12)
								mAnim = CharacterAnimation.Balance2;
						} else {
							mAnim = CharacterAnimation.Balance3;
							d1 = ChkFloorEdge(DisplacementX - 6, DisplacementY);
							if (d1 >= 12) {
								mAnim = CharacterAnimation.Balance4;
								mStatus = mStatus & ~CharacterState.FacingLeft;
							}
						}
						mLookDelayCounter = 0;
						return true;
					}
				}
			}

			return false;
		}

		private void MoveLeft()
		{
			if (mGroundVelocity > 0) {
				// Turn left
				int d0 = mGroundVelocity - mDeceleration;
				if (d0 < 0)
					d0 = -128;
				mGroundVelocity = d0;
				d0 = (mAngle + 32) & 192;
				if (d0 != 0)
					return;

				if (mGroundVelocity < 1024)
					return;

				mAnim = CharacterAnimation.Stop;
				mStatus &= ~CharacterState.FacingLeft;

				if (mSkidSoundDuration <= 0) {
					Level.AddSound(ResourceManager.BrakeSound, DisplacementX, DisplacementY);
					mSkidSoundDuration = 8;
				}

				if (mAirLeft < 12)
					return;

				// Do skid dust

				return;
			}

			if ((mStatus & CharacterState.FacingLeft) == 0) {
				mStatus &= ~CharacterState.Pushing;
				mNextAnim = CharacterAnimation.Run;
			}
			mStatus |= CharacterState.FacingLeft;

			int newInertia = mGroundVelocity - mAcceleration;
			int maxInertia = -mTopSpeed;
			if (newInertia <= maxInertia) {
				newInertia += mAcceleration;
				if (newInertia > maxInertia)
					newInertia = maxInertia;
			}

			mGroundVelocity = newInertia;
			mAnim = 0;
		}

		private void MoveRight()
		{
			if (mGroundVelocity < 0) {
				// Turn right
				int d0 = mGroundVelocity + mDeceleration;
				if (d0 > 0)
					d0 = 128;
				mGroundVelocity = d0;
				d0 = (mAngle + 32) & 192;
				if (d0 != 0)
					return;

				if (mGroundVelocity > -1024)
					return;

				mAnim = CharacterAnimation.Stop;
				mStatus |= CharacterState.FacingLeft;

				if (mSkidSoundDuration <= 0) {
					Level.AddSound(ResourceManager.BrakeSound, DisplacementX, DisplacementY);
					mSkidSoundDuration = 8;
				}

				if (mAirLeft < 12)
					return;

				// Do skid dust

				return;
			}

			if ((mStatus & CharacterState.FacingLeft) != 0) {
				mStatus &= ~CharacterState.Pushing;
				mNextAnim = CharacterAnimation.Run;
			}
			mStatus &= ~CharacterState.FacingLeft;

			int newInertia = mGroundVelocity + mAcceleration;
			int maxInertia = mTopSpeed;
			if (newInertia >= maxInertia) {
				newInertia -= mAcceleration;
				if (newInertia < maxInertia)
					newInertia = maxInertia;
			}

			mGroundVelocity = newInertia;
			mAnim = 0;
		}

		/// <summary>
		/// Checks if we are pushing against a wall.
		/// </summary>
		private void CheckPushing()
		{
			if (mAngle + 64 < 0)
				return;

			if (mGroundVelocity == 0)
				return;

			byte ang = (byte)(mAngle + (mGroundVelocity > 0 ? -64 : 64));
			int roomInFront = CalcRoomInFront(ang);
			if (roomInFront >= 0)
				return;

			roomInFront *= 256;
			ang += 32;
			ang &= 192;
			if (ang == 0) {
				VelocityY += roomInFront;
			} else if (ang == 64) {
				VelocityX -= roomInFront;
				mStatus |= CharacterState.Pushing;
				mGroundVelocity = 0;
			} else if (ang == 128) {
				VelocityY -= roomInFront;
			} else {
				VelocityX += roomInFront;
				mStatus |= CharacterState.Pushing;
				mGroundVelocity = 0;
			}
		}

		/// <summary>
		/// Increases or decreases the speed if not zero by the slope resistance.
		/// </summary>
		private void ApplySlopeResistance()
		{
			if ((mAngle + 96) % 256 >= 192)
				return;

			int resistance = (SonicMaths.Sin(mAngle) * 32) >> 8;
			if (mGroundVelocity == 0 || resistance == 0)
				return;

			mGroundVelocity += resistance;
		}

		/// <summary>
		/// Stops Sonic from walking up a slope that is too steep by locking movement control.
		/// </summary>
		private void ApplySlopeLocking()
		{
			if (mStickToConvex != 0)
				return;

			if (mMoveLock > 0) {
				mMoveLock--;
				return;
			}

			if (((mAngle + 32) & 192) == 0)
				return;

			if (Math.Abs(mGroundVelocity) > 640)
				return;

			mGroundVelocity = 0;
			mStatus |= CharacterState.Airborne;
			mMoveLock = 30;
		}

		/// <summary>
		/// Originally 'AnglePos' which changes the angle and position to the angle and position of the floor.
		/// </summary>
		private void UpdateAngleAndPositionToFloor()
		{
			if ((mStatus & CharacterState.OnObject) != 0) {
				mPrimaryAngle = 0;
				mSecondaryAngle = 0;
				SetAngle(0);
				return;
			}

			mPrimaryAngle = 3;
			mSecondaryAngle = 3;

			if (mAngle > -96 && mAngle < -32)
				WalkRightWall();
			else if (mAngle >= -32 && mAngle <= 32)
				WalkFloor();
			else if (mAngle > 32 && mAngle <= 96)
				WalkLeftWall();
			else
				WalkCeiling();
		}

		private void WalkFloor()
		{
			int d0;

			int leftDistance, rightDistance;
			Level.FindFloor(DisplacementX + RadiusX, DisplacementY + RadiusY, mLayer, false, true, out rightDistance, ref mPrimaryAngle);
			Level.FindFloor(DisplacementX - RadiusX, DisplacementY + RadiusY, mLayer, false, true, out leftDistance, ref mSecondaryAngle);

			int d1 = SetAngleFromFloor(leftDistance, rightDistance);
			if (d1 == 0)
				return;

			if (d1 >= 0) {
				d0 = (Math.Abs(VelocityX) >> 8) + 4;
				if (d0 >= 14)
					d0 = 14;
				if (d1 > d0) {
					if (mStickToConvex != 0) {
						DisplacementY += d1;
						return;
					}

					mStatus |= CharacterState.Airborne;
					mStatus &= ~CharacterState.Pushing;
					mNextAnim = CharacterAnimation.Run;
					return;
				}

				DisplacementY += d1;
				return;
			}

			if (d1 < -14)
				return;

			DisplacementY += d1;
		}

		private void WalkRightWall()
		{
			int leftDist, rightDist, minDist, speed;
			Level.FindWallRight(DisplacementX + RadiusY, DisplacementY - RadiusX, mLayer, false, true, out rightDist, ref mPrimaryAngle);
			Level.FindWallRight(DisplacementX + RadiusY, DisplacementY + RadiusX, mLayer, false, true, out leftDist, ref mSecondaryAngle);
			minDist = SetAngleFromFloor(leftDist, rightDist);
			if (minDist == 0)
				return;

			if (minDist < 0) {
				if (minDist < -14)
					return;
				DisplacementX += minDist;
				return;
			}

			speed = (Math.Abs(VelocityY) >> 8) + 4;
			if (speed > 14)
				speed = 14;

			if (minDist <= speed) {
				DisplacementX += minDist;
				return;
			}

			if (mStickToConvex != 0) {
				DisplacementX += minDist;
				return;
			}

			mStatus |= CharacterState.Airborne;
			mStatus &= ~CharacterState.Pushing;
			mNextAnim = CharacterAnimation.Run;
		}

		private void WalkCeiling()
		{
			int leftDist, rightDist, minDist, speed;
			Level.FindCeiling(DisplacementX + RadiusX, DisplacementY - RadiusY, mLayer, false, true, out rightDist, ref mPrimaryAngle);
			Level.FindCeiling(DisplacementX - RadiusX, DisplacementY - RadiusY, mLayer, false, true, out leftDist, ref mSecondaryAngle);
			minDist = SetAngleFromFloor(leftDist, rightDist);
			if (minDist == 0)
				return;

			if (minDist < 0) {
				if (minDist < -14)
					return;
				DisplacementY -= minDist;
				return;
			}

			speed = (Math.Abs(VelocityX) >> 8) + 4;
			if (speed > 14)
				speed = 14;

			if (minDist <= speed) {
				DisplacementY -= minDist;
				return;
			}

			if (mStickToConvex != 0) {
				DisplacementY -= minDist;
				return;
			}

			mStatus |= CharacterState.Airborne;
			mStatus &= ~CharacterState.Pushing;
			mNextAnim = CharacterAnimation.Run;
		}

		private void WalkLeftWall()
		{
			int leftDist, rightDist, minDist, speed;
			Level.FindWallLeft(DisplacementX - RadiusY, DisplacementY - RadiusX, mLayer, false, true, out rightDist, ref mPrimaryAngle);
			Level.FindWallLeft(DisplacementX - RadiusY, DisplacementY + RadiusX, mLayer, false, true, out leftDist, ref mSecondaryAngle);
			minDist = SetAngleFromFloor(leftDist, rightDist);
			if (minDist == 0)
				return;

			if (minDist < 0) {
				if (minDist < -14)
					return;
				DisplacementX -= minDist;
				return;
			}

			speed = (Math.Abs(VelocityY) >> 8) + 4;
			if (speed > 14)
				speed = 14;

			if (minDist <= speed) {
				DisplacementX -= minDist;
				return;
			}

			if (mStickToConvex != 0) {
				DisplacementX -= minDist;
				return;
			}

			mStatus |= CharacterState.Airborne;
			mStatus &= ~CharacterState.Pushing;
			mNextAnim = CharacterAnimation.Run;
		}

		#endregion

		#region Ground rolling

		/// <summary>
		/// Subroutine allowing Sonic to start rolling when he's moving.
		/// </summary>
		private void CheckForRollStart()
		{
			if (mStatusSecondary < 0)
				return;

			if (Math.Abs(mGroundVelocity) < 128)
				return;

			if (mControlState.Left || mControlState.Right)
				return;

			if (!mControlState.Down)
				return;

			if ((mStatus & CharacterState.Spinning) != 0)
				return;

			mStatus |= CharacterState.Spinning;
			RadiusY = 14;
			RadiusX = 7;
			mAnim = CharacterAnimation.Roll;
			DisplacementY += 5;

			Level.AddSound(ResourceManager.SpinSound, DisplacementX, DisplacementY);

			if (mGroundVelocity != 0)
				return;

			mGroundVelocity = 512;
		}

		/// <summary>
		/// Originally 'MdRoll' which updates the movement logic when rolling.
		/// </summary>
		private void UpdateRollState()
		{
			if (!mSpindashing)
				if (CheckForJump())
					return;
			RollRepel();
			UpdateRollVelocity();
			KeepInLevelBoundaries();
			UpdatePosition();
			UpdateAngleAndPositionToFloor();
			ApplySlopeLocking();
		}

		/// <summary>
		/// Originally 'RollSpeed' which updates the velocity.
		/// </summary>
		private void UpdateRollVelocity()
		{
			if (mStatusSecondary >= 0) {
				// Apply player movement
				if (mMoveLock == 0) {
					if (mControlState.Left)
						RollLeft();
					if (mControlState.Right)
						RollRight();
				}

				// Apply ground velocity friction
				if (mGroundVelocity < 0) {
					mGroundVelocity += (mAcceleration >> 1);
					if (mGroundVelocity > 0)
						mGroundVelocity = 0;
				} else if (mGroundVelocity > 0) {
					mGroundVelocity -= (mAcceleration >> 1);
					if (mGroundVelocity < 0)
						mGroundVelocity = 0;
				}

				// Check if rolling has stopped
				if (mGroundVelocity == 0) {
					// note: the spindash flag has a different meaning when Sonic's already rolling -- it's used to mean he's not allowed to stop rolling
					if (!mSpindashing) {
						// 
						mStatus &= ~CharacterState.Spinning;
						RadiusY = 19;
						RadiusX = 9;
						mAnim = CharacterAnimation.Wait;
						DisplacementY -= 5;
					} else {
						// Forced rolling
						mGroundVelocity = 1024;
						if ((mStatus & CharacterState.FacingLeft) != 0)
							mGroundVelocity = -mGroundVelocity;
					}
				}
			}

			RestoreCameraBias();

			// SetRollSpeeds
			VelocityY = (SonicMaths.Sin(mAngle) * mGroundVelocity) >> 8;
			int newVX = (SonicMaths.Cos(mAngle) * mGroundVelocity) >> 8;
			if (newVX > 4096)
				newVX = 4096;
			if (newVX < -4096)
				newVX = -4096;
			VelocityX = newVX;

			CheckPushing();
		}

		/// <summary>
		/// Increases ground velocity when on a hill descent.
		/// </summary>
		private void RollRepel()
		{
			if (((mAngle + 96) & 0xFF) >= 192)
				return;

			int d0 = (SonicMaths.Sin(mAngle) * 80) >> 8;
			if (mGroundVelocity >= 0) {
				if (d0 < 0)
					d0 >>= 2;
				mGroundVelocity += d0;
				return;
			}

			if (d0 >= 0)
				d0 >>= 2;

			mGroundVelocity += d0;
		}

		/// <summary>
		/// Apply player left force.
		/// </summary>
		private void RollLeft()
		{
			if (mGroundVelocity > 0) {
				// Brake
				int d0 = mGroundVelocity - (mDeceleration >> 2);
				if (d0 < 0)
					d0 = -128;
				mGroundVelocity = d0;
				return;
			}

			mStatus |= CharacterState.FacingLeft;
			mAnim = CharacterAnimation.Roll;
		}

		/// <summary>
		/// Apply player right force.
		/// </summary>
		private void RollRight()
		{
			if (mGroundVelocity < 0) {
				// Brake
				int d0 = mGroundVelocity + (mDeceleration >> 2);
				if (d0 > 0)
					d0 = 128;
				mGroundVelocity = d0;
				return;
			}

			mStatus &= ~CharacterState.FacingLeft;
			mAnim = CharacterAnimation.Roll;
		}

		#endregion

		#region Airborne

		/// <summary>
		/// Updates logic for when Sonic is in a ball and airborne (he could be jumping but not necessarily).
		/// </summary>
		private void UpdateAirSpinState()
		{
			UpdateAirborneState();
		}

		/// <summary>
		/// Originally 'Jump' which starts a jump if we can and we should.
		/// </summary>
		private bool CheckForJump()
		{
			int d0, d1, d2;

			if (!mNewDownControlState.A && !mNewDownControlState.B && !mNewDownControlState.C)
				return false;

			d0 = mAngle + 128;
			d1 = CalcRoomOverHead(d0);
			if (d1 < 6)
				return false;

			d2 = 1664;
			if (mSuper)
				d2 = 2048;

			if ((mStatus & CharacterState.Underwater) != 0)
				d2 = 896;

			VelocityX += (SonicMaths.Cos(mAngle - 64) * d2) >> 8;
			VelocityY += (SonicMaths.Sin(mAngle - 64) * d2) >> 8;
			mStatus |= CharacterState.Airborne;
			mStatus &= ~CharacterState.Pushing;

			mJumping = true;
			mStickToConvex = 0;

			Level.AddSound(ResourceManager.JumpSound, DisplacementX, DisplacementY);

			RadiusY = 19;
			RadiusX = 9;
			if ((mStatus & CharacterState.Spinning) != 0) {
				mStatus |= CharacterState.RollJumping;
				return true;
			}

			RadiusY = 14;
			RadiusX = 7;
			mAnim = CharacterAnimation.Roll;
			mStatus |= CharacterState.Spinning;
			DisplacementY += 5;

			return true;
		}

		/// <summary>
		/// Called if Sonic is airborne, but not in a ball (thus, probably not jumping).
		/// </summary>
		private void UpdateAirborneState()
		{
			UpdateJumpHeight();
			UpdateAirborneVelocity();
			KeepInLevelBoundaries();
			UpdatePositionWithGravity();

			// If underwater
			if ((mStatus & CharacterState.Underwater) != 0)
				VelocityY -= 40;

			UpdateAirborneAngle();
			DoLevelCollision();
		}

		/// <summary>
		/// Updates and controls velocity when airborne.
		/// </summary>
		private void UpdateAirborneVelocity()
		{
			// Prevent air control if we jumped from ball state
			if ((mStatus & CharacterState.RollJumping) == 0) {
				if (mControlState.Left) {
					mStatus |= CharacterState.FacingLeft;
					VelocityX -= mAcceleration * 2;

					// Enforce top speed
					if (VelocityX < -mTopSpeed)
						VelocityX = -mTopSpeed;
				} else if (mControlState.Right) {
					mStatus &= ~CharacterState.FacingLeft;
					VelocityX += mAcceleration * 2;

					// Enforce top speed
					if (VelocityX > mTopSpeed)
						VelocityX = mTopSpeed;
				}
			}

			// Restore the camera bias
			RestoreCameraBias();

			// No drag if we are going up this quickly
			if (VelocityY < -1024)
				return;

			// Slow velocity X down by 1/32
			int velocityChange = VelocityX >> 5;
			if (velocityChange == 0)
				return;
			VelocityX -= velocityChange;

			// Make sure we set velocity X to 0 if we've changed direction
			if (velocityChange < 0 && VelocityX > 0)
				VelocityX = 0;
			else if (velocityChange >= 0 && VelocityX < 0)
				VelocityX = 0;
		}

		/// <summary>
		/// Restores the character's angle when airborne.
		/// </summary>
		private void UpdateAirborneAngle()
		{
			sbyte angle = (sbyte)(mAngle & -1);

			if (angle < 0)
				angle += 2;
			else if (angle > 0)
				angle -= 2;

			SetAngle(angle);

			UpdateAirborneFlip();
		}

		#endregion

		#region Jumping

		/// <summary>
		/// Increases the height of the jump if A, B or C is down.
		/// </summary>
		private void UpdateJumpHeight()
		{
			if (!mJumping) {
				if (mSpindashing)
					return;
				if (VelocityY < -4032)
					VelocityY = -4032;
				return;
			}

			int d1 = -1024;
			if ((mStatus & CharacterState.Underwater) != 0)
				d1 = -512;

			if (d1 > VelocityY)
				if (!mControlState.A && !mControlState.B && !mControlState.C)
					VelocityY = d1;
		}

		#endregion

		#region Flipping

		private int mFlipsRemaining;
		private int mFlipSpeed;
		private int mFlipAngle;
		private int mFlipTurned;

		/// <summary>
		/// Restores the character's flip angle when airborne.
		/// </summary>
		private void UpdateAirborneFlip()
		{
			if (mFlipAngle == 0)
				return;

			if (mGroundVelocity < 0 && mFlipTurned != 0)
				JumpFlipLeft();
			else
				JumpFlipRight();
		}

		private void JumpFlipLeft()
		{
			int d0 = mFlipAngle - mFlipSpeed;
			if (d0 >= 0) {
				mFlipAngle = d0;
				return;
			}

			mFlipsRemaining--;
			if (mFlipsRemaining >= 0) {
				mFlipAngle = d0;
				return;
			}

			mFlipsRemaining = 0;
			mFlipAngle = 0;
		}

		private void JumpFlipRight()
		{
			int d0 = mFlipAngle + mFlipSpeed;
			if (d0 >= 0) {
				mFlipAngle = d0;
				return;
			}

			mFlipsRemaining--;
			if (mFlipsRemaining >= 0) {
				mFlipAngle = d0;
				return;
			}

			mFlipsRemaining = 0;
			mFlipAngle = 0;
		}

		public int FlipsRemaining
		{
			get
			{
				return mFlipsRemaining;
			}
			set
			{
				mFlipsRemaining = value;
			}
		}

		public int FlipSpeed
		{
			get
			{
				return mFlipSpeed;
			}
			set
			{
				mFlipSpeed = value;
			}
		}

		public int FlipAngle
		{
			get
			{
				return mFlipAngle;
			}
			set
			{
				mFlipAngle = value;
			}
		}

		#endregion

		#region Looking up / Ducking

		private bool mLookingUpOrDucking;
		private int mLookDelayCounter;

		/// <summary>
		/// Update ducking / looking up
		/// </summary>
		private void UpdateLooking()
		{
			mLookingUpOrDucking = false;

			if (mControlState.Up) {
				// Lookup
				mAnim = CharacterAnimation.LookUp;
				mLookDelayCounter++;
				if (mLookDelayCounter < 120)
					return;
				mLookDelayCounter = 120;

				if (mCamera.BiasY > -86)
					mCamera.BiasY -= 2;

				mLookingUpOrDucking = true;

			} else if (mControlState.Down) {
				// Duck
				mAnim = CharacterAnimation.Duck;
				mLookDelayCounter++;
				if (mLookDelayCounter < 120)
					return;
				mLookDelayCounter = 120;

				if (mCamera.BiasY < 88)
					mCamera.BiasY += 2;

				mLookingUpOrDucking = true;
			} else {
				mLookDelayCounter = 0;
			}
		}

		#endregion

		#endregion

		#region Hurting

		private void UpdateHurt()
		{
			if (mRoutineSecondary < 0) {
				// Instant recovery
				mRoutine = 2;
				mRoutineSecondary = 0;
				Animate();
				return;
			}

			if (mInteract != null)
				mInteract.Interact(this);

			UpdatePosition();
			if ((mStatus & CharacterState.Underwater) != 0)
				VelocityY += 16;
			else
				VelocityY += 48;

			DoLevelCollision();

			if ((mStatus & CharacterState.Airborne) == 0) {
				VelocityY = 0;
				VelocityX = 0;
				mGroundVelocity = 0;
				mObjControl = 0;
				mAnim = 0;
				mRoutine = 2;
				GiveInvulnerability();
				mSpindashing = false;
			}

			KeepInLevelBoundaries();
			Animate();
			if (mObjControl >= 0)
				TouchResponse();

			CameraScroll();
		}

		public void Hurt(LevelObject causeObject)
		{
			// Check if invincible
			if ((mStatusSecondary & 2) != 0)
				return;

			if (mInvulnerable)
				return;

			// Check if character has a shield
			if ((mStatusSecondary & 1) != 0) {
				mStatusSecondary &= ~1;
				Level.AddSound(ResourceManager.HurtSound, DisplacementX, DisplacementY);
			} else if (mPlayer.Rings > 0) {
				ScatterRings();
			} else {
				Kill(causeObject);
				return;
			}

			mRoutine = 4;
			ResetOnFloor();
			mStatus |= CharacterState.Airborne;
			if ((mStatus & CharacterState.Underwater) != 0) {
				VelocityY = -512;
				VelocityX = -256;
			} else {
				VelocityY = -1024;
				VelocityX = -512;
			}

			// Reverse X velocity if other direction
			if (DisplacementX >= causeObject.DisplacementX)
				VelocityX = -VelocityX;

			mGroundVelocity = 0;
			mAnim = CharacterAnimation.Hurt2;
			GiveInvulnerability();
			if (causeObject is Spikes)
				Level.AddSound(ResourceManager.SpikesSound, DisplacementX, DisplacementY);
		}

		private void ScatterRings()
		{
			int angle = 72;
			int speed = 4;
			bool n = false;

			for (int t = 0; t < Math.Min(mPlayer.Rings, 32); t++) {
				if (t == 16) {
					speed = 2;
					angle = 72;
				}

				Ring ring = new Ring(Game, Level);
				ring.Scattering = true;
				ring.VelocityX = SonicMaths.Cos(angle) * speed;
				ring.VelocityY = -SonicMaths.Sin(angle) * speed;
				ring.DisplacementX = DisplacementX;
				ring.DisplacementY = DisplacementY;
				if (n) {
					ring.VelocityX = -ring.VelocityX;
					angle += 16;
				}

				Level.Objects.Add(ring);

				n = !n;
			}

			mPlayer.Rings = 0;

			Level.AddSound(ResourceManager.RingScatterSound, DisplacementX, DisplacementY);
		}

		#endregion

		#region Dying

		private int mDeadDuration;

		private void UpdateDying()
		{
			mDeadDuration--;
			if (mDeadDuration <= 0) {
				mRoutine = 8;
				mPlayer.Status = PlayerStatus.Dead;
				return;
			}

			UpdatePositionWithGravity();
			Animate();
		}

		public void Kill()
		{
			Kill(null);
		}

		public void Kill(LevelObject causeObject)
		{
			mStatusSecondary = 0;
			mRoutine = 6;
			ResetOnFloor();
			mStatus |= CharacterState.Airborne;
			VelocityY = -1792;
			VelocityX = 0;
			mGroundVelocity = 0;
			mAnim = CharacterAnimation.Death;
			DrawPriority = 1200;
			mDeadDuration = 180;

			if (causeObject is Spikes)
				Level.AddSound(ResourceManager.SpikesSound, DisplacementX, DisplacementY);
			else
				Level.AddSound(ResourceManager.HurtSound, DisplacementX, DisplacementY);
		}

		#endregion

		#region Spindash

		private bool mSpindashing;
		private int mSpindashCharge;
		private int mSpindashAnimationFrame;
		private int mSpindashAnimationFrameDuration;
		private int mSpindashSoundDuration;

		private void DrawSpindashDust(Graphics g)
		{
			Rectangle dst, src = new Rectangle(32 * mSpindashAnimationFrame * Game.DisplayScale, 0, 32 * Game.DisplayScale, 24 * Game.DisplayScale);

			if ((mStatus & CharacterState.FacingLeft) != 0) {
				dst = new Rectangle(0 * Game.DisplayScale, -4 * Game.DisplayScale, 32 * Game.DisplayScale, 24 * Game.DisplayScale);
				g.DrawImage(ResourceManager.SpindashDustTexture, dst, src, Color.White, SpriteEffects.FlipHorizontally);
			} else {
				dst = new Rectangle(-32 * Game.DisplayScale, -4 * Game.DisplayScale, 32 * Game.DisplayScale, 24 * Game.DisplayScale);
				g.DrawImage(ResourceManager.SpindashDustTexture, dst, src, Color.White);
			}
		}

		/// <summary>
		/// Check for starting to charge a spindash.
		/// </summary>
		private bool CheckSpindash()
		{
			if (mSpindashing)
				return UpdateSpindash();

			// Check if ducking
			if (mAnim != CharacterAnimation.Duck)
				return false;

			if (!mNewDownControlState.A && !mNewDownControlState.B && !mNewDownControlState.C)
				return false;

			mAnim = CharacterAnimation.Spindash;

			Level.AddSound(ResourceManager.SpindashChargeSound, DisplacementX, DisplacementY);

			mSpindashing = true;
			mSpindashCharge = 0;
			// if (mAirLeft >= 12)
			//    dust
			KeepInLevelBoundaries();
			UpdateAngleAndPositionToFloor();

			return true;
		}

		/// <summary>
		/// Subrouting to update an already-charging spindash.
		/// </summary>
		private bool UpdateSpindash()
		{
			// Decrease the spindash sound duration
			if (mSpindashSoundDuration > 0)
				mSpindashSoundDuration--;

			// Check if down is still pressed, otherwise release spindash
			if (mControlState.Down) {
				// Slowly discharge the spindash
				if (mSpindashCharge != 0) {
					mSpindashCharge -= mSpindashCharge >> 5;
					if (mSpindashCharge < 0)
						mSpindashCharge = 0;
				}

				// Charge up the spindash if a, b or c is pressed
				if (mNewDownControlState.A || mNewDownControlState.B || mNewDownControlState.C)
					ChargeSpindash();

				// Update spindash animation
				mSpindashAnimationFrameDuration--;
				if (mSpindashAnimationFrameDuration <= 0) {
					mSpindashAnimationFrame = (mSpindashAnimationFrame + 1) % 7;
					mSpindashAnimationFrameDuration = 2;
				}
			} else {
				ReleaseSpindash();
			}

			RestoreCameraBias();

			KeepInLevelBoundaries();
			UpdateAngleAndPositionToFloor();

			return true;
		}

		private void ChargeSpindash()
		{
			mAnim = CharacterAnimation.Spindash;
			mAnimFrame = 0;

			// There must be at least a 10 turn interval before replaying spindash charge sound
			if (mSpindashSoundDuration <= 0) {
				Level.AddSound(ResourceManager.SpindashChargeSound, DisplacementX, DisplacementY);
				mSpindashSoundDuration = 10;
			}

			mSpindashCharge += 512;
			if (mSpindashCharge >= 2048)
				mSpindashCharge = 2048;
		}

		private void ReleaseSpindash()
		{
			RadiusY = 14;
			RadiusX = 7;
			mAnim = CharacterAnimation.Roll;
			DisplacementY += 5;
			mSpindashing = false;
			mGroundVelocity = SpindashSpeeds[mSpindashCharge >> 8];
			mCamera.ScrollDelayX = 8192 - (((mGroundVelocity - 2048) * 2) & 7936);
			if ((mStatus & CharacterState.FacingLeft) != 0)
				mGroundVelocity = -mGroundVelocity;
			mStatus |= CharacterState.Spinning;

			Level.AddSound(ResourceManager.SpindashReleaseSound, DisplacementX, DisplacementY);
		}

		#endregion

		#region Shield

		private int mShieldAnimationFrame;

		private void DrawShield(Graphics g)
		{
			Rectangle src, dst;

			if (mShieldAnimationFrame % 2 == 1) {
				src = new Rectangle(0, 32 * Game.DisplayScale, 48 * Game.DisplayScale, 48 * Game.DisplayScale);
				dst = new Rectangle(-24 * Game.DisplayScale, -24 * Game.DisplayScale, 48 * Game.DisplayScale, 48 * Game.DisplayScale);
			} else {
				src = new Rectangle(32 * (mShieldAnimationFrame / 2) * Game.DisplayScale, 0, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
				dst = new Rectangle(-16 * Game.DisplayScale, -16 * Game.DisplayScale, 32 * Game.DisplayScale, 32 * Game.DisplayScale);
			}

			g.DrawImage(ResourceManager.ShieldTexture, dst, src, Color.White);
		}

		private void UpdateShield()
		{
			mShieldAnimationFrame = (mShieldAnimationFrame + 1) % 10;
		}

		public void GiveShield()
		{
			mStatusSecondary |= 1;
			Level.AddSound(ResourceManager.ShieldSound, DisplacementX, DisplacementY);
		}

		#endregion

		#region Speed Shoes

		private int mSpeedShoesDuration;

		private void UpdateSpeedShoes()
		{
			if (mSpeedShoesDuration > 0) {
				mSpeedShoesDuration--;
				if (mSpeedShoesDuration == 0) {
					mTopSpeed = 1536;
					mAcceleration = 12;
					mDeceleration = 128;
					if ((mStatusSecondary & 2) == 0)
						mPlayer.MusicManager.PlayMusic(ResourceManager.EHZMusic);
				}
			}
		}

		public void GiveSpeedShoes()
		{
			mStatusSecondary |= 4;
			mSpeedShoesDuration = 60 * 20;
			mTopSpeed = 0xC00;
			mAcceleration = 24;
			mDeceleration = 128;
			if ((mStatusSecondary & 2) == 0)
				mPlayer.MusicManager.PlayMusic(ResourceManager.EHZSpeedMusic);
		}

		#endregion

		#region Invincibility

		private int mInvincibilityTime;
		private List<LevelObject> mInvincibilityStars = new List<LevelObject>();

		private void UpdateInvincibility()
		{
			// If invincibility time is 0, we are probably just in super mode
			if (mInvincibilityTime == 0)
				return;

			mInvincibilityTime--;
			if (mInvincibilityTime == 0) {
				mStatusSecondary &= ~2;
				if ((mStatusSecondary & 4) == 0)
					mPlayer.MusicManager.PlayMusic(ResourceManager.EHZMusic);
			} else {
				// Star creation
				mInvincibilityStars.RemoveAll(o => o.Finished);
				while (mInvincibilityStars.Count < 10) {
					InvincibilityStar starObject = new InvincibilityStar(Game, Level);
					starObject.DisplacementX = DisplacementX;
					starObject.DisplacementY = DisplacementY;
					Level.Objects.Add(starObject);
					mInvincibilityStars.Add(starObject);
				}
			}
		}

		public void GiveInvincibility()
		{
			if (mSuper)
				return;

			mStatusSecondary |= 2;
			mInvincibilityTime = 60 * 20;

			mPlayer.MusicManager.PlayMusic(ResourceManager.InvincibilityMusic);
		}

		class InvincibilityStar : LevelObject
		{
			private Animation mAnimation;
			private int mAngle;
			private int mInitialDisplacementX;
			private int mInitialDisplacementY;
			private int mStatus;
			private int mDuration;

			private static byte[][] AnimationData = new byte[][] {
				new byte[] { 0, 5, 0, 5, 1, 5, 2, 5, 3, 5, 4, 0xFF },
			};

			public InvincibilityStar(SonicGame game, Level level)
				: base(game, level)
			{
				mAnimation = new Animation(AnimationData);
				mDuration = SonicMaths.Random(4, 18);
				mAnimation.Frame = SonicMaths.Random(8);
				DrawPriority = 100;
			}

			public override void Draw(Graphics g)
			{
				if (mStatus == 0)
					return;

				Rectangle src = new Rectangle(mAnimation.FrameValue * 31 * Game.DisplayScale, 0 * Game.DisplayScale, 31 * Game.DisplayScale, 31 * Game.DisplayScale);
				Rectangle dst = new Rectangle(-15 * Game.DisplayScale, -15 * Game.DisplayScale, 31 * Game.DisplayScale, 31 * Game.DisplayScale);
				g.DrawImage(ResourceManager.InvincibilityTexture, dst, src, Color.White);
			}

			public override void Update()
			{
				if (mStatus == 0)
					Init();

				mAngle = (mAngle + 4) % 256;
				DisplacementX = mInitialDisplacementX + (SonicMaths.Cos(mAngle) * 14 / 256);
				DisplacementY = mInitialDisplacementY + (SonicMaths.Sin(mAngle) * 14 / 256);

				mAnimation.Update();

				mDuration--;
				if (mDuration <= 0)
					Finished = true;
			}

			private void Init()
			{
				mStatus = 1;

				mAngle = SonicMaths.Random(256);
				mInitialDisplacementX = DisplacementX;
				mInitialDisplacementY = DisplacementY;
			}

			public override int Id
			{
				get { return 1; }
			}

			public int Duration
			{
				get
				{
					return mDuration;
				}
				set
				{
					mDuration = value;
				}
			}
		}

		#endregion

		#region Invulnerability

		private bool mInvulnerable;
		private int mInvulnerableTime;

		private void UpdateInvulnerability()
		{
			mInvulnerableTime--;
			if (mInvulnerableTime <= 0)
				mInvulnerable = false;
		}

		public void GiveInvulnerability()
		{
			mInvulnerable = true;
			mInvulnerableTime = 120;
		}

		public bool Invulnerable
		{
			get
			{
				return mInvulnerable;
			}
		}

		#endregion

		#region Collision checking

		/// <summary>
		/// Prevents Sonic from leaving the boundaries of a level.
		/// </summary>
		private void KeepInLevelBoundaries()
		{
			int newDisplacementX = GetNextDisplacementX();
			if (newDisplacementX < Level.PlayerBoundary.X) {
				// Sonic_Boundary_Sides
				DisplacementX = Level.PlayerBoundary.X;
				PartialDisplacementX = 0;
				VelocityX = 0;
				mGroundVelocity = 0;
			} else if (newDisplacementX > Level.PlayerBoundary.X + Level.PlayerBoundary.Width) {
				// Sonic_Boundary_Sides
				DisplacementX = Level.PlayerBoundary.X + Level.PlayerBoundary.Width;
				PartialDisplacementX = 0;
				VelocityX = 0;
				mGroundVelocity = 0;
			}

			// Sonic_Boundary_CheckBottom
			if (DisplacementY >= Level.PlayerBoundary.Y + Level.PlayerBoundary.Height) {
				Kill();
			}
		}

		/// <summary>
		/// Performs level chunk collision checking when airborne.
		/// </summary>
		private void DoLevelCollision()
		{
			int angle, dist, vy, floorAngle;

			angle = SonicMaths.Atan2(VelocityX, VelocityY);
			angle = (angle - 32) & 192;
			if (angle == 64) {
				HitLeftWall();
				return;
			}

			if (angle == 128) {
				HitCeilingAndWalls();
				return;
			}

			if (angle == 192) {
				HitRightWall();
				return;
			}

			dist = CheckLeftWallDist(DisplacementX, DisplacementY, out floorAngle);
			if (dist <= 0) {
				dist -= DisplacementX;
				VelocityX = 0;
			}

			dist = CheckRightWallDist(DisplacementX, DisplacementY, out floorAngle);
			if (dist <= 0) {
				dist += DisplacementX;
				VelocityX = 0;
			}

			dist = CheckFloorDist(out angle, out floorAngle);
			if (dist >= 0)
				return;
			vy = -((VelocityY >> 8) + 8);
			if (dist < vy && angle < vy)
				return;

			// Character has landed on the ground
			DisplacementY += dist;
			SetAngle(floorAngle);
			ResetOnFloor();
			angle = (floorAngle + 32) & 64;
			if (angle == 0) {
				angle = (floorAngle + 16) & 32;
				if (angle != 0) {
					VelocityY >>= 1;
				} else {
					VelocityY = 0;
					mGroundVelocity = VelocityX;
					return;
				}
			} else {
				VelocityX = 0;
				if (VelocityY > 4032)
					VelocityY = 4032;
			}

			if (floorAngle > 0)
				mGroundVelocity = VelocityY;
			else
				mGroundVelocity = -VelocityY;
		}

		private void HitLeftWall()
		{
			int d3;
			int dist = CheckLeftWallDist(DisplacementX, DisplacementY, out d3);
			if (dist > 0) {
				HitCeiling();
				return;
			}

			DisplacementX -= dist;
			VelocityX = 0;
			mGroundVelocity = VelocityY;
		}

		private void HitCeiling()
		{
			int d3;
			int dist = CheckCeilingDist(out d3);
			if (dist > 0) {
				HitFloor();
				return;
			}

			DisplacementY -= dist;
			if (VelocityY > 0)
				return;
			VelocityY = 0;
		}

		private void HitFloor()
		{
			int dist, angle, d0;
			if (VelocityY < 0)
				return;
			dist = CheckFloorDist(out d0, out angle);
			if (dist > 0)
				return;

			DisplacementY += dist;
			SetAngle(angle);
			ResetOnFloor();
			VelocityY = 0;
			mGroundVelocity = VelocityX;
		}

		private void HitCeilingAndWalls()
		{
			int angle, d3;
			int dist = CheckLeftWallDist(DisplacementX, DisplacementY, out d3);
			if (dist < 0) {
				DisplacementX -= dist;
				VelocityX = 0;
			}

			dist = CheckRightWallDist(DisplacementX, DisplacementY, out d3);
			if (dist < 0) {
				DisplacementX += dist;
				VelocityX = 0;
			}

			dist = CheckCeilingDist(out angle);
			if (dist > 0)
				return;

			dist -= DisplacementY;
			if (((angle + 32) & 64) == 0) {
				VelocityY = 0;
				return;
			}

			SetAngle(angle);
			ResetOnFloor();
			if (angle > 0)
				mGroundVelocity = VelocityY;
			else
				mGroundVelocity = -VelocityY;
		}

		private void HitRightWall()
		{
			int d3;
			int dist = CheckRightWallDist(DisplacementX, DisplacementY, out d3);
			if (dist > 0) {
				HitCeiling();
				return;
			}

			DisplacementX += dist;
			VelocityX = 0;
			mGroundVelocity = VelocityY;
		}

		private int CheckLeftWallDist(int x, int y, out int angle)
		{
			int dist;
			Level.FindWallLeft(x - 10, y, mLayer, true, false, out dist, ref mPrimaryAngle);
			if ((mPrimaryAngle & 1) != 0)
				angle = -64;
			else
				angle = mPrimaryAngle;
			return dist;
		}

		private int CheckRightWallDist(int x, int y, out int angle)
		{
			int dist;
			Level.FindWallRight(x + 10, y, mLayer, true, false, out dist, ref mPrimaryAngle);
			if ((mPrimaryAngle & 1) != 0)
				angle = -64;
			else
				angle = mPrimaryAngle;
			return dist;
		}

		private int CheckCeilingDist(out int angle)
		{
			int leftDist, rightDist, minDist;
			Level.FindCeiling(DisplacementX + RadiusX, DisplacementY - RadiusY, mLayer, true, false, out rightDist, ref mPrimaryAngle);
			Level.FindCeiling(DisplacementX + RadiusX, DisplacementY - RadiusY, mLayer, true, false, out leftDist, ref mSecondaryAngle);

			// loc_1ECC6:
			if (leftDist > rightDist) {
				angle = mPrimaryAngle;
				minDist = rightDist;
				// alternativeDistance = leftDist;
			} else {
				angle = mSecondaryAngle;
				minDist = leftDist;
				// alternativeDistance = rightDist;
			}

			if ((angle & 1) != 0)
				angle = 0;

			return minDist;
		}

		private int CheckFloorDist(out int alternativeDistance, out int angle)
		{
			int leftDist, rightDist, minDist;
			Level.FindFloor(DisplacementX + RadiusX, DisplacementY + RadiusY, mLayer, false, true, out rightDist, ref mPrimaryAngle);
			Level.FindFloor(DisplacementX - RadiusX, DisplacementY + RadiusY, mLayer, false, true, out leftDist, ref mSecondaryAngle);

			// loc_1ECC6:
			if (leftDist > rightDist) {
				angle = mPrimaryAngle;
				minDist = rightDist;
				alternativeDistance = leftDist;
			} else {
				angle = mSecondaryAngle;
				minDist = leftDist;
				alternativeDistance = rightDist;
			}

			if ((angle & 1) != 0)
				angle = 0;

			return minDist;
		}

		private int ChkFloorEdge(int d3, int d0)
		{
			int d1;
			int d2 = RadiusY + DisplacementY;
			mPrimaryAngle = 0;
			Level.FindFloor(d3, d2, mLayer, false, true, out d1, ref mPrimaryAngle);
			d3 = mPrimaryAngle;
			if ((d3 & 1) != 0)
				d3 = 0;

			return d1;
		}

		/// <summary>
		/// Subroutine to calculate how much space is in front of Sonic or Tails on the ground.
		/// </summary>
		private int CalcRoomInFront(int angle)
		{
			int d1, d2, d3;

			d3 = GetNextDisplacementX();
			d2 = GetNextDisplacementY();

			// d2 = (d2 << 16) | (d2 >> 16);
			// d3 = (d3 << 16) | (d2 >> 16);
			mPrimaryAngle = angle;
			mSecondaryAngle = angle;
			d1 = angle;
			angle += 32;
			if (angle < 0) {
				angle = d1;
				if (angle < 0)
					angle = 1;
				angle = 32;
			} else {
				angle = d1;
				if (angle < 0)
					angle = 1;
				angle += 31;
			}

			angle &= 192;
			if (angle == 0) {
				d2 += 10;
				Level.FindFloor(d3, d2, mLayer, true, false, out d1, ref mPrimaryAngle);
				d2 = 0;
				d3 = mPrimaryAngle;
				if ((d3 & 1) != 0)
					d3 = d2;
			}

			if (angle == 128) {
				d1 = CheckSlopeDist(d3, d2);
				return d1;
			}

			d1 &= 56;
			if (d1 == 0)
				d2 += 8;

			if (angle == 64)
				d1 = CheckLeftWallDist(d3, d2, out angle);
			else
				d1 = CheckRightWallDist(d3, d2, out angle);
			return d1;
		}

		private int CheckSlopeDist(int x, int y)
		{
			int d;
			Level.FindFloor(x, y, mLayer, true, false, out d, ref mPrimaryAngle);
			return d;
		}

		private int SetAngleFromFloor(int leftDistance, int rightDistance)
		{
			int minDistance = leftDistance;
			int d2 = mSecondaryAngle;
			if (leftDistance > rightDistance) {
				d2 = mPrimaryAngle;
				minDistance = rightDistance;
			}

			if ((d2 & 1) != 0) {
				if (mAngle >= -96 && mAngle < -32)
					SetAngle(-64);
				else if (mAngle >= -32 && mAngle < 32)
					SetAngle(0);
				else if (mAngle >= 32 && mAngle < 96)
					SetAngle(64);
				else
					SetAngle(-128);
				return minDistance;
			}

			int d0 = d2 - mAngle;
			d0 = ((d0 & 128) != 0 ? d0 | -256 : d0 & 256);
			if (d0 < 0)
				d0 = -d0;
			if (d0 >= 32) {
				if (mAngle >= -96 && mAngle < -32)
					SetAngle(-64);
				else if (mAngle >= -32 && mAngle < 32)
					SetAngle(0);
				else if (mAngle >= 32 && mAngle < 96)
					SetAngle(64);
				else
					SetAngle(-128);
				return minDistance;
			}

			SetAngle(d2);
			return minDistance;
		}

		private void SetAngle(int angle)
		{
			Debug.Assert(angle >= -128 && angle < 127, "Angle is not valid for signed-byte");
			mAngle = angle;
		}

		private void TouchResponse()
		{
			foreach (LevelObject obj in Level.Objects.GetObjectsInArea(new Rectangle(DisplacementX - RadiusX - 1, DisplacementY - RadiusY, (RadiusX + 1) * 2, RadiusY * 2)))
				if (obj != this)
					obj.Touch(this);
		}

		private int CheckLeftCeilingDist()
		{
			int leftDistance, rightDistance;

			Level.FindWallLeft(DisplacementX - RadiusY, DisplacementY - RadiusX, mLayer, true, false, out leftDistance, ref mPrimaryAngle);
			Level.FindWallRight(DisplacementX - RadiusY, DisplacementY + RadiusX, mLayer, true, false, out rightDistance, ref mSecondaryAngle);

			return 0;
		}

		private int CalcRoomOverHead(int d0)
		{
			CheckCeilingDist(out d0);

			return 8;
		}

		#endregion

		#region Animation

		private CharacterAnimation mAnim;
		private CharacterAnimation mNextAnim;
		private int mAnimFrame;
		private int mAnimFrameDuration;
		private int mMappingFrame;

		private void Animate()
		{
			if (mAnim != mNextAnim) {
				mNextAnim = mAnim;
				mAnimFrame = 0;
				mAnimFrameDuration = 0;
				mStatus &= ~CharacterState.Pushing;
			}

			if ((int)mAnim >= AnimationData.Length)
				return;

			byte[] animationScript = AnimationData[(int)mAnim];
			if (animationScript[0] > 127) {
				AnimateWalkRun();
				return;
			}

			mAnimFrameDuration--;
			if (mAnimFrameDuration >= 0)
				return;

			mAnimFrameDuration = animationScript[0];
			AnimateGeneric(animationScript);
		}

		private void AnimateGeneric(byte[] animationScript)
		{
			byte asf = animationScript[mAnimFrame + 1];
			switch (asf) {
				case 0xFF:
					mMappingFrame = animationScript[1];
					mAnimFrame = 1;
					break;
				case 0xFE:
					mAnimFrame -= animationScript[mAnimFrame + 2];
					mMappingFrame = animationScript[mAnimFrame + 1];
					mAnimFrame++;
					break;
				case 0xFD:
					mAnim = (CharacterAnimation)animationScript[mAnimFrame + 2];
					break;
				default:
					mMappingFrame = asf;
					mAnimFrame++;
					break;
			}
		}

		private void AnimateWalkRun()
		{
			byte[] animationScript = AnimationData[(int)mAnim];

			if (animationScript[0] != 0xFF) {
				AnimateRoll();
				return;
			}

			if (mFlipAngle != 0) {
				AnimateTumble();
				return;
			}

			// stuff
			// stuff

			if ((mStatus & CharacterState.Pushing) != 0) {
				AnimatePush();
				return;
			}

			int d2 = Math.Abs(mGroundVelocity);
			if (mStatusSecondary < 0)
				d2 *= 2;

			if (d2 >= 1536)
				animationScript = AnimationData[1];
			else
				animationScript = AnimationData[0];

			byte asf = animationScript[mAnimFrame + 1];
			if (asf == 0xFF) {
				mAnimFrame = 0;
				asf = animationScript[1];
			}

			mMappingFrame = asf;
			mAnimFrameDuration--;
			if (mAnimFrameDuration >= 0)
				return;

			d2 = -d2;
			d2 += 2048;
			if (d2 < 0)
				d2 = 0;
			d2 >>= 8;
			mAnimFrameDuration = d2;
			mAnimFrame++;
		}

		private void AnimatePush()
		{
			mAnimFrameDuration--;
			if (mAnimFrameDuration >= 0)
				return;

			int d2 = mGroundVelocity;
			if (d2 >= 0)
				d2 = -d2;
			d2 += 2048;
			if (d2 < 0)
				d2 = 0;
			d2 >>= 6;
			mAnimFrameDuration = d2;
			AnimateGeneric(AnimationData[4]);
		}

		private void AnimateTumble()
		{
			if ((mStatus & CharacterState.FacingLeft) != 0) {
				int d0 = mFlipAngle;
				if (mFlipTurned != 0)
					d0 += 11;
				else
					d0 = -d0 + 143;

				d0 = ((d0 & 0xFF) / 22) + 95;
				mMappingFrame = d0;
				mAnimFrameDuration = 0;
			} else {
				mMappingFrame = (((mFlipAngle + 11) & 0xFF) / 22) + 95;
				mAnimFrameDuration = 0;
			}
		}

		private void AnimateRoll()
		{
			byte[] animationScript = AnimationData[(int)mAnim];

			mAnimFrameDuration--;
			if (mAnimFrameDuration >= 0)
				return;

			if (animationScript[0] != 0xFE) {
				// AnimatePush();
				return;
			}

			int d2 = Math.Abs(mGroundVelocity);
			if (mGroundVelocity >= 1536)
				animationScript = AnimationData[3];
			else
				animationScript = AnimationData[2];

			d2 = -d2;
			d2 += 1024;
			if (d2 < 0)
				d2 = 0;
			d2 >>= 8;

			mAnimFrameDuration = d2;
			AnimateGeneric(animationScript);
		}

		#endregion

		#region Camera

		private void RestoreCameraBias()
		{
			if (mCamera.BiasY < 0)
				mCamera.BiasY += 2;
			else if (mCamera.BiasY > 0)
				mCamera.BiasY -= 2;
		}

		private void CameraScroll()
		{
			HorizontalScrolling();
			VerticalScrolling();
		}

		private void HorizontalScrolling()
		{
			// Delay scrolling if there is a delay set
			if (mCamera.ScrollDelayX > 0) {
				mCamera.ScrollDelayX -= 512;
				return;
			} else {
				mCamera.ScrollDelayX = 0;
			}

			// Is character past camera x or behind camera x - 16
			if (DisplacementX > mCamera.X) {
				// Scroll right
				int difference = DisplacementX - mCamera.X;
				if (difference > 16)
					difference = 16;
				mCamera.X += difference;
			} else if (DisplacementX < mCamera.X - 16) {
				// Scroll left
				int difference = mCamera.X - 16 - DisplacementX;
				if (difference > 16)
					difference = 16;
				mCamera.X -= difference;
			}
		}

		private void VerticalScrolling()
		{
			if ((mStatus & CharacterState.Airborne) == 0) {
				// On ground
				int destY = DisplacementY + mCamera.BiasY;
				int maxChange = (Math.Abs(VelocityY) > 6 ? 16 : 6);
				if (Math.Abs(destY - mCamera.Y) < maxChange)
					mCamera.Y = destY;
				else if (destY > mCamera.Y)
					mCamera.Y += maxChange;
				else
					mCamera.Y -= maxChange;
			} else {
				// In air
				int topBorder = mCamera.Y - 48;
				int bottomBorder = mCamera.Y + 16;
				if (DisplacementY < topBorder) {
					mCamera.Y -= Math.Min(topBorder - DisplacementY, 16);
				} else if (DisplacementY > bottomBorder) {
					mCamera.Y += Math.Min(DisplacementY - bottomBorder, 16);
				}
			}
		}

		public Camera Camera
		{
			get
			{
				return mCamera;
			}
			set
			{
				mCamera = value;
			}
		}

		#endregion
	}
}