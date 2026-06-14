using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>TraderScript.MeetPlayer 由游戏内 !startedConvo 守卫天然每 trader 一次，Postfix 直接计数。</summary>
    [HarmonyPatch(typeof(TraderScript), nameof(TraderScript.MeetPlayer))]
    internal static class EncounterTrackerPatch
    {
        private static void Postfix()
        {
            try
            {
                StatsManager.ReportEncounter();
                ModLog.Debug("[Encounter] trader meet");
            }
            catch (System.Exception ex) { ModLog.Warn("[Encounter] 失败：" + ex.Message); }
        }
    }
}
