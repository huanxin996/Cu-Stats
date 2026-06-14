using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>Body.PickUpItem Postfix：仅在 item 之前不在玩家 transform 树（即"从世界/外部"进入），且 InstanceID 未计过时才上报。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.PickUpItem), new[] { typeof(Item), typeof(int), typeof(bool) })]
    internal static class ItemPickupPatch
    {
        private static readonly HashSet<int> _alreadyOwnedInSession = new HashSet<int>();

        private static void Prefix(Body __instance, Item item, out bool __state)
        {
            __state = item != null && IsUnderPlayer(item.transform, __instance);
        }

        private static void Postfix(Body __instance, Item item, int slot, bool __state)
        {
            try
            {
                if (item == null || string.IsNullOrEmpty(item.id)) return;
                if (PlayerCamera.main == null || __instance != PlayerCamera.main.body) return;
                if (__state) return;
                if (slot < 0 || slot >= __instance.slots.Length) return;
                if (__instance.GetItem(slot) != item) return;
                int iid = item.GetInstanceID();
                if (!_alreadyOwnedInSession.Add(iid)) return;
                StatsManager.ReportItemPicked(item.id, 1);
                ModLog.Debug("[Pickup] " + item.id + " iid=" + iid);
            }
            catch (System.Exception ex) { ModLog.Warn("[Pickup] 失败：" + ex.Message); }
        }

        private static bool IsUnderPlayer(Transform t, Body player)
        {
            if (t == null || player == null) return false;
            t = t.parent;
            while (t != null)
            {
                if (t == player.transform) return true;
                t = t.parent;
            }
            return false;
        }
    }
}
