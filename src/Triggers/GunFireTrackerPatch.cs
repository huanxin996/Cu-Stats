using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>本地玩家持枪开火：记 RangedAt 给击杀归因 + 上报开火事件到战斗分区按枪 itemId 累加；GunScript.body 是 `=> PlayerCamera.main.body` 私有属性即本地玩家专属。</summary>
    [HarmonyPatch(typeof(GunScript), nameof(GunScript.Fire))]
    internal static class GunFireTrackerPatch
    {
        private static void Postfix(GunScript __instance)
        {
            if (__instance == null) return;
            if (PlayerCamera.main == null || PlayerCamera.main.body == null) return;
            PlayerAttackTracker.RecordRanged();
            try
            {
                var item = __instance.GetComponent<Item>();
                if (item != null && !string.IsNullOrEmpty(item.id))
                {
                    StatsManager.ReportItemUsed(item, 100, forceCombat: true);
                    ModLog.Debug("[Fire] " + item.id);
                }
            }
            catch (System.Exception ex) { ModLog.Warn("[Fire] 失败：" + ex.Message); }
        }
    }
}
