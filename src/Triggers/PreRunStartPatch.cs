using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>读档完成后刷新存档命名空间并武装移动跟踪。</summary>
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class PreRunStartPatch
    {
        private static void Postfix()
        {
            try
            {
                StatsManager.RefreshSaveKey();
                MovementTracker.Arm();
            }
            catch (System.Exception ex) { ModLog.Warn("[PreRunStart] 失败：" + ex.Message); }
        }
    }
}
