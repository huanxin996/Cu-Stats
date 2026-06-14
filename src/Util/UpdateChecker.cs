using System;
using System.Collections;
using BepInEx.Bootstrap;
using UnityEngine;
using UnityEngine.Networking;

namespace CasualtiesUnknown.Stats.Util
{
    /// <summary>启动时拉 GitHub releases/latest 比版本，新版在屏幕左上角红字提示并可点开 release 页；受 StatsConfig.CheckUpdate 控制。</summary>
    [DisallowMultipleComponent]
    internal sealed class UpdateChecker : MonoBehaviour
    {
        private const string ApiUrl = "https://api.github.com/repos/huanxin996/Cu-Stats/releases/latest";
        private const string ReleasesUrl = "https://github.com/huanxin996/Cu-Stats/releases";
        private const int UpdateNoticeSlot = 2;

        private static bool _checked;
        private static string _currentVersion = "";
        private static string _latestTag = "";
        private static bool _updateAvailable;

        private void Start()
        {
            if (_checked) return;
            if (Core.StatsConfig.CheckUpdate == null || !Core.StatsConfig.CheckUpdate.Value) return;
            _checked = true;
            try
            {
                if (Chainloader.PluginInfos.TryGetValue(Plugin.PluginGuid, out var info))
                    _currentVersion = info.Metadata.Version.ToString();
            }
            catch (Exception ex) { ModLog.Warn("UpdateChecker init: " + ex.Message); return; }
            StartCoroutine(Check());
        }

        private static IEnumerator Check()
        {
            using (var www = UnityWebRequest.Get(ApiUrl))
            {
                www.SetRequestHeader("User-Agent", "Cu-Stats");
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success) { ModLog.Warn("Update check failed: " + www.error); yield break; }
                _latestTag = ParseTag(www.downloadHandler.text);
                if (string.IsNullOrEmpty(_latestTag)) yield break;
                if (IsNewer(_currentVersion, _latestTag))
                {
                    _updateAvailable = true;
                    ModLog.Warn("CuStats 有新版本：" + _currentVersion + " → " + _latestTag);
                }
                else ModLog.Info("CuStats 已是最新（" + _currentVersion + "）");
            }
        }

        private static string ParseTag(string json)
        {
            const string key = "\"tag_name\":\"";
            int idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return null;
            int start = idx + key.Length;
            int end = json.IndexOf('"', start);
            return end > start ? json.Substring(start, end - start) : null;
        }

        private static bool IsNewer(string current, string latest)
        {
            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(latest)) return false;
            return Version.TryParse(current.TrimStart('v', 'V').Trim(), out var vc)
                && Version.TryParse(latest.TrimStart('v', 'V').Trim(), out var vl)
                && vl > vc;
        }

        private void OnGUI()
        {
            if (!_updateAvailable) return;
            if (Core.StatsConfig.CheckUpdate == null || !Core.StatsConfig.CheckUpdate.Value) return;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                richText = false,
                normal = { textColor = new Color(1f, 0.3f, 0.3f) },
                hover = { textColor = new Color(1f, 0.55f, 0.55f) },
                active = { textColor = new Color(1f, 0.2f, 0.2f) },
            };
            string text = I18n.F("update.available", _latestTag);
            float x = 32f;
            float y = Screen.height * 0.12f + UpdateNoticeSlot * 40f;
            Vector2 size = style.CalcSize(new GUIContent(text));
            var rect = new Rect(x, y, size.x + 8f, size.y + 4f);
            if (GUI.Button(rect, text, style)) Application.OpenURL(ReleasesUrl);
        }
    }
}
