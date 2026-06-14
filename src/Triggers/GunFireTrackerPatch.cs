using HarmonyLib;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>本地玩家持枪开火时记录时间戳；GunScript.body 始终指向 PlayerCamera.main.body，无需额外判定。</summary>
    [HarmonyPatch(typeof(GunScript), nameof(GunScript.Fire))]
    internal static class GunFireTrackerPatch
    {
        private static void Postfix()
        {
            if (PlayerCamera.main == null || PlayerCamera.main.body == null) return;
            PlayerAttackTracker.RecordRanged();
        }
    }
}
