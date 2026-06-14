using System.Diagnostics;
using CasualtiesUnknown.Stats.Core;

namespace CasualtiesUnknown.Stats.Util
{
    /// <summary>统一日志门面：Info 受 StatsConfig.LogVerbose 控制；Warn/Error 总是输出；Debug 在 Release 编译期剥离。</summary>
    internal static class ModLog
    {
        internal static void Info(string msg)
        {
            if (StatsConfig.LogVerbose != null && !StatsConfig.LogVerbose.Value) return;
            Plugin.Log?.LogInfo(msg);
        }

        internal static void Warn(string msg) => Plugin.Log?.LogWarning(msg);

        internal static void Error(string msg) => Plugin.Log?.LogError(msg);

        [Conditional("DEBUG")]
        internal static void Debug(string msg) => Plugin.Log?.LogInfo("[DBG] " + msg);
    }
}
