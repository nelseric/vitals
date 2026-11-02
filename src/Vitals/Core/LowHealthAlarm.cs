namespace Vitals.Core
{
    public sealed class LowHealthSettings
    {
        public float Threshold = 0.4f;
        public float RepeatSeconds = 10f;
    }

    // Decides when to sound the low-health alarm: on losing health below the threshold, then every RepeatSeconds while still low.
    public sealed class LowHealthAlarm
    {
        // Health must climb this far above the threshold before a new drop alarms again, so regen hovering at the line stays quiet.
        public const float RearmMargin = 0.05f;

        private enum State { Clear, Ignored, Alarmed }

        private readonly LowHealthSettings _settings;
        private State _state;
        private float? _lastHealth;
        private float _lastAlarm;

        public LowHealthAlarm(LowHealthSettings settings) => _settings = settings;

        // Call when there is no living local player, so respawning or logging in doesn't count as a drop.
        public void Reset()
        {
            _state = State.Clear;
            _lastHealth = null;
        }

        // True when the alarm should play this frame. Going low only because max health rose (eating) stays quiet.
        public bool Step(float time, float health, float maxHealth)
        {
            bool lostHealth = _lastHealth.HasValue && health < _lastHealth.Value;
            _lastHealth = health;
            float fraction = maxHealth > 0f ? health / maxHealth : 1f;

            if (fraction >= _settings.Threshold + RearmMargin || (_state == State.Clear && fraction >= _settings.Threshold))
            {
                _state = State.Clear;
                return false;
            }
            if (_state != State.Alarmed)
            {
                if (fraction >= _settings.Threshold) return false;
                if (!lostHealth)
                {
                    _state = State.Ignored;
                    return false;
                }
                _state = State.Alarmed;
                _lastAlarm = time;
                return true;
            }
            if (fraction >= _settings.Threshold || _settings.RepeatSeconds <= 0f || time - _lastAlarm < _settings.RepeatSeconds) return false;
            _lastAlarm = time;
            return true;
        }
    }
}
