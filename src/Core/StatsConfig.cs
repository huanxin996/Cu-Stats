using BepInEx.Configuration;

namespace CasualtiesUnknown.Stats.Core
{
    /// <summary>BepInEx 配置：日志详细输出、UI 字号缩放、更新检测开关。</summary>
    internal static class StatsConfig
    {
        internal static ConfigEntry<bool> LogVerbose { get; private set; }
        internal static ConfigEntry<float> UiScale { get; private set; }
        internal static ConfigEntry<bool> CheckUpdate { get; private set; }

        internal static void Bind(ConfigFile cfg)
        {
            LogVerbose = cfg.Bind("Log", "Verbose", false, "Enable Info/Debug log output (Warn/Error always shown).");
            UiScale = cfg.Bind("UI", "FontScale", 1.0f, new ConfigDescription("UI font scale, 0.6..2.0", new AcceptableValueRange<float>(0.6f, 2.0f)));
            CheckUpdate = cfg.Bind("Update", "Check", true, "Check GitHub for new releases on startup.");
        }
    }
}
