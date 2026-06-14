using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>Body.UseItem 入口：Prefix 进入 use 上下文，Postfix 按是否触发 body.Eat 决定分到 food 还是默认分类。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.UseItem))]
    internal static class ItemUsePatch
    {
        private static void Prefix(Body __instance, Item item, out bool __state)
        {
            __state = ShouldTrack(__instance, item);
            if (__state) ItemUseTracker.Enter();
        }

        private static void Postfix(Body __instance, Item item, bool __state)
        {
            if (!__state) return;
            bool ate = ItemUseTracker.Exit();
            try
            {
                StatsManager.ReportItemUsed(item, forceFood: ate);
                ModLog.Debug("[Use] " + item.id + (ate ? " (ate)" : ""));
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

    /// <summary>Body.UseItemInHand 左键使用手持物品入口：游戏内直接调 useAction 不走 UseItem，本 patch 自管 Enter/Exit。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.UseItemInHand))]
    internal static class ItemUseInHandPatch
    {
        private static void Prefix(Body __instance, out (Item item, bool tracked) __state)
        {
            __state = (null, false);
            try
            {
                int slot = __instance.handSlot;
                if (!__instance.HoldingItem(slot)) return;
                var item = __instance.GetItem(slot);
                if (item == null || item.Stats == null) return;
                if (!item.Stats.usable || !item.Stats.usableWithLMB) return;
                if (!__instance.conscious) return;
                if (!ItemUsePatch.ShouldTrack(__instance, item)) return;
                __state = (item, true);
                ItemUseTracker.Enter();
            }
            catch (System.Exception ex) { ModLog.Warn("[UseInHand] Prefix 失败：" + ex.Message); }
        }

        private static void Postfix((Item item, bool tracked) __state)
        {
            if (!__state.tracked || __state.item == null) return;
            bool ate = ItemUseTracker.Exit();
            try
            {
                StatsManager.ReportItemUsed(__state.item, forceFood: ate);
                ModLog.Debug("[UseInHand] " + __state.item.id + (ate ? " (ate)" : ""));
            }
            catch (System.Exception ex) { ModLog.Warn("[UseInHand] 失败：" + ex.Message); }
        }
    }
}
