using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace IntelOrca.Sonic
{
	class Level
	{
		private SonicGame mGame;

		private string mName;
		private int mZoneIndex;
		private int mActIndex;

		private int mStartX;
		private int mStartY;
		private Microsoft.Xna.Framework.Rectangle mPlayerBoundary;
		private Microsoft.Xna.Framework.Rectangle mVisibleBoundary;

		private List<Sound> mSounds = new List<Sound>();

		// Level chunks
		private int mChunkColumns;
		private int mChunkRows;
		public byte[,] mChunkLayout;
		// private List<Chunk> mChunks = new List<Chunk>();
		private Landscape mLandscape;
		private string mDirectory;

		// Level objects
		private List<LevelObjectDefinition> mObjectDefinitions = new List<LevelObjectDefinition>();
		private LevelObjectManager mObjects = new LevelObjectManager();

		public Level(SonicGame game)
		{
			mGame = game;
		}

		public void Load(string directory)
		{
			mDirectory = directory;
			mName = "Emerald Hill";
			mZoneIndex = 1;
			mActIndex = 1;

			mPlayerBoundary = new Microsoft.Xna.Framework.Rectangle(16, 0, 11264 - 16, 1024);
			mVisibleBoundary = new Microsoft.Xna.Framework.Rectangle(0, 0, 10976, 1024);

			mLandscape = new Landscape(mDirectory + "\\landscape.dat");
			LoadLayout(mDirectory + "\\act1.dat");

			// for (int i = 0; i < 256; i++)
				// mChunks.Add(new Chunk());

			// LoadChunks(directory + "\\chunks.dat", directory + "\\fgback.png", directory + "\\fgfront.png");
		}

		public void Restart()
		{
			mObjects = new LevelObjectManager();
			mSounds.Clear();

			mPlayerBoundary = new Microsoft.Xna.Framework.Rectangle(16, 0, 11264 - 16, 1024);
			mVisibleBoundary = new Microsoft.Xna.Framework.Rectangle(0, 0, 10976, 1024);

			mObjects.Clear();
			foreach (LevelObjectDefinition def in mObjectDefinitions) {
				LevelObject obj = LevelObject.Create(mGame, this, def);
				if (obj != null)
					mObjects.Add(obj);
			}
		}

		public void Update()
		{
			// Clear the sounds
			mSounds.Clear();

			mObjects.Update();
		}

		public Character GetClosestCharacter(LevelObject obj, out int directionX, out int directionY, out int horizontalDistance, out int verticalDistance)
		{
			List<Character> characters = new List<Character>();
			foreach (LevelObject o in mObjects)
				if (o is Character)
					characters.Add((Character)o);

			Character closestCharacter = null;
			int lowestHorizontalDistance = -1;

			directionX = 0;
			directionY = 0;
			horizontalDistance = 0;
			verticalDistance = 0;

			foreach (Character character in characters) {
				int hDist = obj.DisplacementX - character.DisplacementX;
				int vDist = obj.DisplacementY - character.DisplacementY;
				int ahDist = Math.Abs(hDist);

				if (closestCharacter != null && ahDist >= lowestHorizontalDistance)
					continue;

				closestCharacter = character;
				horizontalDistance = hDist;
				verticalDistance = vDist;

				// Is player to object's left
				if (hDist < 0)
					directionX = 2;

				// Is player under object
				if (vDist < 0)
					directionY = 2;
			}

			return closestCharacter;
		}

		private void LoadLayout(string path)
		{
			FileStream fs = new FileStream(path, FileMode.Open);
			BinaryReader br = new BinaryReader(fs);

			br.ReadString();
			br.ReadString();

			// Chunk layout
			mChunkColumns = br.ReadInt32();
			mChunkRows = br.ReadInt32();
			mChunkLayout = new byte[mChunkColumns, mChunkRows];
			for (int y = 0; y < mChunkRows; y++)
				for (int x = 0; x < mChunkColumns; x++)
					mChunkLayout[x, y] = br.ReadByte();

			// Objects
			mObjectDefinitions.Clear();
			int numObjects = br.ReadInt32();
			for (int i = 0; i < numObjects; i++) {
				LevelObjectDefinition definition = new LevelObjectDefinition();

				definition.Id = br.ReadInt32();
				definition.SubType = br.ReadInt32();
				definition.DisplacementX = br.ReadInt32();
				definition.DisplacementY = br.ReadInt32();
				definition.Respawn = br.ReadBoolean();
				definition.FlipY = br.ReadBoolean();
				definition.FlipX = br.ReadBoolean();

				if (definition.Id == 1) {
					mStartX = definition.DisplacementX;
					mStartY = definition.DisplacementY;
				}

				LevelObject obj = LevelObject.Create(mGame, this, definition);
				if (obj != null)
					mObjects.Add(obj);

				mObjectDefinitions.Add(definition);
			}

			br.Close();
			fs.Close();
		}

		public void FindCeiling(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = y + 1; i < y + 32; i++) {
					if (IsSolid(x, i, layer, lrb, t))
						continue;
					distance = y - i;
					angle = GetAngle(x, i, layer);
					return;
				}

				distance = -32;
			}

			for (int i = y - 1; i > y - 32; i--) {
				if (!IsSolid(x, i, layer, lrb, t))
					continue;
				distance = y - i - 1;
				angle = GetAngle(x, i, layer);
				return;
			}

			distance = 32;
		}

		public void FindFloor(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = y - 1; i > y - 32; i--) {
					if (IsSolid(x, i, layer, lrb, t))
						continue;
					distance = i - y;
					angle = GetAngle(x, i, layer);
					return;
				}

				distance = -32;
			}

			for (int i = y + 1; i < y + 32; i++) {
				if (!IsSolid(x, i, layer, lrb, t))
					continue;
				distance = i - y - 1;
				angle = GetAngle(x, i, layer);
				return;
			}

			distance = 32;
		}

		public void FindWallLeft(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = x + 1; i < x + 32; i++) {
					if (IsSolid(i, y, layer, lrb, t))
						continue;
					distance = x - i;
					angle = GetAngle(i, y, layer);
					return;
				}

				distance = -32;
			}

			for (int i = x - 1; i > x - 32; i--) {
				if (!IsSolid(i, y, layer, lrb, t))
					continue;
				distance = x - i - 1;
				angle = GetAngle(i, y, layer);
				return;
			}

			distance = 32;
		}

		public void FindWallRight(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = x - 1; i > x - 32; i--) {
					if (IsSolid(i, y, layer, lrb, t))
						continue;
					distance = i - x;
					angle = GetAngle(i, y, layer);
					return;
				}

				distance = -32;
			}

			for (int i = x + 1; i < x + 32; i++) {
				if (!IsSolid(i, y, layer, lrb, t))
					continue;
				distance = i - x - 1;
				angle = GetAngle(i, y, layer);
				return;
			}

			distance = 32;
		}

		public bool IsSolid(int x, int y, int layer, bool lrb, bool t)
		{
			int chunkX = x / 128;
			int chunkY = y / 128;
			int blockX = (x % 128);
			int blockY = (y % 128);

			if (x < 0 || y < 0)
				return false;

			if (chunkX >= mChunkColumns || chunkY >= mChunkRows)
				return false;

			Landscape.Chunk chunk = mLandscape.Chunks[mChunkLayout[chunkX, chunkY]];
			Landscape.Chunk.Layer clayer = (layer == 0 ? chunk.BackLayer : chunk.FrontLayer);
			
			bool solid = false;
			if (lrb)
				solid = clayer.CollisionLRB[blockX, blockY];
			if (t && !solid)
				solid = clayer.CollisionT[blockX, blockY];
			return solid;
		}

		private int GetAngle(int x, int y, int layer)
		{
			int chunkX = x / 128;
			int chunkY = y / 128;
			int blockX = (x % 128) / 16;
			int blockY = (y % 128) / 16;

			if (x < 0 || y < 0)
				return 0;

			if (chunkX >= mChunkColumns || chunkY >= mChunkRows)
				return 0;

			Landscape.Chunk chunk = mLandscape.Chunks[mChunkLayout[chunkX, chunkY]];
			Landscape.Chunk.Layer clayer = (layer == 0 ? chunk.BackLayer : chunk.FrontLayer);
			return clayer.Angles[blockX, blockY];
		}

		public void AddSound(SoundEffect soundEffect, int x, int y)
		{
			AddSound(new Sound(soundEffect, x, y));
		}

		public void AddSound(Sound sound)
		{
			mSounds.Add(sound);
		}

		public IEnumerable<Sound> Sounds
		{
			get
			{
				return mSounds;
			}
		}

		public int Width
		{
			get
			{
				return mChunkColumns;
			}
		}

		public int Height
		{
			get
			{
				return mChunkRows;
			}
		}

		public byte[,] LevelLayout
		{
			get
			{
				return mChunkLayout;
			}
		}

		public LevelObjectManager Objects
		{
			get
			{
				return mObjects;
			}
		}

		public string Name
		{
			get
			{
				return mName;
			}
		}

		public int ZoneIndex
		{
			get
			{
				return mZoneIndex;
			}
		}

		public int ActIndex
		{
			get
			{
				return mActIndex;
			}
		}

		public Microsoft.Xna.Framework.Rectangle PlayerBoundary
		{
			get
			{
				return mPlayerBoundary;
			}
			set
			{
				mPlayerBoundary = value;
			}
		}

		public Microsoft.Xna.Framework.Rectangle VisibleBoundary
		{
			get
			{
				return mVisibleBoundary;
			}
			set
			{
				mVisibleBoundary = value;
			}
		}

		public int StartX
		{
			get
			{
				return mStartX;
			}
		}

		public int StartY
		{
			get
			{
				return mStartY;
			}
		}

		public Landscape Landscape
		{
			get
			{
				return mLandscape;
			}
		}

		public struct Sound
		{
			private SoundEffect mSoundEffect;
			private int mDisplacementX;
			private int mDisplacementY;

			public Sound(SoundEffect soundEffect, int x, int y)
			{
				mSoundEffect = soundEffect;
				mDisplacementX = x;
				mDisplacementY = y;
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

			public SoundEffect SoundEffect
			{
				get
				{
					return mSoundEffect;
				}
				set
				{
					mSoundEffect = value;
				}
			}
		}
	}
}
