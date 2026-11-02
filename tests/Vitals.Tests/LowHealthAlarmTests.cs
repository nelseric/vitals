using Vitals.Core;
using Xunit;

namespace Vitals.Tests
{
    public class LowHealthAlarmTests
    {
        private static LowHealthAlarm Alarm(float repeat = 10f) => new LowHealthAlarm(new LowHealthSettings { Threshold = 0.4f, RepeatSeconds = repeat });

        // Health readings out of a max of 100.
        private static bool Step(LowHealthAlarm alarm, float time, float health) => alarm.Step(time, health, 100f);

        [Fact]
        public void QuietAboveThreshold()
        {
            var alarm = Alarm();
            Assert.False(Step(alarm, 0f, 100f));
            Assert.False(Step(alarm, 1f, 40f));
        }

        [Fact]
        public void SoundsOnceOnDropping()
        {
            var alarm = Alarm();
            Assert.False(Step(alarm, 0f, 80f));
            Assert.True(Step(alarm, 1f, 39f));
            Assert.False(Step(alarm, 2f, 20f));
        }

        [Fact]
        public void RepeatsWhileLow()
        {
            var alarm = Alarm();
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 30f));
            Assert.False(Step(alarm, 9.9f, 30f));
            Assert.True(Step(alarm, 10f, 30f));
            Assert.False(Step(alarm, 15f, 30f));
            Assert.True(Step(alarm, 20f, 30f));
        }

        [Fact]
        public void ZeroRepeat_SoundsOnlyOncePerDrop()
        {
            var alarm = Alarm(repeat: 0f);
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 30f));
            Assert.False(Step(alarm, 100f, 30f));
        }

        [Fact]
        public void HoveringAtTheLine_DoesNotRealarm()
        {
            var alarm = Alarm();
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 39f));
            Assert.False(Step(alarm, 1f, 42f));
            Assert.False(Step(alarm, 2f, 39f));
        }

        [Fact]
        public void HoveringJustAboveTheLine_DoesNotRepeat()
        {
            var alarm = Alarm();
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 39f));
            Assert.False(Step(alarm, 30f, 42f));
        }

        [Fact]
        public void RecoveringClear_RearmsTheAlarm()
        {
            var alarm = Alarm();
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 39f));
            Assert.False(Step(alarm, 1f, 50f));
            Assert.True(Step(alarm, 2f, 39f));
        }

        [Fact]
        public void EatingRaisesMaxHealth_StaysQuiet()
        {
            var alarm = Alarm();
            Assert.False(alarm.Step(0f, 25f, 25f));
            Assert.False(alarm.Step(1f, 25f, 75f));
            Assert.False(alarm.Step(20f, 26f, 75f));
        }

        [Fact]
        public void DamageAfterEating_StillAlarms()
        {
            var alarm = Alarm();
            alarm.Step(0f, 25f, 25f);
            alarm.Step(1f, 25f, 75f);
            Assert.True(alarm.Step(2f, 20f, 75f));
        }

        [Fact]
        public void AfterReset_FirstLowReadingIsQuiet_DamageAlarms()
        {
            var alarm = Alarm();
            Step(alarm, 0f, 80f);
            Assert.True(Step(alarm, 0f, 30f));
            alarm.Reset();
            Assert.False(Step(alarm, 1f, 30f));
            Assert.True(Step(alarm, 2f, 29f));
        }
    }

    public class TonesTests
    {
        [Fact]
        public void RendersTheNotesLength()
        {
            var samples = Tones.Render(new[] { new Note(440f, 0.1f), new Note(0f, 0.05f) }, decay: false);
            Assert.Equal((int)(0.1f * Tones.SampleRate) + (int)(0.05f * Tones.SampleRate), samples.Length);
        }

        [Fact]
        public void StaysInRangeAndStartsAndEndsSilent()
        {
            foreach (var notes in new[] { Tones.LowHealth, Tones.FoodExpired })
            {
                var samples = Tones.Render(notes, decay: true);
                Assert.All(samples, s => Assert.InRange(s, -1f, 1f));
                Assert.Equal(0f, samples[0]);
                Assert.Equal(0f, samples[samples.Length - 1], 3);
            }
        }
    }
}
