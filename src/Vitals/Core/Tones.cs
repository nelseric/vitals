using System;

namespace Vitals.Core
{
    // A note in a generated sound: frequency in Hz (0 = silence) and length in seconds.
    public readonly struct Note
    {
        public readonly float Hz;
        public readonly float Seconds;

        public Note(float hz, float seconds)
        {
            Hz = hz;
            Seconds = seconds;
        }
    }

    // Builds mono sample data for the alert sounds, so the mod ships no audio files.
    public static class Tones
    {
        public const int SampleRate = 44100;
        private const float FadeSeconds = 0.008f;

        // Two sharp beeps: hard to miss over combat noise.
        public static readonly Note[] LowHealth = { new Note(988f, 0.12f), new Note(0f, 0.07f), new Note(988f, 0.12f) };

        // A soft falling pair of notes.
        public static readonly Note[] FoodExpired = { new Note(659f, 0.16f), new Note(440f, 0.28f) };

        public static float[] Render(Note[] notes, bool decay)
        {
            int total = 0;
            foreach (var note in notes) total += (int)(note.Seconds * SampleRate);
            var samples = new float[total];
            int start = 0;
            foreach (var note in notes)
            {
                int n = (int)(note.Seconds * SampleRate);
                if (note.Hz > 0f)
                {
                    int fade = Math.Min((int)(FadeSeconds * SampleRate), n / 2);
                    for (int i = 0; i < n; i++)
                    {
                        float t = (float)i / SampleRate;
                        float envelope = Math.Min(1f, Math.Min(i, n - 1 - i) / (float)fade);
                        if (decay) envelope *= (float)Math.Exp(-4f * i / n);
                        samples[start + i] = 0.8f * envelope * (float)Math.Sin(2 * Math.PI * note.Hz * t);
                    }
                }
                start += n;
            }
            return samples;
        }
    }
}
