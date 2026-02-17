using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;

namespace SafetySentinel.Services
{
    /// <summary>
    /// Generates all application sounds programmatically as in-memory WAV data.
    /// No audio files are needed — all sounds are synthesized from sine waves and frequency sweeps.
    /// </summary>
    public static class SoundGenerator
    {
        // P/Invoke for reliable in-memory WAV playback (works on any thread)
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(byte[] pszSound, IntPtr hmod, uint fdwSound);
        private const uint SND_MEMORY = 0x0004;
        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        // Pre-generated telex sounds — created once at startup, reused
        private static readonly byte[][] _telexTicks;     // 6 tick variants for natural variation
        private static readonly byte[] _telexBurst;       // "trrr" carriage/motor burst
        private static readonly byte[] _telexBell;        // soft bell (on section headers)
        private static long _lastTickTime = 0;
        private static int _tickVariant = 0;
        private static int _charsSinceLastBurst = 0;
        private static readonly object _tickLock = new();
        private static readonly Random _rng = new();

        static SoundGenerator()
        {
            // Pre-generate tick variants with different pitch/timbre
            _telexTicks = new byte[6][];
            _telexTicks[0] = GenerateTelexTick(baseFreq: 2800, durationMs: 12, volume: 0.07);
            _telexTicks[1] = GenerateTelexTick(baseFreq: 3200, durationMs: 10, volume: 0.06);
            _telexTicks[2] = GenerateTelexTick(baseFreq: 2600, durationMs: 14, volume: 0.08);
            _telexTicks[3] = GenerateTelexTick(baseFreq: 3000, durationMs: 11, volume: 0.065);
            _telexTicks[4] = GenerateTelexTick(baseFreq: 2900, durationMs: 13, volume: 0.07);
            _telexTicks[5] = GenerateTelexTick(baseFreq: 3400, durationMs: 9, volume: 0.055);

            _telexBurst = GenerateTelexBurst();
            _telexBell = GenerateTelexBell();
        }

        /// <summary>
        /// Play telex sounds during brief streaming.
        /// Varies the tick character and occasionally plays a "trrr" burst
        /// to simulate a real telex machine receiving a message.
        /// </summary>
        public static void PlayTelexTick(string? textDelta = null)
        {
            long now = Environment.TickCount64;
            lock (_tickLock)
            {
                if (now - _lastTickTime < 80) return; // max ~12/sec
                _lastTickTime = now;
            }

            // Check if this delta contains a newline — play burst for carriage return
            bool hasNewline = textDelta != null && (textDelta.Contains('\n') || textDelta.Contains('\r'));

            // Track chars for periodic bursts (simulate motor/paper advance)
            int charCount = textDelta?.Length ?? 1;
            int burstThreshold;
            lock (_tickLock)
            {
                _charsSinceLastBurst += charCount;
                burstThreshold = _charsSinceLastBurst;
            }

            // Check for section headers (═ character) — play bell
            bool hasHeader = textDelta != null && textDelta.Contains('═');

            try
            {
                if (hasHeader)
                {
                    PlaySound(_telexBell, IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);
                    return;
                }

                if (hasNewline || burstThreshold > 200)
                {
                    lock (_tickLock) { _charsSinceLastBurst = 0; }
                    PlaySound(_telexBurst, IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);
                }
                else
                {
                    // Pick a tick variant — cycle through with slight randomness
                    int variant;
                    lock (_tickLock)
                    {
                        _tickVariant = (_tickVariant + 1 + _rng.Next(0, 2)) % _telexTicks.Length;
                        variant = _tickVariant;
                    }
                    PlaySound(_telexTicks[variant], IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);
                }
            }
            catch { }
        }

        /// <summary>
        /// Generate a single telex key strike — solenoid hammer hitting paper through ribbon.
        /// Sharp metallic transient with mechanical resonance and rapid decay.
        /// </summary>
        private static byte[] GenerateTelexTick(double baseFreq, int durationMs, double volume)
        {
            int sampleRate = 44100;
            int samples = sampleRate * durationMs / 1000;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int dataSize = samples * 2;
            WriteWavHeader(bw, sampleRate, 1, 16, dataSize);

            var rng = new Random((int)(baseFreq * 100));
            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                double progress = (double)i / samples;

                // Steep exponential decay — solenoid snap
                double envelope = Math.Exp(-progress * 18.0);

                // Initial impact transient (first 1ms = broadband noise)
                double impact = 0;
                if (progress < 0.15)
                    impact = (rng.NextDouble() * 2 - 1) * 1.5 * Math.Exp(-progress * 40.0);

                // Mechanical resonance — metallic tone from the print head
                double resonance = Math.Sin(2 * Math.PI * baseFreq * t) * 0.6;

                // Secondary lower resonance — body of the machine
                double body = Math.Sin(2 * Math.PI * baseFreq * 0.4 * t) * 0.25;

                double sample = (impact + resonance + body) * envelope * volume;
                short s16 = (short)Math.Clamp(sample * 32767, short.MinValue, short.MaxValue);
                bw.Write(s16);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Generate telex "trrr" burst — the mechanical ratcheting/motor sound
        /// heard during paper advance, carriage return, or motor engagement.
        /// Rapid series of micro-clicks with low-frequency rumble.
        /// </summary>
        private static byte[] GenerateTelexBurst()
        {
            int sampleRate = 44100;
            int durationMs = 60; // Short burst
            int samples = sampleRate * durationMs / 1000;
            double volume = 0.05;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int dataSize = samples * 2;
            WriteWavHeader(bw, sampleRate, 1, 16, dataSize);

            double clickRate = 180; // ~180 clicks per second = motor ratchet speed
            var rng = new Random(42);

            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                double progress = (double)i / samples;

                // Ratcheting: periodic sharp impulses
                double phase = (t * clickRate) % 1.0;
                double ratchet = phase < 0.08 ? (rng.NextDouble() * 2 - 1) * 1.2 : 0;

                // Low-frequency motor rumble
                double rumble = Math.Sin(2 * Math.PI * 120 * t) * 0.3
                              + Math.Sin(2 * Math.PI * 85 * t) * 0.2;

                // Bell-shaped envelope (rises then falls)
                double envelope = Math.Sin(Math.PI * progress);

                double sample = (ratchet + rumble) * envelope * volume;
                short s16 = (short)Math.Clamp(sample * 32767, short.MinValue, short.MaxValue);
                bw.Write(s16);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Generate a very soft telex bell — tiny "ding" for section headers.
        /// Classic typewriter margin bell, but very quiet.
        /// </summary>
        private static byte[] GenerateTelexBell()
        {
            int sampleRate = 44100;
            int durationMs = 80;
            int samples = sampleRate * durationMs / 1000;
            double volume = 0.04;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int dataSize = samples * 2;
            WriteWavHeader(bw, sampleRate, 1, 16, dataSize);

            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                double progress = (double)i / samples;

                // Bell harmonics
                double bell = Math.Sin(2 * Math.PI * 2200 * t) * 0.7
                            + Math.Sin(2 * Math.PI * 4400 * t) * 0.2
                            + Math.Sin(2 * Math.PI * 6600 * t) * 0.1;

                // Fast attack, medium decay
                double envelope = Math.Exp(-progress * 6.0) * Math.Min(1.0, progress * 30);

                double sample = bell * envelope * volume;
                short s16 = (short)Math.Clamp(sample * 32767, short.MinValue, short.MaxValue);
                bw.Write(s16);
            }

            return ms.ToArray();
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
