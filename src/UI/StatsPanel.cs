using System.Collections.Generic;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.UI
{
    /// <summary>仿 MC 统计面板：scope 切换 + 类目 tab + 列表区 + 设置页；独立窗口与嵌入 saveManager 分页共用同一渲染。</summary>
    internal sealed class StatsPanel
    {
        private const string TabSettings = "settings";
        private static readonly string[] Tabs =
        {
            StatKeys.General, StatKeys.Blocks, StatKeys.Picked,
            StatKeys.UsedFood, StatKeys.UsedMedical, StatKeys.UsedCombat, StatKeys.UsedMisc,
            TabSettings,
        };

        private bool _open;
        private bool _showGlobal = true;
        private string _activeTab = StatKeys.General;
        private Vector2 _scroll;

        internal bool Open => _open;
        internal void Toggle() { _open = !_open; }
        internal void Close() { _open = false; }

        internal void Draw()
        {
            if (!_open) return;
            BlackWhiteSkin.Push();
            try
            {
                ApplyFontScale();
                int sw = Screen.width, sh = Screen.height;
                int w = Mathf.Min(1200, sw - 80);
                int h = Mathf.Min(800, sh - 80);
                var rect = new Rect((sw - w) * 0.5f, (sh - h) * 0.5f, w, h);
                GUI.Box(rect, GUIContent.none);
                BlackWhiteSkin.DrawBorder(rect, 2f);

                float scale = StatsConfig.UiScale != null ? StatsConfig.UiScale.Value : 1f;
                int headerH = (int)(60 * scale);
                int tabsH = (int)(56 * scale + 12);
                int footerH = (int)(36 * scale + 16);
                float bodyH = rect.height - 40 - headerH - tabsH - footerH;

                GUILayout.BeginArea(new Rect(rect.x + 24, rect.y + 20, rect.width - 48, rect.height - 40));
                DrawHeader(true, headerH);
                DrawTabs(tabsH);
                DrawBody(bodyH);
                DrawFooter();
                GUILayout.EndArea();
            }
            finally { BlackWhiteSkin.Pop(); }
        }

        /// <summary>嵌入 saveManager 设置分页时调用：宿主控制窗口外框，不画自身边框 / 关闭按钮。</summary>
        internal void DrawEmbedded()
        {
            ApplyFontScale();
            float scale = StatsConfig.UiScale != null ? StatsConfig.UiScale.Value : 1f;
            DrawHeader(false, (int)(60 * scale));
            DrawTabs((int)(56 * scale + 12));
            DrawBody(Screen.height * 0.55f);
        }

        private void DrawHeader(bool withClose, int headerH)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(headerH));
            GUILayout.Label(I18n.T("panel.title"), BlackWhiteSkin.HeaderStyle);
            GUILayout.FlexibleSpace();
            string scopeLabel = _showGlobal ? I18n.T("panel.scope.global") : I18n.T("panel.scope.world");
            if (GUILayout.Button(scopeLabel, GUILayout.Height(headerH * 0.7f), GUILayout.MinWidth(220))) _showGlobal = !_showGlobal;
            if (withClose)
            {
                if (GUILayout.Button(I18n.T("panel.close"), GUILayout.Height(headerH * 0.7f), GUILayout.MinWidth(120))) _open = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            BlackWhiteSkin.DrawHLine(GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true)));
            GUILayout.Space(4);
        }

        private void DrawTabs(int tabsH)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Tabs.Length; i++)
            {
                var t = Tabs[i];
                var style = t == _activeTab ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle;
                if (GUILayout.Button(I18n.T("tab." + t), style, GUILayout.Height(tabsH), GUILayout.MinWidth(120))) { _activeTab = t; _scroll = Vector2.zero; }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void DrawBody(float bodyH)
        {
            if (_activeTab == TabSettings) DrawSettings(bodyH);
            else DrawList(bodyH);
        }

        private void DrawList(float listHeight)
        {
            var rows = BuildRows(_activeTab, _showGlobal);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Max(120, listHeight)));
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

        private void DrawSettings(float bodyH)
        {
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Max(120, bodyH)));
            if (StatsConfig.LogVerbose != null)
            {
                GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
                GUILayout.Label(I18n.T("settings.log_verbose"));
                GUILayout.FlexibleSpace();
                bool v = GUILayout.Toggle(StatsConfig.LogVerbose.Value, StatsConfig.LogVerbose.Value ? I18n.T("settings.on") : I18n.T("settings.off"), GUILayout.MinWidth(120));
                if (v != StatsConfig.LogVerbose.Value) StatsConfig.LogVerbose.Value = v;
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
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
                GUILayout.Space(2);
            }
            if (StatsConfig.CheckUpdate != null)
            {
                GUILayout.BeginHorizontal(BlackWhiteSkin.CardStyle);
                GUILayout.Label(I18n.T("settings.check_update"));
                GUILayout.FlexibleSpace();
                bool v = GUILayout.Toggle(StatsConfig.CheckUpdate.Value, StatsConfig.CheckUpdate.Value ? I18n.T("settings.on") : I18n.T("settings.off"), GUILayout.MinWidth(120));
                if (v != StatsConfig.CheckUpdate.Value) StatsConfig.CheckUpdate.Value = v;
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            GUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            GUILayout.Space(4);
            BlackWhiteSkin.DrawHLine(GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true)));
            GUILayout.Space(2);
            GUILayout.Label(I18n.T("panel.hint"));
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
                rows.Add(new Row(I18n.T("general.items_used"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.ItemsUsed, global)), null));
                rows.Add(new Row(I18n.T("general.kills_mobs"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.KillsMobs, global)), null));
                rows.Add(new Row(I18n.T("general.encounters_survivors"), I18n.F("fmt.count", store.Get(StatKeys.General, StatKeys.EncountersSurvivors, global)), null));
                return rows;
            }
            var section = store.Section(tab, global);
            var list = new List<KeyValuePair<string, long>>(section);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            bool isBlock = tab == StatKeys.Blocks;
            foreach (var kv in list)
            {
                string label = isBlock ? NameResolver.Block(kv.Key) : NameResolver.Item(kv.Key);
                Sprite icon = isBlock ? IconCache.GetBlockSprite(kv.Key) : IconCache.GetItemSprite(kv.Key);
                rows.Add(new Row(label, I18n.F("fmt.count", kv.Value), icon));
            }
            return rows;
        }

        private static string FormatDistance(long cm)
        {
            float meters = cm / 100f;
            if (meters >= 1000f) return I18n.F("fmt.distance_km", meters / 1000f);
            return I18n.F("fmt.distance_m", meters);
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
