using System.Collections.Generic;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.UI
{
    /// <summary>仿 saveManager 风格统计面板：GUI.matrix 自适应缩放 + ModalWindow 可拖动 + 类目 tab + 列表区 + 设置页；嵌入 saveManager 分页时不画窗口框。</summary>
    internal sealed class StatsPanel
    {
        private const int WindowId = 0x07A75A75;
        private const float WindowWidth = 1200f;
        private const float WindowHeight = 800f;
        private const float TitleBarHeight = 64f;
        private const float CloseBtnSize = 52f;
        private const string TabSettings = "settings";
        private const string TabAbout = "about";

        private static readonly string[] Tabs =
        {
            StatKeys.General, StatKeys.Blocks, StatKeys.Picked,
            StatKeys.UsedFood, StatKeys.UsedMedical, StatKeys.UsedCombat, StatKeys.UsedMisc,
            TabSettings, TabAbout,
        };

        private bool _open;
        private bool _showGlobal = true;
        private string _activeTab = StatKeys.General;
        private Vector2 _scroll;
        private Rect _rect = new Rect(-1f, -1f, WindowWidth, WindowHeight);
        private float _drawScale = 1f;

        internal bool Open => _open;
        internal void Toggle() { _open = !_open; if (_open) RecenterIfNeeded(); }
        internal void Close() { _open = false; }

        internal void Draw()
        {
            if (!_open) return;
            BlackWhiteSkin.Push();
            var prev = GUI.matrix;
            try
            {
                _drawScale = ComputeScale();
                ApplyFontScale();
                _rect.width = WindowWidth;
                _rect.height = WindowHeight;
                float maxX = Mathf.Max(0f, Screen.width / _drawScale - WindowWidth);
                float maxY = Mathf.Max(0f, Screen.height / _drawScale - WindowHeight);
                _rect.x = Mathf.Clamp(_rect.x, 0f, maxX);
                _rect.y = Mathf.Clamp(_rect.y, 0f, maxY);
                GUI.matrix = Matrix4x4.Scale(new Vector3(_drawScale, _drawScale, 1f));
                _rect = GUI.ModalWindow(WindowId, _rect, DrawWindowContent, "");
            }
            catch (System.Exception ex) { ModLog.Warn("StatsPanel.Draw 失败：" + ex.Message); _open = false; }
            finally { GUI.matrix = prev; BlackWhiteSkin.Pop(); }
        }

        /// <summary>嵌入 saveManager 设置分页时调用：宿主提供窗口外框，本面板顺序画 topbar/tab/列表，不缩放不拖动；Push/Pop 确保 BlackWhiteSkin styles 在嵌入路径下也已初始化。</summary>
        internal void DrawEmbedded()
        {
            BlackWhiteSkin.Push();
            try
            {
                ApplyFontScale();
                DrawTopBar();
                DrawTabs();
                DrawBody();
            }
            finally { BlackWhiteSkin.Pop(); }
        }

        internal bool IsCursorOver()
        {
            if (!_open) return false;
            var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            var screenRect = new Rect(_rect.x * _drawScale, _rect.y * _drawScale, _rect.width * _drawScale, _rect.height * _drawScale);
            return screenRect.Contains(mouse);
        }

        private static float ComputeScale()
        {
            float fit = Mathf.Min(Screen.width / WindowWidth, Screen.height / WindowHeight) * 0.92f;
            return Mathf.Clamp(fit, 0.3f, 1f);
        }

        private void RecenterIfNeeded()
        {
            if (_rect.x >= 0f && _rect.y >= 0f) return;
            float scale = ComputeScale();
            _rect.x = (Screen.width / scale - WindowWidth) * 0.5f;
            _rect.y = (Screen.height / scale - WindowHeight) * 0.5f;
        }

        private void DrawWindowContent(int id)
        {
            BlackWhiteSkin.DrawBorder(new Rect(0f, 0f, WindowWidth, WindowHeight), 6f);

            GUI.Label(new Rect(28f, 14f, WindowWidth - CloseBtnSize - 56f, 40f),
                I18n.T("panel.title"), BlackWhiteSkin.HeaderStyle);

            var closeRect = new Rect(WindowWidth - CloseBtnSize - 12f, 8f, CloseBtnSize, CloseBtnSize);
            if (GUI.Button(closeRect, GUIContent.none)) _open = false;
            BlackWhiteSkin.DrawBorder(closeRect, 4f);
            BlackWhiteSkin.DrawCloseX(new Rect(closeRect.x + 13f, closeRect.y + 13f,
                closeRect.width - 26f, closeRect.height - 26f), 6f);

            BlackWhiteSkin.DrawHLine(new Rect(0f, TitleBarHeight, WindowWidth, 4f));

            var bodyRect = new Rect(24f, TitleBarHeight + 16f,
                WindowWidth - 48f, WindowHeight - TitleBarHeight - 40f);
            GUILayout.BeginArea(bodyRect);
            DrawTopBar();
            DrawTabs();
            DrawBody();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0f, 0f, WindowWidth - CloseBtnSize - 24f, TitleBarHeight));
        }

        private void DrawTopBar()
        {
            GUILayout.BeginHorizontal();
            if (StatsConfig.ShowAsCount != null && IsUsageTab(_activeTab))
            {
                string unitLabel = StatsConfig.ShowAsCount.Value ? I18n.T("panel.unit.count") : I18n.T("panel.unit.durability");
                if (GUILayout.Button(unitLabel, GUILayout.Height(44f), GUILayout.MinWidth(180f))) StatsConfig.ShowAsCount.Value = !StatsConfig.ShowAsCount.Value;
                GUILayout.Space(8f);
            }
            GUILayout.FlexibleSpace();
            string scopeLabel = _showGlobal ? I18n.T("panel.scope.global") : I18n.T("panel.scope.world");
            if (GUILayout.Button(scopeLabel, GUILayout.Height(44f), GUILayout.MinWidth(220f))) _showGlobal = !_showGlobal;
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Tabs.Length; i++)
            {
                var t = Tabs[i];
                var style = t == _activeTab ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle;
                if (GUILayout.Button(I18n.T("tab." + t), style, GUILayout.Height(64f), GUILayout.ExpandWidth(true))) { _activeTab = t; _scroll = Vector2.zero; }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void DrawBody()
        {
            if (_activeTab == TabSettings) DrawSettings();
            else if (_activeTab == TabAbout) DrawAbout();
            else DrawList();
        }

        private static bool IsUsageTab(string tab)
        {
            return tab == StatKeys.UsedFood || tab == StatKeys.UsedMedical
                || tab == StatKeys.UsedCombat || tab == StatKeys.UsedMisc;
        }

        private void DrawAbout()
        {
            _scroll = GUILayout.BeginScrollView(_scroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Space(12f);
            GUILayout.Label(I18n.T("about.title"), CenterTitleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(6f);
            GUILayout.Label(I18n.T("about.desc"), CenterLabelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label(I18n.F("about.version", AboutVersion), CenterLabelStyle, GUILayout.ExpandWidth(true));

            GUILayout.Space(16f);
            GUILayout.Label(I18n.T("about.sec_links"), CenterTitleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(6f);
            DrawLinkButton(I18n.T("about.link_repo"), "https://github.com/huanxin996/Cu-Stats");
            DrawLinkButton(I18n.T("about.link_release"), "https://github.com/huanxin996/Cu-Stats/releases/latest");

            GUILayout.Space(16f);
            GUILayout.Label(I18n.T("about.sec_credits"), CenterTitleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(6f);
            DrawNameButton("huanxin996", "https://github.com/huanxin996");

            GUILayout.Space(16f);
            GUILayout.Label(I18n.T("about.sec_deps"), CenterTitleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(6f);
            DrawLinkButton("BepInEx", "https://github.com/BepInEx/BepInEx");
            DrawLinkButton("CuSaveManager", "https://github.com/huanxin996/Cu-Save-Manager");

            GUILayout.Space(20f);
            GUILayout.EndScrollView();
        }

        private const string AboutVersion = "1.0.0";

        private static GUIStyle _centerTitle;
        private static GUIStyle CenterTitleStyle
        {
            get
            {
                if (_centerTitle == null) _centerTitle = new GUIStyle(BlackWhiteSkin.HeaderStyle) { alignment = TextAnchor.MiddleCenter };
                return _centerTitle;
            }
        }

        private static GUIStyle _centerLabel;
        private static GUIStyle CenterLabelStyle
        {
            get
            {
                if (_centerLabel == null) _centerLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                return _centerLabel;
            }
        }

        private static void DrawLinkButton(string label, string url)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(label, BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(560f), GUILayout.ExpandWidth(false), GUILayout.MinHeight(48f)))
            {
                try { Application.OpenURL(url); } catch { }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private static void DrawNameButton(string name, string url)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(name,
                GUILayout.MinWidth(280f), GUILayout.ExpandWidth(false), GUILayout.MinHeight(40f)))
            {
                try { Application.OpenURL(url); } catch { }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        private void DrawList()
        {
            var rows = BuildRows(_activeTab, _showGlobal);
            _scroll = GUILayout.BeginScrollView(_scroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (rows.Count == 0) GUILayout.Label(I18n.T("panel.empty"));
            else
            {
                foreach (var row in rows)
                {
                    GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
                    var iconRect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40), GUILayout.Height(40));
                    if (row.Icon != null) IconCache.Draw(iconRect, row.Icon);
                    GUILayout.Space(8);
                    GUILayout.Label(row.Label);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(row.ValueText);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }
            GUILayout.EndScrollView();
        }

        private bool _capturingHotkey;

        internal bool ExpectsHotkeyCapture => _open && _capturingHotkey;

        private void DrawSettings()
        {
            _scroll = GUILayout.BeginScrollView(_scroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (StatsConfig.LogVerbose != null)
            {
                DrawSettingRowToggle(I18n.T("settings.log_verbose"), StatsConfig.LogVerbose);
            }
            if (StatsConfig.UiScale != null)
            {
                GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
                GUILayout.Label(I18n.F("settings.fmt_font_scale", StatsConfig.UiScale.Value));
                GUILayout.FlexibleSpace();
                float v = GUILayout.HorizontalSlider(StatsConfig.UiScale.Value, 0.6f, 2.0f, GUILayout.Width(280));
                v = Mathf.Round(v * 20f) / 20f;
                if (!Mathf.Approximately(v, StatsConfig.UiScale.Value)) StatsConfig.UiScale.Value = v;
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            if (StatsConfig.CheckUpdate != null)
            {
                DrawSettingRowToggle(I18n.T("settings.check_update"), StatsConfig.CheckUpdate);
            }
            if (StatsConfig.ToggleHotkey != null)
            {
                DrawSettingRowHotkey(I18n.T("settings.toggle_hotkey"), StatsConfig.ToggleHotkey);
            }
            if (StatsConfig.PreferredLanguage != null)
            {
                DrawSettingRowLanguage(I18n.T("lbl.language"), StatsConfig.PreferredLanguage);
            }
            GUILayout.EndScrollView();
        }

        private static void DrawSettingRowLanguage(string label, BepInEx.Configuration.ConfigEntry<string> entry)
        {
            string mode = I18n.NormalizeLanguageMode(entry.Value);
            GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(I18n.T("opt.language_auto"),
                mode == "auto" ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(140))) entry.Value = "auto";
            GUILayout.Space(8f);
            if (GUILayout.Button(I18n.T("opt.language_zh"),
                mode == "zh" ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(120))) entry.Value = "zh";
            GUILayout.Space(8f);
            if (GUILayout.Button(I18n.T("opt.language_en"),
                mode == "en" ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(120))) entry.Value = "en";
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private static void DrawSettingRowToggle(string label, BepInEx.Configuration.ConfigEntry<bool> entry)
        {
            GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            string text = entry.Value ? I18n.T("settings.on") : I18n.T("settings.off");
            if (GUILayout.Button(text, GUILayout.MinWidth(120))) entry.Value = !entry.Value;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void DrawSettingRowHotkey(string label, BepInEx.Configuration.ConfigEntry<KeyCode> entry)
        {
            GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            string text = _capturingHotkey ? I18n.T("settings.press_a_key") : entry.Value.ToString();
            if (GUILayout.Button(text, GUILayout.MinWidth(160))) _capturingHotkey = !_capturingHotkey;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (_capturingHotkey && Event.current.type == EventType.KeyDown)
            {
                var k = Event.current.keyCode;
                if (k != KeyCode.None && k != KeyCode.Escape && k != KeyCode.Mouse0 && k != KeyCode.Mouse1)
                {
                    entry.Value = k;
                    _capturingHotkey = false;
                    Event.current.Use();
                }
                else if (k == KeyCode.Escape)
                {
                    _capturingHotkey = false;
                    Event.current.Use();
                }
            }
        }

        private static void ApplyFontScale()
        {
            float s = StatsConfig.UiScale != null ? StatsConfig.UiScale.Value : 1f;
            BlackWhiteSkin.HeaderStyle.fontSize = (int)(32 * s);
            BlackWhiteSkin.TabStyle.fontSize = (int)(26 * s);
            BlackWhiteSkin.TabActiveStyle.fontSize = (int)(26 * s);
            var skin = GUI.skin;
            skin.label.fontSize = (int)(20 * s);
            skin.button.fontSize = (int)(20 * s);
            skin.toggle.fontSize = (int)(20 * s);
            skin.box.fontSize = (int)(20 * s);
        }

        private static List<Row> BuildRows(string tab, bool global)
        {
            var store = StatsManager.Store;
            var rows = new List<Row>();
            if (store == null) return rows;
            if (tab == StatKeys.General)
            {
                long movedCm = store.Get(StatKeys.General, StatKeys.MovedCm, global);
                rows.Add(new Row(I18n.T("general.moved"), FormatDistance(movedCm), null));
                rows.Add(new Row(I18n.T("general.descended"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.DescendedLayers, global)), null));
                rows.Add(new Row(I18n.T("general.deepest"), I18n.F("fmt.layers", store.Get(StatKeys.General, StatKeys.DeepestLayer, global)), null));
                rows.Add(new Row(I18n.T("general.blocks_broken"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.BlocksBroken, global)), null));
                rows.Add(new Row(I18n.T("general.items_picked"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.ItemsPicked, global)), null));
                rows.Add(new Row(I18n.T("general.items_used"), FormatUsage(store.Get(StatKeys.General, StatKeys.ItemsUsed, global)), null));
                rows.Add(new Row(I18n.T("general.kills_mobs"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.KillsMobs, global)), null));
                rows.Add(new Row(I18n.T("general.encounters_survivors"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.EncountersSurvivors, global)), null));
                return rows;
            }
            var section = store.Section(tab, global);
            var list = new List<KeyValuePair<string, long>>(section);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            bool isBlock = tab == StatKeys.Blocks;
            bool isUsage = IsUsageTab(tab);
            bool showCount = isUsage && StatsConfig.ShowAsCount != null && StatsConfig.ShowAsCount.Value;
            if (showCount)
            {
                section = store.Section(tab + "_count", global);
                list = new List<KeyValuePair<string, long>>(section);
                list.Sort((a, b) => b.Value.CompareTo(a.Value));
            }
            foreach (var kv in list)
            {
                string label = isBlock ? NameResolver.Block(kv.Key) : NameResolver.Item(kv.Key);
                Sprite icon = isBlock ? IconCache.GetBlockSprite(kv.Key) : IconCache.GetItemSprite(kv.Key);
                string value = (isUsage && !showCount) ? FormatUsage(kv.Value) : I18n.F("fmt.count", kv.Value);
                rows.Add(new Row(label, value, icon));
            }
            return rows;
        }

        private static string FormatDistance(long cm)
        {
            float meters = cm / 100f;
            if (meters >= 1000f) return I18n.F("fmt.distance_km", meters / 1000f);
            return I18n.F("fmt.distance_m", meters);
        }

        /// <summary>把 amountX100（耐久 ×100，单一物品完整一次=100）格式化为"件等价"显示。</summary>
        private static string FormatUsage(long amountX100)
        {
            float v = amountX100 / 100f;
            return I18n.F("fmt.usage", v);
        }

        private readonly struct Row
        {
            internal readonly string Label;
            internal readonly string ValueText;
            internal readonly Sprite Icon;
            internal Row(string l, string v, Sprite icon) { Label = l; ValueText = v; Icon = icon; }
        }
    }
}
