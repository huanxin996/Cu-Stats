using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>Body.HandlePeriodicChecks Pre/Postfix 检测 brainHealth 跨 0 跳变；本地玩家自己死亡不计。</summary>
    [HarmonyPatch(typeof(Body), "HandlePeriodicChecks")]
    internal static class KillTrackerPatch
    {
        private static void Prefix(Body __instance, out bool __state)
        {
            __state = __instance != null && __instance.brainHealth > 0f;
        }

        private static void Postfix(Body __instance, bool __state)
        {
            if (!__state || __instance == null) return;
            if (__instance.brainHealth > 0f) return;
            try
            {
                if (PlayerCamera.main != null && __instance == PlayerCamera.main.body) return;
                StatsManager.ReportKill();
                ModLog.Debug("[Kill] body=" + __instance.name);
            }
            catch (System.Exception ex) { ModLog.Warn("[Kill] 失败：" + ex.Message); }
        }
    }
}
