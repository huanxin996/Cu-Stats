using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>Body.UseItem 入口：Prefix 缓存 condition_before，Postfix 按耐久减量 ×100 上报；触发了 body.Eat 强制 food 分区。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.UseItem))]
    internal static class ItemUsePatch
    {
        private static void Prefix(Body __instance, Item item, out (bool tracked, float before) __state)
        {
            __state = (false, 0f);
            if (!ShouldTrack(__instance, item)) return;
            __state = (true, item.condition);
            ItemUseTracker.Enter();
        }

        private static void Postfix(Body __instance, Item item, (bool tracked, float before) __state)
        {
            if (!__state.tracked) return;
            bool ate = ItemUseTracker.Exit();
            try
            {
                float after = item != null ? item.condition : 0f;
                int amount = ItemUseTracker.ConditionDeltaX100(__state.before, after);
                StatsManager.ReportItemUsed(item, amount, forceFood: ate);
                ModLog.Debug("[Use] " + item.id + " delta=" + amount + (ate ? " (ate)" : ""));
            }
            catch (System.Exception ex) { ModLog.Warn("[Use] 失败：" + ex.Message); }
        }

        internal static bool ShouldTrack(Body body, Item item)
        {
            if (item == null || item.Stats == null || !item.Stats.usable) return false;
            if (PlayerCamera.main == null || body != PlayerCamera.main.body) return false;
            return true;
        }
    }

    /// <summary>Body.UseItemInHand 左键使用手持物品入口：游戏直接调 useAction 不走 UseItem，本 patch 自管 Enter/Exit 与 condition 缓存。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.UseItemInHand))]
    internal static class ItemUseInHandPatch
    {
        private static void Prefix(Body __instance, out (Item item, bool tracked, float before) __state)
        {
            __state = (null, false, 0f);
            try
            {
                int slot = __instance.handSlot;
                if (!__instance.HoldingItem(slot)) return;
                var item = __instance.GetItem(slot);
                if (item == null || item.Stats == null) return;
                if (!item.Stats.usable || !item.Stats.usableWithLMB) return;
                if (!__instance.conscious) return;
                if (!ItemUsePatch.ShouldTrack(__instance, item)) return;
                __state = (item, true, item.condition);
                ItemUseTracker.Enter();
            }
            catch (System.Exception ex) { ModLog.Warn("[UseInHand] Prefix 失败：" + ex.Message); }
        }

        private static void Postfix((Item item, bool tracked, float before) __state)
        {
            if (!__state.tracked || __state.item == null) return;
            bool ate = ItemUseTracker.Exit();
            try
            {
                float after = __state.item.condition;
                int amount = ItemUseTracker.ConditionDeltaX100(__state.before, after);
                StatsManager.ReportItemUsed(__state.item, amount, forceFood: ate);
                ModLog.Debug("[UseInHand] " + __state.item.id + " delta=" + amount + (ate ? " (ate)" : ""));
            }
            catch (System.Exception ex) { ModLog.Warn("[UseInHand] 失败：" + ex.Message); }
        }
    }
}
