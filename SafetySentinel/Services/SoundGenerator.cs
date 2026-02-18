using System;
using System.IO;
using System.Media;

namespace SafetySentinel.Services
{
    /// <summary>
    /// Plays application sounds: tab clicks, swooshes (synthesized),
    /// and the authentic Teletext loop from Teletext.wav.
    /// </summary>
    public static class SoundGenerator
    {
        private static SoundPlayer? _teletextPlayer;
        private static bool _teletextPlaying = false;
        private static readonly object _teletextLock = new();

        static SoundGenerator()
        {
            // Pre-load the Teletext WAV so looping starts instantly
            try
            {
                var wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teletext.wav");
                if (File.Exists(wavPath))
                {
                    _teletextPlayer = new SoundPlayer(wavPath);
                    _teletextPlayer.Load();
                }
            }
            catch { /* Audio is non-critical */ }
        }

        /// <summary>
        /// Start looping the Teletext sound. Called when streaming text begins.
        /// Safe to call multiple times — only starts once until stopped.
        /// </summary>
        public static void PlayTelexTick(string? textDelta = null)
        {
            if (string.IsNullOrEmpty(textDelta)) return;

            lock (_teletextLock)
            {
                if (_teletextPlaying || _teletextPlayer == null) return;
                _teletextPlaying = true;
            }

            try
            {
                _teletextPlayer.PlayLooping();
            }
            catch { /* Audio is non-critical */ }
        }

        /// <summary>
        /// Stop the Teletext loop (e.g., when streaming completes).
        /// </summary>
        public static void StopTypewriter()
        {
            lock (_teletextLock)
            {
                _teletextPlaying = false;
            }

            try
            {
                _teletextPlayer?.Stop();
            }
            catch { /* Audio is non-critical */ }
        }

        /// <summary>
        /// Play a soft "toop" click sound for tab switching.
        /// Short sine wave burst at ~800Hz, 50ms, low volume.
        /// </summary>
        public static void PlayTabClick()
        {
            var wav = GenerateSineWave(frequency: 800, durationMs: 50, volume: 0.15, fadeMs: 15);
            PlayWav(wav);
        }

        /// <summary>
        /// Play swoosh sound for chat panel opening.
        /// Rising frequency sweep from 300Hz to 1200Hz over 200ms.
        /// </summary>
        public static void PlaySwooshOpen()
        {
            var wav = GenerateFrequencySweep(startFreq: 300, endFreq: 1200, durationMs: 200, volume: 0.2);
            PlayWav(wav);
        }

        /// <summary>
        /// Play reverse swoosh for chat panel closing.
        /// Falling frequency sweep from 1200Hz to 300Hz over 200ms.
        /// </summary>
        public static void PlaySwooshClose()
        {
            var wav = GenerateFrequencySweep(startFreq: 1200, endFreq: 300, durationMs: 200, volume: 0.2);
            PlayWav(wav);
        }

        private static byte[] GenerateSineWave(double frequency, int durationMs, double volume, int fadeMs)
        {
            int sampleRate = 44100;
            int samples = sampleRate * durationMs / 1000;
            int fadeSamples = sampleRate * fadeMs / 1000;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int dataSize = samples * 2; // 16-bit mono
            WriteWavHeader(bw, sampleRate, 1, 16, dataSize);

            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                double sample = Math.Sin(2 * Math.PI * frequency * t) * volume;

                // Fade in/out envelope
                if (i < fadeSamples)
                    sample *= (double)i / fadeSamples;
                else if (i > samples - fadeSamples)
                    sample *= (double)(samples - i) / fadeSamples;

                short s16 = (short)(sample * 32767);
                bw.Write(s16);
            }

            return ms.ToArray();
        }

        private static byte[] GenerateFrequencySweep(double startFreq, double endFreq, int durationMs, double volume)
        {
            int sampleRate = 44100;
            int samples = sampleRate * durationMs / 1000;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int dataSize = samples * 2;
            WriteWavHeader(bw, sampleRate, 1, 16, dataSize);

            double phase = 0;
            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / samples;
                double freq = startFreq + (endFreq - startFreq) * t;
                phase += 2 * Math.PI * freq / sampleRate;
                double sample = Math.Sin(phase) * volume;

                // Smooth bell-shaped envelope
                double envelope = Math.Sin(Math.PI * t);
                sample *= envelope;

                short s16 = (short)(sample * 32767);
                bw.Write(s16);
            }

            return ms.ToArray();
        }

        private static void WriteWavHeader(BinaryWriter bw, int sampleRate, short channels, short bitsPerSample, int dataSize)
        {
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            // RIFF header
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + dataSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt subchunk
            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16);              // Subchunk1Size (PCM)
            bw.Write((short)1);        // AudioFormat (PCM)
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(bitsPerSample);

            // data subchunk
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(dataSize);
        }

        private static void PlayWav(byte[] wavData)
        {
            try
            {
                var ms = new MemoryStream(wavData);
                var player = new SoundPlayer(ms);
                player.Play(); // Async, non-blocking
            }
            catch
            {
                // Silently ignore audio errors — sounds are non-critical
            }
        }
    }
}
