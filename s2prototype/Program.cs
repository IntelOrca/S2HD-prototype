using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Sonic
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			for (int i = -128; i < 127; i++) {
				int d0 = i + 32;
				d0 &= 192;
				Debug.WriteLine("{0} -> {1}", i, d0);
			}

			// MakeSonicSS();
			// MakeTransparent("data\\graphics\\starpost.png", Color.FromArgb(255, 0, 255));
			// return;

			// LevelConverter conv = new LevelConverter();
			// conv.Convert();
			// return;

			using (SonicGame game = new SonicGame())
				game.Run();
		}

		static void MakeTransparent(string filename, Color transparent)
		{
			Bitmap img = (Bitmap)Bitmap.FromFile(filename);
			img.MakeTransparent(transparent);
			img.Save(filename + ".transparent.png");
		}

		static void MakeSonicSS()
		{
			List<Bitmap> sprites = new List<Bitmap>();

			Color ttc = Color.FromArgb(0, 52, 76);
			for (int i = 1; i <= 213; i++) {
				string path = String.Format(@"C:\Users\Ted\Desktop\Sonic\sonic_s2_{0:000}.BMP", i);
				Bitmap original = (Bitmap)Bitmap.FromFile(path);
				original.MakeTransparent(ttc);

				Bitmap bmp = new Bitmap(64 * 4, 70 * 4);
				System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
				g.DrawImage(original, new Rectangle(0, 0, 64 * 4, 70 * 4), new Rectangle(0, 0, 64, 70), GraphicsUnit.Pixel);
				g.Dispose();

				original.Dispose();

				sprites.Add(bmp);
			}

			FileStream fs = new FileStream(@"C:\Users\Ted\Desktop\sonic.dat", FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);
			bw.Write(sprites.Count);
			for (int i = 0; i < sprites.Count; i++) {
				int startPosition = (int)fs.Position;
				bw.Write(0);
				sprites[i].Save(fs, ImageFormat.Png);
				int endPosition = (int)fs.Position;

				fs.Position = startPosition;
				bw.Write(endPosition - startPosition - 4);
				fs.Position = endPosition;
			}

			fs.Close();
		}
	}
}
