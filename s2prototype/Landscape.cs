using System;
using System.Collections.Generic;
using System.IO;

namespace IntelOrca.Sonic
{
	class Landscape
	{
		private string mInformation = String.Empty;
		private List<Chunk> mChunks = new List<Chunk>();

		public Landscape()
		{
		}

		public Landscape(string file)
		{
			Open(file);
		}

		public Landscape(Stream stream)
		{
			Open(stream);
		}

		private bool Open(string file)
		{
			using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
				return Open(fs);
		}

		private bool Open(Stream stream)
		{
			BinaryReader br = new BinaryReader(stream);
			int magicNumber = br.ReadInt32();
			mInformation = br.ReadString();
			int version = br.ReadInt32();
			int compression = br.ReadInt32();
			long uncompressedSize = br.ReadInt64();

			// uncompress stuff

			int numChunks = br.ReadInt32();
			for (int i = 0; i < numChunks; i++)
				mChunks.Add(new Chunk(stream));

			return true;
		}

		public bool Save(string file)
		{
			using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
				return Save(fs);
		}

		public bool Save(Stream stream)
		{
			long dataLengthPosition, dataEndPosition;

			BinaryWriter bw = new BinaryWriter(stream);
			bw.Write(0x4C455354);
			bw.Write(mInformation);
			bw.Write(1);
			bw.Write(0);

			dataLengthPosition = stream.Position;

			// Length of data
			bw.Write(0L);

			bw.Write(mChunks.Count);
			foreach (Chunk chunk in mChunks)
				chunk.Save(stream);

			dataEndPosition = stream.Position;
			stream.Position = dataLengthPosition;
			bw.Write(dataEndPosition - dataLengthPosition + 4);
			stream.Position = dataEndPosition;

			return true;
		}

		public string Information
		{
			get
			{
				return mInformation;
			}
			set
			{
				mInformation = value;
			}
		}

		public List<Chunk> Chunks
		{
			get
			{
				return mChunks;
			}
			set
			{
				mChunks = value;
			}
		}

		public class Chunk
		{
			private Layer mBackLayer;
			private Layer mFrontLayer;

			public Chunk()
			{
				mBackLayer = new Layer();
				mFrontLayer = new Layer();
			}

			public Chunk(Stream stream)
			{
				mBackLayer = new Layer(stream);
				mFrontLayer = new Layer(stream);
			}

			public void Save(Stream stream)
			{
				mBackLayer.Save(stream);
				mFrontLayer.Save(stream);
			}

			public Layer BackLayer
			{
				get
				{
					return mBackLayer;
				}
				set
				{
					mBackLayer = value;
				}
			}

			public Layer FrontLayer
			{
				get
				{
					return mFrontLayer;
				}
				set
				{
					mFrontLayer = value;
				}
			}

			public class Layer
			{
				private sbyte[,] mAngles;
				private bool[,] mCollisionLRB;
				private bool[,] mCollisionT;
				private Image mArt;

				public Layer()
				{
					mAngles = new sbyte[8, 8];
					mCollisionLRB = new bool[128, 128];
					mCollisionT = new bool[128, 128];
					mArt = new Image();
				}

				public Layer(Stream stream)
					: this()
				{
					BinaryReader br = new BinaryReader(stream);

					// Read angles
					for (int y = 0; y < 8; y++)
						for (int x = 0; x < 8; x++)
							mAngles[x, y] = br.ReadSByte();

					// Read collision masks
					int bits = 0;
					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							if (x % 32 == 0)
								bits = br.ReadInt32();

							mCollisionLRB[x, y] = ((bits & (1 << (x % 32))) != 0);
						}
					}

					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							if (x % 32 == 0)
								bits = br.ReadInt32();

							mCollisionT[x, y] = ((bits & (1 << (x % 32))) != 0);
						}
					}

					// Read art
					mArt = new Image(stream);
				}

				public void Save(Stream stream)
				{
					BinaryWriter bw = new BinaryWriter(stream);

					// Write angles
					for (int y = 0; y < 8; y++)
						for (int x = 0; x < 8; x++)
							bw.Write(mAngles[x, y]);

					// Write collision masks
					int bits = 0;
					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							if (mCollisionLRB[x, y])
								bits |= 1 << (x % 32);
							if (x % 32 == 31) {
								bw.Write(bits);
								bits = 0;
							}
						}
					}

					bits = 0;
					for (int y = 0; y < 128; y++) {
						for (int x = 0; x < 128; x++) {
							if (mCollisionT[x, y])
								bits |= 1 << (x % 32);
							if (x % 32 == 31) {
								bw.Write(bits);
								bits = 0;
							}
						}
					}

					// Write art
					mArt.Save(stream);
				}

				public sbyte[,] Angles
				{
					get
					{
						return mAngles;
					}
					set
					{
						mAngles = value;
					}
				}

				public bool[,] CollisionLRB
				{
					get
					{
						return mCollisionLRB;
					}
					set
					{
						mCollisionLRB = value;
					}
				}

				public bool[,] CollisionT
				{
					get
					{
						return mCollisionT;
					}
					set
					{
						mCollisionT = value;
					}
				}

				public Image Art
				{
					get
					{
						return mArt;
					}
					set
					{
						mArt = value;
					}
				}

				public class Image
				{
					private byte mAnimationDuration;
					private byte mAnimationFrames;
					private byte[][] mFrameData;

					public Image()
					{
						mFrameData = new byte[0][];
					}

					public Image(Stream stream)
					{
						BinaryReader br = new BinaryReader(stream);
						mAnimationDuration = br.ReadByte();
						mAnimationFrames = br.ReadByte();
						mFrameData = new byte[mAnimationFrames][];
						for (int i = 0; i < mAnimationFrames; i++)
							mFrameData[i] = br.ReadBytes(512 * 512 * 4);
					}

					public void Save(Stream stream)
					{
						BinaryWriter bw = new BinaryWriter(stream);
						bw.Write(mAnimationDuration);
						bw.Write(mAnimationFrames);
						for (int i = 0; i < mAnimationFrames; i++)
							bw.Write(mFrameData[i]);
					}

					public byte AnimationDuration
					{
						get
						{
							return mAnimationDuration;
						}
						set
						{
							mAnimationDuration = value;
						}
					}

					public byte AnimationFrames
					{
						get
						{
							return mAnimationFrames;
						}
						set
						{
							mAnimationFrames = value;
						}
					}

					public byte[][] FrameData
					{
						get
						{
							return mFrameData;
						}
						set
						{
							mFrameData = value;
						}
					}
				}
			}
		}
	}
}
