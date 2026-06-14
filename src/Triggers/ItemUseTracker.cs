namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>跨 patch 协作标记：ItemUsePatch 进入时 Active=true，Body.Eat 触发时 AteFlag=true，Postfix 读完即清。</summary>
    internal static class ItemUseTracker
    {
        private static int _depth;
        private static bool _ate;

        internal static void Enter()
        {
            if (++_depth == 1) _ate = false;
        }

        internal static bool Exit()
        {
            bool ate = _ate;
            if (--_depth <= 0) { _depth = 0; _ate = false; }
            return ate;
        }

        internal static void MarkAte()
        {
            if (_depth > 0) _ate = true;
        }

        /// <summary>按 condition 减量算"件等价 ×100"：≥0.95 视为完整一次（100），否则按比例四舍五入；不减或反增的特殊用法回落 100。</summary>
        internal static int ConditionDeltaX100(float before, float after)
        {
            float d = before - after;
            if (d <= 0f || d >= 0.95f) return 100;
            int v = UnityEngine.Mathf.RoundToInt(d * 100f);
            return v < 1 ? 1 : v;
        }
    }
}
