using UnityEngine;

namespace CasualtiesUnknown.Stats.Core
{
    /// <summary>把 Item 归到统计分区：战斗 / 医疗 / 食物 / 杂物。</summary>
    internal static class ItemCategorizer
    {
        internal static string Categorize(Item item)
        {
            if (item == null) return StatKeys.UsedMisc;
            if (IsCombat(item)) return StatKeys.UsedCombat;
            if (IsMedical(item)) return StatKeys.UsedMedical;
            if (IsFood(item)) return StatKeys.UsedFood;
            return StatKeys.UsedMisc;
        }

        private static bool IsCombat(Item item)
        {
            if (item.GetComponent<GunScript>() != null) return true;
            if (item.GetComponent<AmmoScript>() != null) return true;
            if (item.GetComponent<MineScript>() != null) return true;
            if (item.GetComponent<GunmineScript>() != null) return true;
            var stats = item.Stats;
            if (stats != null && (stats.HasTag("gun") || stats.HasTag("bullet"))) return true;
            return false;
        }

        private static bool IsMedical(Item item)
        {
            var stats = item.Stats;
            if (stats == null) return false;
            if (stats.usableOnLimb) return true;
            if (stats.category == "medical" || stats.category == "drug") return true;
            if (stats.HasTag("medicine") || stats.HasTag("dressing") || stats.HasTag("antiseptic")) return true;
            return false;
        }

        private static bool IsFood(Item item)
        {
            if (item.GetComponent<NonDescriptCan>() != null) return true;
            var stats = item.Stats;
            if (stats != null && stats.HasTag("fruit")) return true;
            return false;
        }
    }
}
