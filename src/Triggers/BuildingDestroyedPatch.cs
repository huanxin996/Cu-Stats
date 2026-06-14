using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>BuildingEntity.health 跌破销毁阈值的那一帧上报建筑/植被破坏（发光草等不在 worldBlocks 里）。</summary>
    [HarmonyPatch(typeof(BuildingEntity), "Update")]
    internal static class BuildingDestroyedPatch
    {
        private static void Prefix(BuildingEntity __instance)
        {
            if (__instance == null) return;
            if (__instance.health >= 0.5f) return;
            string id = __instance.id;
            if (string.IsNullOrEmpty(id)) return;
            StatsManager.ReportBuildingDestroyed(id);
            ModLog.Debug("[Building] destroyed id=" + id);
        }
    }
}
