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
    }
}
