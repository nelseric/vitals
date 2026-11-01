namespace Vitals.Core
{
    public sealed class LowHealthSettings
    {
        public float Threshold = 0.4f;
        public float RepeatSeconds = 10f;
    }

    // Decides when to sound the low-health alarm: on dropping below the threshold, then every RepeatSeconds while still low.
    public sealed class LowHealthAlarm
    {
        // Health must climb this far above the threshold before a new drop alarms again, so regen hovering at the line stays quiet.
        public const float RearmMargin = 0.05f;

        private readonly LowHealthSettings _settings;
        private bool _low;
        private float _lastAlarm;

        public LowHealthAlarm(LowHealthSettings settings) => _settings = settings;

        // Call when there is no living local player, so respawning or logging in doesn't count as a drop.
        public void Reset() => _low = false;

        // True when the alarm should play this frame; healthFraction is health / max health.
        public bool Step(float time, float healthFraction)
        {
            if (!_low)
            {
                if (healthFraction >= _settings.Threshold) return false;
                _low = true;
                _lastAlarm = time;
                return true;
            }
            if (healthFraction >= _settings.Threshold + RearmMargin)
            {
                _low = false;
                return false;
            }
            if (healthFraction >= _settings.Threshold || _settings.RepeatSeconds <= 0f || time - _lastAlarm < _settings.RepeatSeconds) return false;
            _lastAlarm = time;
            return true;
        }
    }
}
