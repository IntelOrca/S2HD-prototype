using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SonicRetro.KensSharp;

namespace IntelOrca.Sonic
{
	class LevelConverter
	{
		private int mWidth = 88;
		private int mHeight = 8;
		private byte[,] mBGLayoutTiles;
		private byte[,] mFGLayoutTiles;

		private List<LayoutBlock> mLayoutBlocks = new List<LayoutBlock>();
		private List<TileBlock> mTileBlocks = new List<TileBlock>();

		private byte[] mArtTiles;
		private byte[] mArtPalette;

		private byte[] mPrimaryCollisionIndicies;
		private byte[] mSecondaryCollisionIndicies;

		private sbyte[] mCollisionArray1;
		private sbyte[] mCollisionArray2;

		private sbyte[] mCurveMapping;

		private byte[] mObjects;
		private byte[] mRings;

		public LevelConverter()
		{
			mBGLayoutTiles = new byte[mWidth, mHeight];
			mFGLayoutTiles = new byte[mWidth, mHeight];
			Load();
		}

		public void Load()
		{
			mArtTiles = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\art\kosinski\EHZ_HTZ.bin");

			byte[] pal0 = File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\art\palettes\SonicAndTails.bin");
			byte[] pal1 = File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\art\palettes\EHZ.bin");
			mArtPalette = new byte[pal0.Length + pal1.Length];
			Array.Copy(pal0, 0, mArtPalette, 0, pal0.Length);
			Array.Copy(pal1, 0, mArtPalette, pal0.Length, pal1.Length);

			byte[] layout = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\level\layout\EHZ_1.bin");
			for (int y = 0; y < mHeight; y++) {
				for (int x = 0; x < mWidth; x++) {
					mFGLayoutTiles[x, y] = layout[256 * y + x];
				}
			}

			byte[] blockMappings = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\mappings\128x128\EHZ_HTZ.bin");
			for (int i = 0; i < 256; i++)
				mLayoutBlocks.Add(new LayoutBlock(blockMappings, i * 128));

			blockMappings = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\mappings\16x16\EHZ.bin");
			for (int i = 0; i < 500; i++)
				mTileBlocks.Add(new TileBlock(blockMappings, i * 8));

			mCollisionArray1 = (sbyte[])(Array)File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\collision\Collision array 1.bin");
			mCollisionArray2 = (sbyte[])(Array)File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\collision\Collision array 2.bin");

			mPrimaryCollisionIndicies = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\collision\EHZ and HTZ primary 16x16 collision index.bin");
			mSecondaryCollisionIndicies = Kosinski.Decompress(@"C:\Users\Ted\Documents\Programming\Open Source\S2\collision\EHZ and HTZ secondary 16x16 collision index.bin");
			for (int i = 0; i < mTileBlocks.Count; i++)
				mTileBlocks[i].SetCollision(mPrimaryCollisionIndicies[i], mSecondaryCollisionIndicies[i]);

			mCurveMapping = (sbyte[])(Array)File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\collision\Curve and resistance mapping.bin");

			mObjects = File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\level\objects\EHZ_1.bin");
			mRings = File.ReadAllBytes(@"C:\Users\Ted\Documents\Programming\Open Source\S2\level\rings\EHZ_1.bin");

			FileStream fs = new FileStream(@"C:\Users\Ted\Documents\programming\Projects\Games\S2HD\assets\levels\ehz1\objects.dat", FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);

			// Objects
			List<object> objects = new List<object>();

			// Player start
			// objects.Add(new { Id = 1, Subtype = 0, X = 0x0060, Y = 0x028F, Respawn = false, Xflip = false, Yflip = false });

			for (int i = 0; i < mObjects.Length; i += 6) {
				short x = BitConverter.ToInt16(new byte[] { mObjects[i + 1], mObjects[i + 0] }, 0);
				short y = BitConverter.ToInt16(new byte[] { mObjects[i + 3], (byte)(mObjects[i + 2] & 0x0F) }, 0);
				byte id = mObjects[i + 4];
				byte subtype = mObjects[i + 5];
				bool respawn = ((mObjects[i + 2] & 0x80) == 0);
				bool yflip = ((mObjects[i + 2] & 0x40) != 0);
				bool xflip = ((mObjects[i + 2] & 0x20) != 0);

				objects.Add(new { Id = (int)id, Subtype = (int)subtype, X = (int)x, Y = (int)y, Respawn = respawn, Xflip = xflip, Yflip = yflip });
			}

			for (int i = 0; i < mRings.Length; i += 4) {
				short x = BitConverter.ToInt16(new byte[] { mRings[i + 1], mRings[i + 0] }, 0);
				if (x == -1)
					break;
				short y = BitConverter.ToInt16(new byte[] { mRings[i + 3], (byte)(mRings[i + 2] & 0x0F) }, 0);
				int t = mRings[i + 2] >> 4;
				bool column = ((t & 8) != 0);
				int c = (t & 7) + 1;

				for (int j = 0; j < c; j++) {
					objects.Add(new { Id = 37, Subtype = 0, X = (int)x, Y = (int)y, Respawn = false, Xflip = false, Yflip = false });

					if (column)
						y += 24;
					else
						x += 24;
				}
			}

			int[] swapIdA = new int[] { 3,  6,   13, 17,  24,  28,  37, 38, 54, 65, 73,  75, 92, 121, 157 };
			int[] swapIdB = new int[] { 38, 131, 19, 128, 130, 129, 16, 17, 49, 48, 132, 64, 66, 18,  65 };

			foreach (dynamic obj in objects) {
				int id = 0;
				for (int i = 0; i < swapIdA.Length; i++)
					if (obj.Id == swapIdA[i])
						id = swapIdB[i];

				if (id == 0)
					continue;				

				bw.Write(id);
				bw.Write((int)obj.Subtype);

				int flags = 0;
				if (obj.Xflip)
					flags |= 1;
				if (obj.Yflip)
					flags |= 2;				
				if (obj.Respawn)
					flags |= 4;

				bw.Write(flags);
				bw.Write((int)obj.X);
				bw.Write((int)obj.Y);
			}

			bw.Close();
			fs.Close();
		}

		public void Convert()
		{
			string directory = Path.GetFullPath(@"data\levels\ehz");

			Landscape landscape = new Landscape();
			int chunkId = 0;
			foreach (LayoutBlock chunk in mLayoutBlocks) {
				Landscape.Chunk lchunk = new Landscape.Chunk();
				for (int layer = 0; layer < 2; layer++) {
					Landscape.Chunk.Layer lclayer = new Landscape.Chunk.Layer();

					// Angles
					for (int y = 0; y < 8; y++) {
						for (int x = 0; x < 8; x++) {
							LayoutBlock.LayoutBlockTile chunkBlock = chunk.Tiles[x, y];
							if (chunkBlock.Index >= mTileBlocks.Count) {
								lclayer.Angles[x, y] = 0;
								continue;
							}

							TileBlock block = mTileBlocks[chunkBlock.Index];
							int ci = (layer == 0 ? block.SecondaryCollision : block.PrimaryCollision);
							sbyte angle = mCurveMapping[ci];
							if (chunkBlock.X)
								angle = (sbyte)-angle;
							if (chunkBlock.Y)
								angle = (sbyte)(-(angle + 64) - 64);

							lclayer.Angles[x, y] = angle;
						}
					}

					// LRB
					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							int blockX = x / 16;
							int blockY = y / 16;
							int tileX = (x % 16) / 2;
							int tileY = (y % 16) / 2;

							LayoutBlock.LayoutBlockTile chunkBlock = chunk.Tiles[blockX, blockY];

							if (layer == 0) {
								if ((chunkBlock.SS & 2) == 0) {
									lclayer.CollisionLRB[x, y] = false;
									continue;
								}
							} else {
								if ((chunkBlock.TT & 2) == 0) {
									lclayer.CollisionLRB[x, y] = false;
									continue;
								}
							}

							if (chunkBlock.Index >= mTileBlocks.Count) {
								lclayer.CollisionLRB[x, y] = false;
								continue;
							}

							int px = x % 16;
							int py = y % 16;
							if (chunkBlock.X)
								px = 15 - px;
							if (chunkBlock.Y)
								py = 15 - py;

							TileBlock block = mTileBlocks[chunkBlock.Index];
							int ci = (layer == 0 ? block.SecondaryCollision : block.PrimaryCollision);
							sbyte collision = mCollisionArray1[ci * 16 + px];
							bool solid;
							if (collision == 0)
								solid = false;
							else if (collision > 0)
								solid = collision >= (16 - py);
							else
								throw new Exception();

							lclayer.CollisionLRB[x, y] = solid;
						}
					}

					// TOP
					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							int blockX = x / 16;
							int blockY = y / 16;
							int tileX = (x % 16) / 2;
							int tileY = (y % 16) / 2;

							LayoutBlock.LayoutBlockTile chunkBlock = chunk.Tiles[blockX, blockY];

							if (layer == 0) {
								if ((chunkBlock.SS & 1) == 0) {
									lclayer.CollisionT[x, y] = false;
									continue;
								}
							} else {
								if ((chunkBlock.TT & 1) == 0) {
									lclayer.CollisionT[x, y] = false;
									continue;
								}
							}

							if (chunkBlock.Index >= mTileBlocks.Count) {
								lclayer.CollisionT[x, y] = false;
								continue;
							}

							int px = x % 16;
							int py = y % 16;
							if (chunkBlock.X)
								px = 15 - px;
							if (chunkBlock.Y)
								py = 15 - py;

							TileBlock block = mTileBlocks[chunkBlock.Index];
							int ci = (layer == 0 ? block.SecondaryCollision : block.PrimaryCollision);
							sbyte collision = mCollisionArray1[ci * 16 + px];
							bool solid;
							if (collision == 0)
								solid = false;
							else if (collision > 0)
								solid = collision >= (16 - py);
							else
								throw new Exception();

							lclayer.CollisionT[x, y] = solid;
						}
					}

					// Art
					Landscape.Chunk.Layer.Image lcli = new Landscape.Chunk.Layer.Image();
					lcli.AnimationDuration = 0;
					lcli.AnimationFrames = 1;
					Color[] bits128 = GetFGChunkArt(chunkId, (layer == 0), (layer == 1));
					Color[] bits512 = new Color[512 * 512];
					for (int y = 0; y < 512; y++)
						for (int x = 0; x < 512; x++)
							bits512[y * 512 + x] = bits128[(y / 4) * 128 + (x / 4)];
					lcli.FrameData = new byte[1][];

					MemoryStream ms2 = new MemoryStream();
					BinaryWriter bw2 = new BinaryWriter(ms2);
					for (int i = 0; i < 512 * 512; i++) {
						bw2.Write(bits512[i].R);
						bw2.Write(bits512[i].G);
						bw2.Write(bits512[i].B);
						bw2.Write(bits512[i].A);
					}
					lcli.FrameData[0] = ms2.ToArray();
					ms2.Close();

					lclayer.Art = lcli;

					if (layer == 0)
						lchunk.BackLayer = lclayer;
					else
						lchunk.FrontLayer = lclayer;
				}

				landscape.Chunks.Add(lchunk);

				chunkId++;
			}

			landscape.Save(directory + "\\landscape.dat");

			return;

			// Level
			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			// Layout
			bw.Write("Emerald Hill, Act 1");
			bw.Write("Sega");
			bw.Write(88);
			bw.Write(8);
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 88; x++)
					bw.Write(mFGLayoutTiles[x, y]);

			// Objects
			List<object> objects = new List<object>();

			// Player start
			objects.Add(new { Id = 1, Subtype = 0, X = 0x0060, Y = 0x028F, Respawn = false, Xflip = false, Yflip = false });

			for (int i = 0; i < mObjects.Length; i += 6) {
				short x = BitConverter.ToInt16(new byte[] { mObjects[i + 1], mObjects[i + 0] }, 0);
				short y = BitConverter.ToInt16(new byte[] { mObjects[i + 3], (byte)(mObjects[i + 2] & 0x0F) }, 0);
				byte id = mObjects[i + 4];
				byte subtype = mObjects[i + 5];
				bool respawn = ((mObjects[i + 2] & 0x80) == 0);
				bool xflip = ((mObjects[i + 2] & 0x40) != 0);
				bool yflip = ((mObjects[i + 2] & 0x20) != 0);

				objects.Add(new { Id = (int)id, Subtype = (int)subtype, X = (int)x, Y = (int)y, Respawn = respawn, Xflip = xflip, Yflip = yflip });
			}

			for (int i = 0; i < mRings.Length; i += 4) {
				short x = BitConverter.ToInt16(new byte[] { mRings[i + 1], mRings[i + 0] }, 0);
				if (x == -1)
					break;
				short y = BitConverter.ToInt16(new byte[] { mRings[i + 3], (byte)(mRings[i + 2] & 0x0F) }, 0);
				int t = mRings[i + 2] >> 4;
				bool column = ((t & 8) != 0);
				int c = (t & 7) + 1;

				for (int j = 0; j < c; j++) {
					objects.Add(new { Id = 37, Subtype = 0, X = (int)x, Y = (int)y, Respawn = false, Xflip = false, Yflip = false });

					if (column)
						y += 24;
					else
						x += 24;
				}
			}

			bw.Write(objects.Count);
			foreach (dynamic obj in objects) {
				bw.Write(obj.Id);
				bw.Write(obj.Subtype);
				bw.Write(obj.X);
				bw.Write(obj.Y);
				bw.Write(obj.Respawn);
				bw.Write(obj.Xflip);
				bw.Write(obj.Yflip);
			}

			bw.Close();
			File.WriteAllBytes(directory + "\\act1.dat", ms.ToArray());
		}

		public Bitmap GetFGChunkImagesAsOne(bool back, bool front)
		{
			Bitmap[] chunkBmps = GetFGChunkImages(back, front);

			int chunksWide = Math.Min(chunkBmps.Length, 8);
			int chunksHigh = (int)Math.Ceiling(chunkBmps.Length / (double)chunksWide);

			Bitmap bmp = new Bitmap(chunksWide * 128, chunksHigh * 128);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);

			int i = 0;
			for (int y = 0; y < chunksHigh; y++) {
				for (int x = 0; x < chunksWide; x++) {
					if (i >= chunkBmps.Length)
						break;

					g.DrawImage(chunkBmps[i], x * 128, y * 128);
					i++;
				}
			}

			g.Dispose();
			return bmp;
		}

		public Bitmap[] GetFGChunkImages(bool back, bool front)
		{
			Bitmap[] bmps = new Bitmap[mLayoutBlocks.Count];
			for (int i = 0; i < bmps.Length; i++)
				bmps[i] = GetFGChunkImage(i, back, front);
			return bmps;
		}

		public Bitmap GetFGChunkImage(int chunkId, bool back, bool front)
		{
			Color[] bits = GetFGChunkArt(chunkId, back, front);
			Bitmap bmp = new Bitmap(128, 128);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
			for (int y = 0; y < 128; y++)
				for (int x = 0; x < 128; x++)
					g.FillRectangle(new SolidBrush(bits[y * 128 + x]), x, y, 1, 1);
			g.Dispose();
			return bmp;
		}

		public Color[] GetFGChunkArt(int chunkId, bool back, bool front)
		{
			Color[] pixels = new Color[128 * 128];
			LayoutBlock chunk = mLayoutBlocks[chunkId];
			for (int y = 0; y < 128; y++) {
				for (int x = 0; x < 128; x++) {
					int sx, sy, dx, dy;

					LayoutBlock.LayoutBlockTile chunkTile = chunk.Tiles[x / 16, y / 16];
					if (chunkTile.Index >= mTileBlocks.Count) {
						pixels[y * 128 + x] = Color.Transparent;
						continue;
					}

					dx = x % 16;
					dy = y % 16;
					if (chunkTile.X)
						dx = 15 - dx;
					if (chunkTile.Y)
						dy = 15 - dy;

					dx += (x / 16) * 16;
					dy += (y / 16) * 16;

					TileBlock block = mTileBlocks[chunkTile.Index];
					TileBlock.TileBlockTile blockTile = block.Tiles[(x % 16) / 8, (y % 16) / 8];
					if (blockTile.P && !front)
						continue;
					if (!blockTile.P && !back)
						continue;

					sx = x % 8;
					sy = y % 8;
					if (blockTile.X)
						sx = 7 - sx;
					if (blockTile.Y)
						sy = 7 - sy;

					pixels[dy * 128 + dx] = GetArtTilePixel(blockTile.CC, blockTile.Index, sx, sy);
				}
			}

			return pixels;
		}

		private Color GetArtTilePixel(int palette, int tileIndex, int x, int y)
		{
			byte pixel = mArtTiles[tileIndex * 32 + (y * 4) + (x / 2)];
			if (x % 2 == 0)
				return GetPaletteColour((pixel >> 4) + (palette * 16));
			else
				return GetPaletteColour((pixel & 0x0F) + (palette * 16));
		}

		private Color GetPaletteColour(int index)
		{
			if (index % 16 == 0)
				return Color.Transparent;

			byte ab = mArtPalette[index * 2];
			byte gr = mArtPalette[index * 2 + 1];
			return Color.FromArgb((gr & 0x0F) * 16, (gr >> 4) * 16, (ab & 0x0F) * 16);
		}

		class LayoutBlock
		{
			private LayoutBlockTile[,] mTiles;

			public LayoutBlock(byte[] data, int index)
			{
				mTiles = new LayoutBlockTile[8, 8];
				for (int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
						mTiles[x, y] = new LayoutBlockTile(BitConverter.ToInt16(data, index + (y * 16) + (x * 2)));
			}

			public LayoutBlockTile[,] Tiles
			{
				get
				{
					return mTiles;
				}
			}

			public struct LayoutBlockTile
			{
				// SSTT YXII IIII IIII
				private short mData;

				public LayoutBlockTile(short data)
				{
					byte[] databytes = BitConverter.GetBytes(data);
					byte tmp = databytes[0];
					databytes[0] = databytes[1];
					databytes[1] = tmp;
					mData = BitConverter.ToInt16(databytes, 0);
				}

				public short Data
				{
					get
					{
						return mData;
					}
				}

				public int SS
				{
					get
					{
						return mData >> 14;
					}
				}

				public int TT
				{
					get
					{
						return (mData >> 12) & 3;
					}
				}

				public bool Y
				{
					get
					{
						return ((mData >> 11) & 1) > 0;
					}
				}

				public bool X
				{
					get
					{
						return ((mData >> 10) & 1) > 0;
					}
				}

				public int Index
				{
					get
					{
						return mData & 0x03FF;
					}
				}
			}
		}

		class TileBlock
		{
			private TileBlockTile[,] mTiles;
			private int mPrimaryCollision;
			private int mSecondaryCollision;

			public TileBlock(byte[] data, int index)
			{
				mTiles = new TileBlockTile[2, 2];
				for (int y = 0; y < 2; y++)
					for (int x = 0; x < 2; x++)
						mTiles[x, y] = new TileBlockTile(BitConverter.ToInt16(data, index + (y * 4) + (x * 2)));
			}

			public void SetCollision(int primary, int secondary)
			{
				mPrimaryCollision = primary;
				mSecondaryCollision = secondary;
			}

			public TileBlockTile[,] Tiles
			{
				get
				{
					return mTiles;
				}
			}

			public int PrimaryCollision
			{
				get
				{
					return mPrimaryCollision;
				}
			}

			public int SecondaryCollision
			{
				get
				{
					return mSecondaryCollision;
				}
			}

			public struct TileBlockTile
			{
				// PCCY XAAA AAAA AAAA
				private short mData;

				public TileBlockTile(short data)
				{
					byte[] databytes = BitConverter.GetBytes(data);
					byte tmp = databytes[0];
					databytes[0] = databytes[1];
					databytes[1] = tmp;
					mData = BitConverter.ToInt16(databytes, 0);
				}

				public bool P
				{
					get
					{
						return ((mData >> 15) & 1) > 0;
					}
				}

				public int CC
				{
					get
					{
						return (mData >> 13) & 3;
					}
				}

				public bool Y
				{
					get
					{
						return ((mData >> 12) & 1) > 0;
					}
				}

				public bool X
				{
					get
					{
						return ((mData >> 11) & 1) > 0;
					}
				}

				public int Index
				{
					get
					{
						return mData & 0x07FF;
					}
				}
			}
		}
	}
}
