using HarmonyLib;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>本地玩家执行 Body.Attack 时记录时间戳与攻击距离，供击杀归因使用。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.Attack))]
    internal static class BodyAttackTrackerPatch
    {
        private static void Postfix(Body __instance, AttackInfo atk)
        {
            if (PlayerCamera.main == null || __instance != PlayerCamera.main.body) return;
            PlayerAttackTracker.RecordMelee(atk != null ? atk.distance : 0f);
        }
    }
}
