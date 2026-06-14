using HarmonyLib;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>WorldGeneration.DamageBlock 前后比对块 id：原本非空、调用后变 0 即视为破坏并按方块名累加。</summary>
    [HarmonyPatch(typeof(WorldGeneration), nameof(WorldGeneration.DamageBlock), new[] { typeof(Vector2Int), typeof(float), typeof(bool), typeof(bool), typeof(bool) })]
    internal static class BlockBreakPatch
    {
        private static void Prefix(WorldGeneration __instance, Vector2Int pos, out ushort __state)
        {
            try { __state = __instance.GetBlock(pos); }
            catch { __state = 0; }
        }

        private static void Postfix(WorldGeneration __instance, Vector2Int pos, ushort __state)
        {
            if (__state == 0) return;
            try
            {
                if (__instance.GetBlock(pos) != 0) return;
                StatsManager.ReportBlockBroken(__state);
                ModLog.Debug("[BlockBreak] id=" + __state);
            }
            catch (System.Exception ex) { ModLog.Warn("[BlockBreak] 失败：" + ex.Message); }
        }
    }
}
