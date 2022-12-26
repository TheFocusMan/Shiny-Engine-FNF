using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace WpfGame
{
    /// <summary>
    /// Based on FlxSound.hx
    /// </summary>
    public class FlxSound : IDisposable
    {
        internal WaveChannel32 Music { get; set; }
        internal WasapiOut Player { get; set; }

        public void Play()
        {
            Player?.Play();
        }

        public void Pause()
        {
            Player?.Pause();
        }

        public void Stop()
        {
            Player.Stop();
        }

        public float Volume
        {
            get => _originalvol;
            set
            {
                _originalvol = value;
                Music.Volume = value * Sound.Volume;
            }
        }

        private float _originalvol;

        internal void RefreshVol()
        {
            Music.Volume = _originalvol * Sound.Volume;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Music?.Dispose();
                Player?.Dispose();
                _disposed = true;
            }
        }


        public TimeSpan Position
        {
            get
            {
                ValuteableObject();
                return Music.CurrentTime;
            }
            set
            {
                ValuteableObject();
                Music.CurrentTime = value;
            }
        }

        public TimeSpan Length
        {
            get
            {
                ValuteableObject();
                return Music.TotalTime;
            }
        }

        public PlaybackState PlaybackState
        {
            get
            {
                return Player.PlaybackState;
            }
        }

        private void ValuteableObject()
        {
            if (_disposed) throw new ObjectDisposedException("flxsound");
        }

        bool _disposed;

        public bool IsDisposed => _disposed;

        public event EventHandler<StoppedEventArgs> PlaybackStopped
        {
            add => Player.PlaybackStopped += value;
            remove => Player.PlaybackStopped -= value;
        }

    }

    public static class Sound
    {
        internal static List<FlxSound> StrongCacheSound = new List<FlxSound>();
        private static float _vol = 1f;
        public static float Volume
        {
            get => _vol;
            set
            {
                _vol = value;
                foreach (var sound in StrongCacheSound)
                {
                    sound.RefreshVol();
                }
            }
        }

        public static FlxSound[] GetSoundStrongCache()
        {
            return StrongCacheSound.ToArray();
        }

        public static int PlayersCount => StrongCacheSound.Count;

        private static WaveChannel32 CreateInputStream(WaveStream stream)
        {
            WaveChannel32 inputStream;
            WaveStream readerStream = stream;

            /*// Provide PCM conversion if needed
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }

            // Provide conversion to 16 bits if needed
            if (readerStream.WaveFormat.BitsPerSample != 16)
            {
                var format = new WaveFormat(readerStream.WaveFormat.SampleRate,
                16, readerStream.WaveFormat.Channels);
                readerStream = new WaveFormatConversionStream(format, readerStream);
            }*/

            inputStream = new WaveChannel32(readerStream) { PadWithZeroes = false, Volume = Volume };

            return inputStream;
        }

        public static FlxSound Play(string play, Action ended = null)
        {
            var sound = LoadOnly(play);
            sound.Play();
            sound.PlaybackStopped += (sender, e) =>
            {
                if (sound.Player.PlaybackState == PlaybackState.Stopped)
                {
                    StrongCacheSound.Remove(sound);
                    sound.Dispose();
                    ended?.Invoke();
                }
            };
            return sound;
        }

        public static FlxSound LoadOnly(string file)
        {
            var wasapi = new WasapiOut();
            using var tsk = Extentions.CreateStreamFromUri(file);
            tsk.Wait();
            VorbisWaveReader reader = new VorbisWaveReader(tsk.Result, true);
            var red = CreateInputStream(reader);
            var ret = new FlxSound { Music = red, Player = wasapi };
            wasapi.Init(red);
            StrongCacheSound.Add(ret);
            return ret;
        }

        public static void StopAll()
        {
            foreach (var g in StrongCacheSound)
            {
                g.Dispose(); // קוד פשוט
            }
            StrongCacheSound.Clear();
        }
    }
}
