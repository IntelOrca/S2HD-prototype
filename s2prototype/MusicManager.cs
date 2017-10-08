using System;
using Microsoft.Xna.Framework.Audio;

namespace IntelOrca.Sonic
{
	class MusicManager : IDisposable
	{
		private float mVolume = 0.7f;
		private SoundEffectInstance mCurrentlyPlayingMusic;
		private SoundEffectInstance mCurrentlyPlayingJingle;

		public void StopAllMusic()
		{
			if (mCurrentlyPlayingMusic != null) {
				mCurrentlyPlayingMusic.Stop();
				mCurrentlyPlayingMusic.Dispose();
				mCurrentlyPlayingMusic = null;
			}

			if (mCurrentlyPlayingJingle != null) {
				mCurrentlyPlayingJingle.Stop();
				mCurrentlyPlayingJingle.Dispose();
				mCurrentlyPlayingJingle = null;
			}
		}

		public void Update()
		{
			if (mCurrentlyPlayingJingle != null) {
				if (mCurrentlyPlayingJingle.State == SoundState.Stopped) {
					mCurrentlyPlayingJingle.Stop();
					mCurrentlyPlayingJingle.Dispose();
					mCurrentlyPlayingJingle = null;

					if (mCurrentlyPlayingMusic != null)
						mCurrentlyPlayingMusic.Volume = mVolume;
				}
			}
		}

		public void PlayMusic(SoundEffect soundEffect)
		{
			if (mCurrentlyPlayingJingle != null) {
				mCurrentlyPlayingJingle.Stop();
				mCurrentlyPlayingJingle.Dispose();
				mCurrentlyPlayingJingle = null;
			}

			if (mCurrentlyPlayingMusic != null) {
				mCurrentlyPlayingMusic.Stop();
				mCurrentlyPlayingMusic.Dispose();
			}

			mCurrentlyPlayingMusic = soundEffect.CreateInstance();
			mCurrentlyPlayingMusic.Volume = mVolume;
			mCurrentlyPlayingMusic.IsLooped = true;
			mCurrentlyPlayingMusic.Play();
		}

		public void PlayJingle(SoundEffect soundEffect)
		{
			if (mCurrentlyPlayingJingle != null) {
				mCurrentlyPlayingJingle.Stop();
				mCurrentlyPlayingJingle.Dispose();
			}

			if (mCurrentlyPlayingMusic != null)
				mCurrentlyPlayingMusic.Volume = 0.0f;

			mCurrentlyPlayingJingle = soundEffect.CreateInstance();
			mCurrentlyPlayingJingle.Volume = mVolume;
			mCurrentlyPlayingJingle.Play();
		}

		public void Dispose()
		{
			StopAllMusic();
		}
	}
}
