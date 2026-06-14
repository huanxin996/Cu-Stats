using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>WorldGeneration.IncreaseDepthByLayer 是切下一层的唯一入口，每次调用记一层并刷新最深层数。</summary>
    [HarmonyPatch(typeof(WorldGeneration), nameof(WorldGeneration.IncreaseDepthByLayer))]
    internal static class LayerDescentPatch
    {
        private static void Postfix(WorldGeneration __instance)
        {
            try
            {
                int depthMeters = (int)__instance.PlayerLayerDepthMeters() + __instance.totalTraveled;
                StatsManager.ReportLayerDescended(depthMeters);
                ModLog.Debug("[Layer] depth_m=" + depthMeters);
            }
            catch (System.Exception ex) { ModLog.Warn("[Layer] 失败：" + ex.Message); }
        }
    }
}
