using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.Core
{
    /// <summary>双层统计落盘：global.json 跨存档总计 + perSave/&lt;saveKey&gt;.json 当前世界明细，分区→键→计数。</summary>
    internal sealed class StatsStore
    {
        private readonly string _globalPath;
        private readonly string _perSaveDir;
        private string _currentSaveKey;
        private Dictionary<string, Dictionary<string, long>> _global;
        private Dictionary<string, Dictionary<string, long>> _perSave = new Dictionary<string, Dictionary<string, long>>();
        private bool _globalDirty;
        private bool _perSaveDirty;

        internal StatsStore()
        {
            string root = Path.Combine(Paths.PluginPath, "CuStats");
            _globalPath = Path.Combine(root, "global.json");
            _perSaveDir = Path.Combine(root, "perSave");
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(_perSaveDir);
            _global = LoadFile(_globalPath);
        }

        internal string CurrentSaveKey => _currentSaveKey;

        internal void SwitchSaveKey(string saveKey)
        {
            if (saveKey == _currentSaveKey) return;
            Flush();
            _currentSaveKey = saveKey;
            _perSave = LoadFile(PerSavePath(saveKey));
            _perSaveDirty = false;
        }

        /// <summary>同时累加到全局总计与当前世界，amount 可正可负。</summary>
        internal void Add(string section, string key, long amount)
        {
            if (amount == 0 || string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key)) return;
            AddTo(_global, section, key, amount); _globalDirty = true;
            if (!string.IsNullOrEmpty(_currentSaveKey))
            {
                AddTo(_perSave, section, key, amount); _perSaveDirty = true;
            }
        }

        /// <summary>分别在全局和当前世界对该键取 max(value, 已有)。</summary>
        internal void SetMax(string section, string key, long value)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key)) return;
            if (SetMaxIn(_global, section, key, value)) _globalDirty = true;
            if (!string.IsNullOrEmpty(_currentSaveKey) && SetMaxIn(_perSave, section, key, value)) _perSaveDirty = true;
        }

        internal long Get(string section, string key, bool global)
        {
            var data = global ? _global : _perSave;
            return data.TryGetValue(section, out var bucket) && bucket.TryGetValue(key, out var v) ? v : 0;
        }

        internal IReadOnlyDictionary<string, long> Section(string section, bool global)
        {
            var data = global ? _global : _perSave;
            return data.TryGetValue(section, out var bucket) ? bucket : EmptyBucket;
        }

        internal void Flush()
        {
            if (_globalDirty) { WriteFile(_globalPath, _global); _globalDirty = false; }
            if (_perSaveDirty && !string.IsNullOrEmpty(_currentSaveKey))
            {
                WriteFile(PerSavePath(_currentSaveKey), _perSave);
                _perSaveDirty = false;
            }
        }

        private static void AddTo(Dictionary<string, Dictionary<string, long>> data, string section, string key, long amount)
        {
            if (!data.TryGetValue(section, out var bucket))
            {
                bucket = new Dictionary<string, long>();
                data[section] = bucket;
            }
            bucket.TryGetValue(key, out var cur);
            bucket[key] = cur + amount;
        }

        private static bool SetMaxIn(Dictionary<string, Dictionary<string, long>> data, string section, string key, long value)
        {
            if (!data.TryGetValue(section, out var bucket))
            {
                bucket = new Dictionary<string, long>();
                data[section] = bucket;
            }
            bucket.TryGetValue(key, out var cur);
            if (value > cur) { bucket[key] = value; return true; }
            return false;
        }

        private static readonly Dictionary<string, long> EmptyBucket = new Dictionary<string, long>();

        private string PerSavePath(string saveKey) => Path.Combine(_perSaveDir, SafeFileName(saveKey) + ".json");

        private static string SafeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "unknown";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        private static Dictionary<string, Dictionary<string, long>> LoadFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return new Dictionary<string, Dictionary<string, long>>();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(File.ReadAllText(path));
                return dict ?? new Dictionary<string, Dictionary<string, long>>();
            }
            catch (Exception ex)
            {
                ModLog.Warn("[Store] 读取 " + path + " 失败：" + ex.Message);
                return new Dictionary<string, Dictionary<string, long>>();
            }
        }

        private static void WriteFile(string path, Dictionary<string, Dictionary<string, long>> data)
        {
            try { File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented)); }
            catch (Exception ex) { ModLog.Warn("[Store] 写入 " + path + " 失败：" + ex.Message); }
        }
    }
}
