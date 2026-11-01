using Vitals.Core;
using Xunit;

namespace Vitals.Tests
{
    public class LowHealthAlarmTests
    {
        private static LowHealthAlarm Alarm(float repeat = 10f) => new LowHealthAlarm(new LowHealthSettings { Threshold = 0.4f, RepeatSeconds = repeat });

        [Fact]
        public void QuietAboveThreshold()
        {
            var alarm = Alarm();
            Assert.False(alarm.Step(0f, 1f));
            Assert.False(alarm.Step(1f, 0.4f));
        }

        [Fact]
        public void SoundsOnceOnDropping()
        {
            var alarm = Alarm();
            Assert.False(alarm.Step(0f, 0.8f));
            Assert.True(alarm.Step(1f, 0.39f));
            Assert.False(alarm.Step(2f, 0.2f));
        }

        [Fact]
        public void RepeatsWhileLow()
        {
            var alarm = Alarm();
            Assert.True(alarm.Step(0f, 0.3f));
            Assert.False(alarm.Step(9.9f, 0.3f));
            Assert.True(alarm.Step(10f, 0.3f));
            Assert.False(alarm.Step(15f, 0.3f));
            Assert.True(alarm.Step(20f, 0.3f));
        }

        [Fact]
        public void ZeroRepeat_SoundsOnlyOncePerDrop()
        {
            var alarm = Alarm(repeat: 0f);
            Assert.True(alarm.Step(0f, 0.3f));
            Assert.False(alarm.Step(100f, 0.3f));
        }

        [Fact]
        public void HoveringAtTheLine_DoesNotRealarm()
        {
            var alarm = Alarm();
            Assert.True(alarm.Step(0f, 0.39f));
            Assert.False(alarm.Step(1f, 0.42f));
            Assert.False(alarm.Step(2f, 0.39f));
        }

        [Fact]
        public void HoveringJustAboveTheLine_DoesNotRepeat()
        {
            var alarm = Alarm();
            Assert.True(alarm.Step(0f, 0.39f));
            Assert.False(alarm.Step(30f, 0.42f));
        }

        [Fact]
        public void RecoveringClear_RearmsTheAlarm()
        {
            var alarm = Alarm();
            Assert.True(alarm.Step(0f, 0.39f));
            Assert.False(alarm.Step(1f, 0.5f));
            Assert.True(alarm.Step(2f, 0.39f));
        }

        [Fact]
        public void Reset_TreatsTheNextLowReadingAsADrop()
        {
            var alarm = Alarm();
            Assert.True(alarm.Step(0f, 0.3f));
            alarm.Reset();
            Assert.True(alarm.Step(1f, 0.3f));
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
