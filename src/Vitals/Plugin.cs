using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Vitals.Core;

namespace Vitals
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("valheim")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;

        private ConfigEntry<bool> _lowHealthEnabled;
        private ConfigEntry<float> _lowHealthThreshold;
        private ConfigEntry<float> _lowHealthRepeat;
        private ConfigEntry<float> _lowHealthVolume;
        private ConfigEntry<bool> _foodEnabled;
        private ConfigEntry<float> _foodVolume;

        private readonly LowHealthSettings _lowHealthSettings = new LowHealthSettings();
        private LowHealthAlarm _lowHealth;
        private GameApi _game;
        private Harmony _harmony;
        private AudioSource _audio;
        private AudioClip _lowHealthClip;
        private AudioClip _foodClip;

        private void Awake()
        {
            Instance = this;
            _lowHealthEnabled = Config.Bind("LowHealth", "Enabled", true, "Beep when health drops below the threshold.");
            _lowHealthThreshold = Config.Bind("LowHealth", "Threshold", 0.4f, new ConfigDescription("Fraction of max health below which the alarm sounds.", new AcceptableValueRange<float>(0.05f, 0.95f)));
            _lowHealthRepeat = Config.Bind("LowHealth", "RepeatSeconds", 10f, "While health stays low, beep again this often (0 = only once per drop).");
            _lowHealthVolume = Config.Bind("LowHealth", "Volume", 0.6f, new ConfigDescription("Alarm volume, scaled by the game's master volume.", new AcceptableValueRange<float>(0f, 1f)));
            _foodEnabled = Config.Bind("FoodExpired", "Enabled", true, "Chime when a food buff runs out.");
            _foodVolume = Config.Bind("FoodExpired", "Volume", 0.5f, new ConfigDescription("Chime volume, scaled by the game's master volume.", new AcceptableValueRange<float>(0f, 1f)));

            ApplySettings();
            Config.SettingChanged += (_, __) => ApplySettings();

            _game = GameApi.Resolve(msg => Logger.LogError(msg));
            if (_game == null) return;
            _lowHealth = new LowHealthAlarm(_lowHealthSettings);
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.spatialBlend = 0f;
            _audio.playOnAwake = false;
            _lowHealthClip = MakeClip("vitals-low-health", Tones.Render(Tones.LowHealth, decay: false));
            _foodClip = MakeClip("vitals-food-expired", Tones.Render(Tones.FoodExpired, decay: true));
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.Patch(_game.UpdateFood,
                prefix: new HarmonyMethod(typeof(PlayerUpdateFoodPatch), nameof(PlayerUpdateFoodPatch.Prefix)),
                postfix: new HarmonyMethod(typeof(PlayerUpdateFoodPatch), nameof(PlayerUpdateFoodPatch.Postfix)));
        }

        private void ApplySettings()
        {
            _lowHealthSettings.Threshold = _lowHealthThreshold.Value;
            _lowHealthSettings.RepeatSeconds = _lowHealthRepeat.Value;
        }

        private static AudioClip MakeClip(string name, float[] samples)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, Tones.SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            if (_game == null) return;
            var player = _game.LocalPlayer();
            if (player == null || _game.IsDead(player))
            {
                _lowHealth.Reset();
                return;
            }
            if (_lowHealth.Step(Time.time, _game.HealthFraction(player)) && _lowHealthEnabled.Value)
                _audio.PlayOneShot(_lowHealthClip, _lowHealthVolume.Value);
        }

        internal int FoodCount(object player) => _game.LocalPlayer() == player ? _game.FoodCount(player) : -1;

        // Called after Player.UpdateFood, which removes a food only when it runs out.
        internal void AfterUpdateFood(object player, int countBefore)
        {
            if (countBefore < 0 || !_foodEnabled.Value || _game.FoodCount(player) >= countBefore) return;
            _audio.PlayOneShot(_foodClip, _foodVolume.Value);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Instance = null;
        }
    }

    // Applied to Player.UpdateFood(float dt, bool forceUpdate) in Plugin.Awake.
    internal static class PlayerUpdateFoodPatch
    {
        internal static void Prefix(object __instance, out int __state) => __state = Plugin.Instance?.FoodCount(__instance) ?? -1;

        internal static void Postfix(object __instance, int __state) => Plugin.Instance?.AfterUpdateFood(__instance, __state);
    }
}
