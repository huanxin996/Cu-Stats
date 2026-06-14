using System.Collections.Generic;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Core
{
    /// <summary>统计运行时门面：触发器把数据投给这里，UI 通过 Store 读。</summary>
    internal static class StatsManager
    {
        private static StatsStore _store;
        private static float _flushAccum;
        private const float FlushIntervalSeconds = 30f;

        internal static StatsStore Store => _store;

        internal static void Init()
        {
            _store = new StatsStore();
            RefreshSaveKey();
        }

        internal static void RefreshSaveKey()
        {
            if (_store == null) return;
            string key = SaveKeyResolver.CurrentSaveKey();
            _store.SwitchSaveKey(key);
            ModLog.Debug("[Manager] saveKey=" + key);
        }

        internal static void Add(string section, string key, long amount = 1)
        {
            _store?.Add(section, key, amount);
        }

        internal static void SetMax(string section, string key, long value)
        {
            _store?.SetMax(section, key, value);
        }

        /// <summary>记录方块被破坏：综合 +1，blocks 分区按 b:&lt;id&gt; +1（前缀区分真方块）。</summary>
        internal static void ReportBlockBroken(ushort blockId)
        {
            _store?.Add(StatKeys.General, StatKeys.BlocksBroken, 1);
            _store?.Add(StatKeys.Blocks, "b:" + blockId, 1);
        }

        /// <summary>记录建筑/植被被破坏（发光草等不在 worldBlocks 中）：综合 +1，blocks 分区按 e:&lt;id&gt; +1。</summary>
        internal static void ReportBuildingDestroyed(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return;
            _store?.Add(StatKeys.General, StatKeys.BlocksBroken, 1);
            _store?.Add(StatKeys.Blocks, "e:" + buildingId, 1);
        }

        /// <summary>记录拾取物品：综合 +1，picked 分区按 itemId 累加 amount。</summary>
        internal static void ReportItemPicked(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;
            _store?.Add(StatKeys.General, StatKeys.ItemsPicked, amount);
            _store?.Add(StatKeys.Picked, itemId, amount);
        }

        /// <summary>记录使用物品：耐久分区按 amountX100 累加 + count 分区按 +1 累加；section 按 forceFood/forceCombat/Categorizer 决定。</summary>
        internal static void ReportItemUsed(Item item, int amountX100, bool forceFood = false, bool forceCombat = false)
        {
            if (item == null || amountX100 <= 0) return;
            string id = item.id;
            if (string.IsNullOrEmpty(id)) return;
            string section = forceFood ? StatKeys.UsedFood
                : forceCombat ? StatKeys.UsedCombat
                : ItemCategorizer.Categorize(item);
            string countSection = section + "_count";
            _store?.Add(StatKeys.General, StatKeys.ItemsUsed, amountX100);
            _store?.Add(section, id, amountX100);
            _store?.Add(countSection, id, 1);
        }

        /// <summary>累加移动距离（厘米），按需调用，避免每帧入盘。</summary>
        internal static void AddMovedCm(long cm)
        {
            if (cm <= 0) return;
            _store?.Add(StatKeys.General, StatKeys.MovedCm, cm);
        }

        /// <summary>切层时累加层数 + 更新最深层数。</summary>
        internal static void ReportLayerDescended(int totalTraveled)
        {
            _store?.Add(StatKeys.General, StatKeys.DescendedLayers, 1);
            _store?.SetMax(StatKeys.General, StatKeys.DeepestLayer, totalTraveled);
        }

        internal static void ReportKill() => _store?.Add(StatKeys.General, StatKeys.KillsMobs, 1);

        internal static void ReportEncounter() => _store?.Add(StatKeys.General, StatKeys.EncountersSurvivors, 1);

        internal static void Tick(float dt)
        {
            _flushAccum += dt;
            if (_flushAccum < FlushIntervalSeconds) return;
            _flushAccum = 0f;
            _store?.Flush();
        }

        internal static void Flush() => _store?.Flush();
    }
}
