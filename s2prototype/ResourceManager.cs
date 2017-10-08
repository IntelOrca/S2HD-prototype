using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace IntelOrca.Sonic
{
	static class ResourceManager
	{
		public static GraphicsDevice GraphicsDevice;

		public static Texture2D PlainTexture;

		public static Texture2D MarkerTexture;
		public static Texture2D FontsTexture;
		public static Texture2D[] SonicTextures;
		public static Texture2D SpindashDustTexture;
		public static Texture2D ShieldTexture;
		public static Texture2D InvincibilityTexture;
		public static Texture2D RingTexture;
		public static Texture2D MonitorTexture;
		public static Texture2D LogBridgeTexture;
		public static Texture2D EHZPlatformTexture;
		public static Texture2D MasherTexture;
		public static Texture2D CoconutsTexture;
		public static Texture2D BuzzerTexture;
		public static Texture2D SpikesTexture;
		public static Texture2D SpringTexture;
		public static Texture2D ExplosionTexture;
		public static Texture2D AnimalsTexture;
		public static Texture2D SignpostTexture;
		public static Texture2D StarpostTexture;

		public static List<Texture2D> ChunkTexturesBack;
		public static List<Texture2D> ChunkTexturesFront;

		public static SoundEffect BadnikExplosionSound;
		public static SoundEffect BrakeSound;
		public static SoundEffect JumpSound;
		public static SoundEffect RingSound;
		public static SoundEffect SpinSound;
		public static SoundEffect SpindashChargeSound;
		public static SoundEffect SpindashReleaseSound;
		public static SoundEffect ShieldSound;
		public static SoundEffect RingScatterSound;
		public static SoundEffect SpikesSound;
		public static SoundEffect SpikesMoveSound;
		public static SoundEffect BounceSound;
		public static SoundEffect HurtSound;
		public static SoundEffect SignpostSound;
		public static SoundEffect StarpostSound;

		public static SoundEffect EHZMusic;
		public static SoundEffect EHZSpeedMusic;
		public static SoundEffect InvincibilityMusic;
		public static SoundEffect LifeMusic;

		public static Font NormalFont;

		public static void LoadResources()
		{
			MarkerTexture = LoadTexture("data\\graphics\\marker.png");
			FontsTexture = LoadTexture("data\\graphics\\fonts.png");
			SonicTextures = LoadTextures("data\\graphics\\sonic.dat");
			SpindashDustTexture = LoadTexture("data\\graphics\\spindash_dust.png");
			ShieldTexture = LoadTexture("data\\graphics\\shield.png");
			InvincibilityTexture = LoadTexture("data\\graphics\\invincibility.png");
			RingTexture = LoadTexture("data\\graphics\\ring.png");
			MonitorTexture = LoadTexture("data\\graphics\\monitor.png");
			LogBridgeTexture = LoadTexture("data\\graphics\\logbridge.png");
			EHZPlatformTexture = LoadTexture("data\\graphics\\ehzplatform.png");
			MasherTexture = LoadTexture("data\\graphics\\masher.png");
			CoconutsTexture = LoadTexture("data\\graphics\\coconuts.png");
			BuzzerTexture = LoadTexture("data\\graphics\\buzzer.png");
			SpikesTexture = LoadTexture("data\\graphics\\spikes.png");
			SpringTexture = LoadTexture("data\\graphics\\spring.png");
			ExplosionTexture = LoadTexture("data\\graphics\\explosion.png");
			AnimalsTexture = LoadTexture("data\\graphics\\animals.png");
			SignpostTexture = LoadTexture("data\\graphics\\signpost.png");
			StarpostTexture = LoadTexture("data\\graphics\\starpost.png");

			BadnikExplosionSound = LoadSound("data\\sounds\\badnik_explosion.wav");
			BrakeSound = LoadSound("data\\sounds\\brake.wav");
			JumpSound = LoadSound("data\\sounds\\jump.wav");
			RingSound = LoadSound("data\\sounds\\ring.wav");
			SpinSound = LoadSound("data\\sounds\\spin.wav");
			SpindashChargeSound = LoadSound("data\\sounds\\spindash_charge.wav");
			SpindashReleaseSound = LoadSound("data\\sounds\\spindash_release.wav");
			ShieldSound = LoadSound("data\\sounds\\shield.wav");
			RingScatterSound = LoadSound("data\\sounds\\ring_scatter.wav");
			SpikesSound = LoadSound("data\\sounds\\spikes.wav");
			SpikesMoveSound = LoadSound("data\\sounds\\spikes_move.wav");
			BounceSound = LoadSound("data\\sounds\\bounce.wav");
			HurtSound = LoadSound("data\\sounds\\hurt.wav");
			SignpostSound = LoadSound("data\\sounds\\signpost.wav");
			StarpostSound = LoadSound("data\\sounds\\starpost.wav");

			EHZMusic = LoadSound("data\\music\\ehz.wav");
			EHZSpeedMusic = LoadSound("data\\music\\ehz_speed.wav");
			InvincibilityMusic = LoadSound("data\\music\\invincibility.wav");
			LifeMusic = LoadSound("data\\music\\life.wav");

			NormalFont = new Font(FontsTexture);
		}

		public static void FreeResources()
		{

		}

		public static Texture2D CreateTexture(int width, int height, Color colour)
		{
			Texture2D texture = new Texture2D(GraphicsDevice, width, height);
			Color[] bits = new Color[width * height];
			for (int i = 0; i < bits.Length; i++)
				bits[i] = colour;
			texture.SetData(bits);
			return texture;
		}

		private static Texture2D LoadTexture(string path)
		{
			using (Stream s = new FileStream(path, FileMode.Open))
				return Texture2D.FromStream(GraphicsDevice, s);
		}

		private static Texture2D[] LoadTextures(string path)
		{
			Texture2D[] textures;
			using (Stream s = new FileStream(path, FileMode.Open)) {
				BinaryReader br = new BinaryReader(s);
				textures = new Texture2D[br.ReadInt32()];
				for (int i = 0; i < textures.Length; i++) {
					int length = br.ReadInt32();
					using (MemoryStream ms = new MemoryStream(br.ReadBytes(length)))
						textures[i] = Texture2D.FromStream(GraphicsDevice, ms);
				}
			}
			return textures;
		}

		private static SoundEffect LoadSound(string path)
		{
			using (Stream s = new FileStream(path, FileMode.Open))
				return SoundEffect.FromStream(s);
		}
	}
}
