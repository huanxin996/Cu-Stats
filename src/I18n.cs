using System.Collections.Generic;
using UnityEngine;

namespace CasualtiesUnknown.Stats
{
    /// <summary>UI 文案双语字典。</summary>
    internal static class I18n
    {
        private static readonly Dictionary<string, string> _zh = new Dictionary<string, string>
        {
            ["app.name"] = "统计",
            ["panel.title"] = "统计",
            ["panel.scope.global"] = "总计 (所有存档)",
            ["panel.scope.world"] = "本世界",
            ["panel.empty"] = "暂无数据",
            ["panel.close"] = "关闭",
            ["panel.hint"] = "U 切换  ·  ESC 关闭",
            ["tab.general"] = "综合",
            ["tab.blocks"] = "破坏方块",
            ["tab.picked"] = "获得物品",
            ["tab.used_food"] = "食物",
            ["tab.used_medical"] = "医疗",
            ["tab.used_combat"] = "战斗",
            ["tab.used_misc"] = "杂物",
            ["tab.settings"] = "设置",
            ["general.moved"] = "移动距离",
            ["general.descended"] = "下降层数",
            ["general.deepest"] = "最深层数",
            ["general.blocks_broken"] = "方块破坏总数",
            ["general.items_picked"] = "拾取物品总数",
            ["general.items_used"] = "使用物品总数",
            ["general.kills_mobs"] = "击杀生物数",
            ["general.encounters_survivors"] = "相遇求生者数",
            ["fmt.distance_m"] = "{0:F1} m",
            ["fmt.distance_km"] = "{0:F2} km",
            ["fmt.layers"] = "{0} 层",
            ["fmt.count"] = "{0}",
            ["settings.tab_title"] = "统计",
            ["settings.hotkey_open"] = "打开统计面板",
            ["settings.show_global"] = "默认显示总计",
            ["settings.log_verbose"] = "详细日志输出",
            ["settings.fmt_font_scale"] = "字号缩放：×{0:F2}",
            ["settings.check_update"] = "启动时检查更新",
            ["settings.on"] = "开",
            ["settings.off"] = "关",
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            ["app.name"] = "Stats",
            ["panel.title"] = "Statistics",
            ["panel.scope.global"] = "Total (all saves)",
            ["panel.scope.world"] = "This world",
            ["panel.empty"] = "No data",
            ["panel.close"] = "Close",
            ["panel.hint"] = "U toggle  ·  ESC close",
            ["tab.general"] = "General",
            ["tab.blocks"] = "Blocks broken",
            ["tab.picked"] = "Items picked up",
            ["tab.used_food"] = "Food",
            ["tab.used_medical"] = "Medical",
            ["tab.used_combat"] = "Combat",
            ["tab.used_misc"] = "Misc",
            ["tab.settings"] = "Settings",
            ["general.moved"] = "Distance walked",
            ["general.descended"] = "Layers descended",
            ["general.deepest"] = "Deepest layer",
            ["general.blocks_broken"] = "Blocks broken",
            ["general.items_picked"] = "Items picked up",
            ["general.items_used"] = "Items used",
            ["general.kills_mobs"] = "Mobs killed",
            ["general.encounters_survivors"] = "Survivors met",
            ["fmt.distance_m"] = "{0:F1} m",
            ["fmt.distance_km"] = "{0:F2} km",
            ["fmt.layers"] = "Layer {0}",
            ["fmt.count"] = "{0}",
            ["settings.tab_title"] = "Stats",
            ["settings.hotkey_open"] = "Open stats panel",
            ["settings.show_global"] = "Default to total",
            ["settings.log_verbose"] = "Verbose log output",
            ["settings.fmt_font_scale"] = "Font scale: ×{0:F2}",
            ["settings.check_update"] = "Check for updates on startup",
            ["settings.on"] = "ON",
            ["settings.off"] = "OFF",
        };

        internal static string T(string key)
        {
            var dict = UseChinese() ? _zh : _en;
            return dict.TryGetValue(key, out var v) ? v : key;
        }

        internal static string F(string key, params object[] args)
        {
            try { return string.Format(T(key), args); } catch { return T(key); }
        }

        internal static bool UseChinese()
        {
            string name = null;
            try { name = Locale.currentLangName; } catch { }
            if (string.IsNullOrEmpty(name)) try { name = PlayerPrefs.GetString("locale"); } catch { }
            if (string.IsNullOrEmpty(name)) return false;
            return name.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("WC", System.StringComparison.OrdinalIgnoreCase)
                || name.IndexOf("中文", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("chinese", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
