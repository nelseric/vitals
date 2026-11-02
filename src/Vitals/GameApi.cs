using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;

namespace Vitals
{
    // Valheim members used by the mod, resolved at runtime so the build needs no game assemblies.
    internal sealed class GameApi
    {
        public readonly MethodInfo UpdateFood;
        private readonly FieldInfo _localPlayer;
        private readonly FieldInfo _foods;
        private readonly MethodInfo _isDead;
        private readonly MethodInfo _getHealth;
        private readonly MethodInfo _getMaxHealth;

        private GameApi(Type player)
        {
            UpdateFood = AccessTools.Method(player, "UpdateFood");
            _localPlayer = AccessTools.Field(player, "m_localPlayer");
            _foods = AccessTools.Field(player, "m_foods");
            _isDead = AccessTools.Method(player, "IsDead");
            _getHealth = AccessTools.Method(player, "GetHealth");
            _getMaxHealth = AccessTools.Method(player, "GetMaxHealth");
        }

        // Returns null and reports what is missing if this Valheim version doesn't match.
        public static GameApi Resolve(Action<string> logError)
        {
            var player = AccessTools.TypeByName("Player");
            if (player == null)
            {
                logError("Valheim's Player type not found.");
                return null;
            }
            var api = new GameApi(player);
            foreach (var (member, name) in new (MemberInfo, string)[]
                     {
                         (api.UpdateFood, "Player.UpdateFood"), (api._localPlayer, "Player.m_localPlayer"), (api._foods, "Player.m_foods"),
                         (api._isDead, "Character.IsDead"), (api._getHealth, "Character.GetHealth"), (api._getMaxHealth, "Character.GetMaxHealth"),
                     })
            {
                if (member == null)
                {
                    logError($"{name} not found; this Valheim version is not supported.");
                    return null;
                }
            }
            return api;
        }

        public object LocalPlayer() => _localPlayer.GetValue(null);

        public bool IsDead(object player) => (bool)_isDead.Invoke(player, null);

        public float Health(object player) => (float)_getHealth.Invoke(player, null);

        public float MaxHealth(object player) => (float)_getMaxHealth.Invoke(player, null);

        public int FoodCount(object player) => ((ICollection)_foods.GetValue(player)).Count;
    }
}
