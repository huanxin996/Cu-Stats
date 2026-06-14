using UnityEngine;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>记录本地玩家最近一次近战 / 开枪的时间与有效距离，供 BuildingDestroyedPatch 判定击杀归因。</summary>
    internal static class PlayerAttackTracker
    {
        private const float MeleeFallbackDist = 5f;
        private const float RangedRange = 100f;

        internal static float MeleeAtUnscaled = -1000f;
        internal static float MeleeDistance = MeleeFallbackDist;
        internal static float RangedAtUnscaled = -1000f;

        internal static void RecordMelee(float distance)
        {
            MeleeAtUnscaled = Time.unscaledTime;
            MeleeDistance = distance > 0f ? distance + 1f : MeleeFallbackDist;
        }

        internal static void RecordRanged()
        {
            RangedAtUnscaled = Time.unscaledTime;
        }

        /// <summary>给定 building 位置，按时间窗 + 距离判定是否本地玩家击杀。</summary>
        internal static bool IsPlayerKill(Vector3 buildingPos)
        {
            const float TimeWindow = 1.5f;
            var cam = PlayerCamera.main;
            if (cam == null || cam.body == null) return false;
            float now = Time.unscaledTime;
            float dist = (buildingPos - cam.body.transform.position).magnitude;
            if (now - MeleeAtUnscaled <= TimeWindow && dist <= MeleeDistance) return true;
            if (now - RangedAtUnscaled <= TimeWindow && dist <= RangedRange) return true;
            return false;
        }
    }
}
