using HarmonyLib;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>Body.Eat 调用时若处于 ItemUsePatch 的 use 上下文，标记本次使用为吃食物。</summary>
    [HarmonyPatch(typeof(Body), nameof(Body.Eat))]
    internal static class BodyEatPatch
    {
        private static void Prefix() => ItemUseTracker.MarkAte();
    }
}
