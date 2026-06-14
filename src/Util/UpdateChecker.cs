using System;
using System.Collections;
using BepInEx.Bootstrap;
using UnityEngine;
using UnityEngine.Networking;

namespace CasualtiesUnknown.Stats.Util
{
    /// <summary>启动时拉 GitHub releases/latest 比对版本，发现新版输出 Warn 提示；受 StatsConfig.CheckUpdate 控制。</summary>
    [DisallowMultipleComponent]
    internal sealed class UpdateChecker : MonoBehaviour
    {
        private const string ApiUrl = "https://api.github.com/repos/huanxin996/Cu-Stats/releases/latest";

        private static bool _checked;

        private void Start()
        {
            if (_checked) return;
            if (Core.StatsConfig.CheckUpdate == null || !Core.StatsConfig.CheckUpdate.Value) return;
            _checked = true;
            string current = "";
            try
            {
                if (Chainloader.PluginInfos.TryGetValue(Plugin.PluginGuid, out var info))
                    current = info.Metadata.Version.ToString();
            }
            catch (Exception ex) { ModLog.Warn("UpdateChecker init: " + ex.Message); return; }
            StartCoroutine(Check(current));
        }

        private static IEnumerator Check(string current)
        {
            using (var www = UnityWebRequest.Get(ApiUrl))
            {
                www.SetRequestHeader("User-Agent", "Cu-Stats");
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success) { ModLog.Warn("Update check failed: " + www.error); yield break; }
                string tag = ParseTag(www.downloadHandler.text);
                if (string.IsNullOrEmpty(tag)) yield break;
                if (IsNewer(current, tag)) ModLog.Warn("CuStats 有新版本：" + current + " → " + tag);
                else ModLog.Info("CuStats 已是最新（" + current + "）");
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
    }
}
