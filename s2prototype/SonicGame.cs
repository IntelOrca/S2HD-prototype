using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IntelOrca.Sonic
{
	class SonicGame : Game
	{
		private GraphicsDeviceManager mGraphicsManager;
		private SpriteBatch mSpriteBatch;
		private GameScreen mCurrentScreen;
		private ControllerState mControllerA;

		private Level mLevel;
		private List<Player> mPlayers = new List<Player>();
		private bool mLevelStarted;

		// Step debugging
		private bool mStepping;
		private bool mStepped;
		private bool mStepNow;

		private RenderTarget2D mDisplaySurface;
		private int mDisplayWidth = 1920;
		private int mDisplayHeight = 1080;
		private int mDisplayScale = 4;

		private int mFrameRate = 0;
		private int mFrameCounter = 0;
		private TimeSpan mElapsedTime = TimeSpan.Zero;

		public SonicGame()
		{
			mGraphicsManager = new GraphicsDeviceManager(this);
			mGraphicsManager.PreferredBackBufferWidth = 960;
			mGraphicsManager.PreferredBackBufferHeight = 540;
			mGraphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// Create the display surface
			mDisplaySurface = new RenderTarget2D(GraphicsDevice, mDisplayWidth, mDisplayHeight);

			// Create the sprite batch
			mSpriteBatch = new SpriteBatch(GraphicsDevice);

			// Initialise the resource manager
			ResourceManager.GraphicsDevice = GraphicsDevice;
			ResourceManager.PlainTexture = ResourceManager.CreateTexture(mDisplayWidth, mDisplayHeight, Color.White);
			ResourceManager.LoadResources();

			StartEmeraldHillZoneAct1();
		}

		protected override bool BeginDraw()
		{
			GraphicsDevice.SetRenderTarget(mDisplaySurface);
			return base.BeginDraw();
		}

		protected override void Draw(GameTime gameTime)
		{
			mFrameCounter++;
			Window.Title = "s2prototype, fps: " + mFrameRate;

			GraphicsDevice.Clear(Color.Black);
			mSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			Graphics graphics = new Graphics(mSpriteBatch);
			if (mCurrentScreen != null)
				mCurrentScreen.Draw(graphics);

			mSpriteBatch.End();
		}

		protected override void EndDraw()
		{
			GraphicsDevice.SetRenderTarget(null);
			mSpriteBatch.Begin();
			mSpriteBatch.Draw(mDisplaySurface, GraphicsDevice.Viewport.Bounds, Color.White);
			mSpriteBatch.End();
			base.EndDraw();
		}

		protected override void Update(GameTime gameTime)
		{
			mElapsedTime += gameTime.ElapsedGameTime;
			if (mElapsedTime > TimeSpan.FromSeconds(1)) {
				mElapsedTime -= TimeSpan.FromSeconds(1);
				mFrameRate = mFrameCounter;
				mFrameCounter = 0;
			}


			ControllerState state = new ControllerState();
			state.Set(1, Keyboard.GetState().IsKeyDown(Keys.Up));
			state.Set(2, Keyboard.GetState().IsKeyDown(Keys.Down));
			state.Set(4, Keyboard.GetState().IsKeyDown(Keys.Left));
			state.Set(8, Keyboard.GetState().IsKeyDown(Keys.Right));
			state.Set(32, Keyboard.GetState().IsKeyDown(Keys.A));
			state.Set(64, Keyboard.GetState().IsKeyDown(Keys.S));
			state.Set(128, Keyboard.GetState().IsKeyDown(Keys.D));
			mControllerA = state;

			// Step debugging
			if (Keyboard.GetState().IsKeyDown(Keys.OemPipe)) {
				if (!mStepping) {
					mStepping = true;
					mStepped = true;
				} else if (!mStepped) {
					mStepNow = true;
					mStepped = true;
				}
			} else {
				mStepped = false;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				mStepping = false;
			}

			if (mStepping && !mStepNow)
				return;
			mStepNow = false;

			if (mCurrentScreen != null)
				mCurrentScreen.Update();

			if (mLevelStarted) {
				mLevel.Update();

				foreach (Player player in mPlayers)
					player.Update();

				if (mPlayers[0].Status == PlayerStatus.Dead)
					RestartLevel();
			} else if (mPlayers.Count > 0) {
				if (mPlayers.All(p => p.Status != PlayerStatus.NotReady))
					mLevelStarted = true;
			}
		}

		private void StartEmeraldHillZoneAct1()
		{
			LoadingScreen loadingScreen = new LoadingScreen(this);
			loadingScreen.LoadingRoutine = new Action(LoadEmeraldHillZoneAct1);
			loadingScreen.FinishedRoutine = BeginLevel;
			loadingScreen.Begin();

			mCurrentScreen = loadingScreen;
		}

		private void LoadEmeraldHillZoneAct1()
		{
			mLevelStarted = false;
			mLevel = new Level(this);
			mLevel.Load("data\\levels\\ehz");

			ResourceManager.ChunkTexturesBack = new List<Texture2D>();
			ResourceManager.ChunkTexturesFront = new List<Texture2D>();
			for (int i = 0; i < mLevel.Landscape.Chunks.Count; i++) {
				ResourceManager.ChunkTexturesBack.Add(GetChunkTexture(i, 0));
				ResourceManager.ChunkTexturesFront.Add(GetChunkTexture(i, 1));
			}

			GC.Collect();
		}

		private void RestartLevel()
		{
			mPlayers[0].MusicManager.StopAllMusic();

			mLevel.Restart();
			mPlayers[0].Restart();
			BeginLevel();
		}

		private void BeginLevel()
		{
			mLevelStarted = false;

			Player mainPlayer;
			if (mPlayers.Count == 0)
				mPlayers.Add(new Player(this));

			mainPlayer = mPlayers[0];
			mainPlayer.SetupCharacter();
			mainPlayer.MainCharacter.Player = mainPlayer;			
			mLevel.Objects.Add(mainPlayer.MainCharacter);

			mPlayers = new List<Player>();
			mPlayers.Add(mainPlayer);

			// Initial update to get started
			mLevel.Update();

			LevelScreen levelScreen = new LevelScreen(this);
			mCurrentScreen = levelScreen;

			mPlayers[0].MusicManager.PlayMusic(ResourceManager.EHZMusic);
		}

		private Texture2D GetChunkTexture(int id, int layer)
		{
			byte[] data;
			if (layer == 0)
				data = mLevel.Landscape.Chunks[id].BackLayer.Art.FrameData[0];
			else
				data = mLevel.Landscape.Chunks[id].FrontLayer.Art.FrameData[0];


			Texture2D texture2 = new Texture2D(GraphicsDevice, 512, 512);
			texture2.SetData(data);

			if (layer == 0)
				mLevel.Landscape.Chunks[id].BackLayer.Art.FrameData = null;
			else
				mLevel.Landscape.Chunks[id].FrontLayer.Art.FrameData = null;
			return texture2;
		}

		public int DisplayWidth
		{
			get
			{
				return mDisplayWidth;
			}
		}

		public int DisplayHeight
		{
			get
			{
				return mDisplayHeight;
			}
		}

		public int DisplayScale
		{
			get
			{
				return mDisplayScale;
			}
		}

		public Level Level
		{
			get
			{
				return mLevel;
			}
		}

		public List<Player> Players
		{
			get
			{
				return mPlayers;
			}
		}

		public ControllerState ControllerA
		{
			get
			{
				return mControllerA;
			}
		}
	}
}
