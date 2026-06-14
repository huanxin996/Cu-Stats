using System.Collections.Generic;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;

namespace CasualtiesUnknown.Stats
{
    /// <summary>UI 文案双语字典，按配置 + 游戏 Locale 自动选择中英。</summary>
    internal static class I18n
    {
        private static readonly string[] ChineseKeywords =
        {
            "中文", "汉化", "简中", "简体", "繁中", "繁体", "繁體", "chinese"
        };

        private static readonly Dictionary<string, string> _zh = new Dictionary<string, string>
        {
            ["app.name"] = "统计",
            ["panel.title"] = "统计",
            ["panel.scope.global"] = "总计 (所有存档)",
            ["panel.scope.world"] = "本世界",
            ["panel.unit.durability"] = "耐久件数",
            ["panel.unit.count"] = "使用次数",
            ["panel.empty"] = "暂无数据",
            ["panel.close"] = "关闭",
            ["tab.general"] = "综合",
            ["tab.blocks"] = "破坏方块",
            ["tab.picked"] = "获得物品",
            ["tab.used_food"] = "食物",
            ["tab.used_medical"] = "医疗",
            ["tab.used_combat"] = "战斗",
            ["tab.used_misc"] = "杂物",
            ["tab.settings"] = "设置",
            ["tab.about"] = "关于",
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
            ["fmt.usage"] = "{0:F2} 件",
            ["status.bottom"] = "由 CuStats 提供 · 当前存档：{0}",
            ["settings.tab_title"] = "统计",
            ["settings.hotkey_open"] = "打开统计面板",
            ["settings.show_global"] = "默认显示总计",
            ["settings.log_verbose"] = "详细日志输出",
            ["settings.fmt_font_scale"] = "字号缩放：×{0:F2}",
            ["settings.check_update"] = "启动时检查更新",
            ["settings.toggle_hotkey"] = "面板开关快捷键",
            ["settings.press_a_key"] = "请按下新按键…",
            ["settings.on"] = "开",
            ["settings.off"] = "关",
            ["lbl.language"] = "界面语言：",
            ["opt.language_auto"] = "跟随游戏",
            ["opt.language_zh"] = "中文",
            ["opt.language_en"] = "English",
            ["update.available"] = "CuStats 有新版本：{0}（点击打开 release 页）",
            ["about.title"] = "CuStats",
            ["about.desc"] = "为 Casualties: Unknown 提供方块破坏 / 物品获得 / 战斗 / 医疗 / 食物 / 杂物 / 移动 / 击杀等统计。",
            ["about.version"] = "版本：{0}",
            ["about.sec_links"] = "链接",
            ["about.link_repo"] = "GitHub 仓库",
            ["about.link_release"] = "最新版本",
            ["about.sec_credits"] = "开发人员",
            ["about.sec_deps"] = "依赖与致谢",
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            ["app.name"] = "Stats",
            ["panel.title"] = "Statistics",
            ["panel.scope.global"] = "Total (all saves)",
            ["panel.scope.world"] = "This world",
            ["panel.unit.durability"] = "Durability",
            ["panel.unit.count"] = "Use count",
            ["panel.empty"] = "No data",
            ["panel.close"] = "Close",
            ["tab.general"] = "General",
            ["tab.blocks"] = "Blocks broken",
            ["tab.picked"] = "Items picked up",
            ["tab.used_food"] = "Food",
            ["tab.used_medical"] = "Medical",
            ["tab.used_combat"] = "Combat",
            ["tab.used_misc"] = "Misc",
            ["tab.settings"] = "Settings",
            ["tab.about"] = "About",
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
            ["fmt.usage"] = "{0:F2}",
            ["status.bottom"] = "by CuStats · saveKey: {0}",
            ["settings.tab_title"] = "Stats",
            ["settings.hotkey_open"] = "Open stats panel",
            ["settings.show_global"] = "Default to total",
            ["settings.log_verbose"] = "Verbose log output",
            ["settings.fmt_font_scale"] = "Font scale: ×{0:F2}",
            ["settings.check_update"] = "Check for updates on startup",
            ["settings.toggle_hotkey"] = "Toggle hotkey",
            ["settings.press_a_key"] = "Press a new key...",
            ["settings.on"] = "ON",
            ["settings.off"] = "OFF",
            ["lbl.language"] = "UI language:",
            ["opt.language_auto"] = "Follow game",
            ["opt.language_zh"] = "Chinese",
            ["opt.language_en"] = "English",
            ["update.available"] = "CuStats update available: {0} (click to open release page)",
            ["about.title"] = "CuStats",
            ["about.desc"] = "Block / item / combat / medical / food / movement / kill stats for Casualties: Unknown.",
            ["about.version"] = "Version: {0}",
            ["about.sec_links"] = "Links",
            ["about.link_repo"] = "GitHub repository",
            ["about.link_release"] = "Latest release",
            ["about.sec_credits"] = "Developers",
            ["about.sec_deps"] = "Dependencies & thanks",
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
            switch (NormalizeLanguageMode(StatsConfig.PreferredLanguage?.Value))
            {
                case "zh":
                    return true;
                case "en":
                    return false;
                default:
                    return IsChineseLocaleName(ReadCurrentLanguageName());
            }
        }

        private static string ReadCurrentLanguageName()
        {
            string name = null;
            try { name = Locale.currentLangName; } catch { }
            if (string.IsNullOrEmpty(name))
            {
                try { name = PlayerPrefs.GetString("locale"); } catch { }
            }
            return name;
        }

        internal static string NormalizeLanguageMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return "auto";
            mode = mode.Trim().ToLowerInvariant();
            return mode == "zh" || mode == "en" ? mode : "auto";
        }

        private static bool IsChineseLocaleName(string name)
        {
            string normalized = StripRichText(name).Trim();
            if (string.IsNullOrEmpty(normalized)) return false;

            if (normalized.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("WC", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            for (int i = 0; i < ChineseKeywords.Length; i++)
            {
                if (normalized.IndexOf(ChineseKeywords[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static string StripRichText(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('<') < 0) return value ?? string.Empty;
            var sb = new System.Text.StringBuilder(value.Length);
            bool inTag = false;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch == '<') { inTag = true; continue; }
                if (ch == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
