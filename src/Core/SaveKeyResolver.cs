using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CasualtiesUnknown.Stats.Core
{
    /// <summary>解析"按存档隔离"的命名空间 key：单机用 cId，多人用 KrokMP persistent id。</summary>
    internal static class SaveKeyResolver
    {
        private static FieldInfo _wvViewField;
        private static FieldInfo _cInfoField;
        private static bool _wvProbed;

        private static Type _krokNetType;
        private static PropertyInfo _isClientProp;
        private static FieldInfo _persistentIdField;
        private static bool _krokProbed;

        /// <summary>当前存档命名空间，解不出时回落 "unknown"。</summary>
        internal static string CurrentSaveKey()
        {
            string mp = TryReadMultiplayerKey();
            if (!string.IsNullOrEmpty(mp)) return "mp_" + mp;

            int runId = TryReadRunIdFromMemory();
            if (runId == 0) runId = TryReadRunIdFromSv();
            if (runId != 0) return "sp_" + runId.ToString();

            return "unknown";
        }

        private static int TryReadRunIdFromMemory()
        {
            try
            {
                if (!_wvProbed)
                {
                    var t = AccessTools.TypeByName("WoundView");
                    if (t != null)
                    {
                        _wvViewField = AccessTools.Field(t, "view");
                        _cInfoField = AccessTools.Field(t, "cInfo");
                    }
                    _wvProbed = true;
                }
                if (_wvViewField == null || _cInfoField == null) return 0;
                var view = _wvViewField.GetValue(null);
                if (view == null) return 0;
                var arr = _cInfoField.GetValue(view) as Array;
                if (arr == null || arr.Length < 3) return 0;
                return Convert.ToInt32(arr.GetValue(2));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SaveKey] 内存读 cId 失败：" + ex.Message);
                return 0;
            }
        }

        private static int TryReadRunIdFromSv()
        {
            try
            {
                string root = Application.persistentDataPath;
                foreach (var name in new[] { "save.sv", "autosave.sv" })
                {
                    string p = Path.Combine(root, name);
                    if (!File.Exists(p)) continue;
                    int v = ParseCIdFromSv(p);
                    if (v != 0) return v;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SaveKey] 文件读 cId 失败：" + ex.Message);
            }
            return 0;
        }

        private static int ParseCIdFromSv(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0) return 0;
            string json = TryUnzipSaveFile(bytes);
            if (string.IsNullOrEmpty(json)) return 0;
            const string key = "\"cId\":";
            int idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return 0;
            int i = idx + key.Length;
            while (i < json.Length && (json[i] == ' ' || json[i] == '\"')) i++;
            int start = i;
            while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '-')) i++;
            if (i == start) return 0;
            return int.TryParse(json.Substring(start, i - start), out var v) ? v : 0;
        }

        private static string TryUnzipSaveFile(byte[] bytes)
        {
            try
            {
                var saveSystem = AccessTools.TypeByName("SaveSystem");
                if (saveSystem == null) return null;
                var unzip = AccessTools.Method(saveSystem, "Unzip", new[] { typeof(byte[]) });
                if (unzip == null) return null;
                return unzip.Invoke(null, new object[] { bytes }) as string;
            }
            catch { return null; }
        }

        private static string TryReadMultiplayerKey()
        {
            try
            {
                if (!_krokProbed)
                {
                    _krokNetType = AccessTools.TypeByName("KrokoshaCasualtiesMP.Net");
                    if (_krokNetType != null)
                    {
                        _isClientProp = AccessTools.Property(_krokNetType, "is_client");
                        _persistentIdField = AccessTools.Field(_krokNetType, "client_persistent_id");
                    }
                    _krokProbed = true;
                }
                if (_krokNetType == null || _isClientProp == null) return null;
                bool isClient = (bool)_isClientProp.GetValue(null);
                if (!isClient) return null;
                var pid = _persistentIdField?.GetValue(null) as string;
                return pid;
            }
            catch { return null; }
        }
    }
}
