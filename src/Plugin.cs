using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.UI;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats
{
    /// <summary>统计 mod 入口：装配 Harmony patch、初始化 Manager、维护 UI 与节流落盘。</summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(SaveManagerGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "com.mod.casualties.stats";
        private const string PluginName = "CuStats";
        private const string PluginVersion = "0.1.0";
        private const string SaveManagerGuid = "com.casualtiesUnknown.saveManager";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private Harmony _harmony;
        private StatsPanel _panel;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            try
            {
                StatsConfig.Bind(Config);
                _harmony = new Harmony(PluginGuid);
                _harmony.PatchAll(typeof(Plugin).Assembly);
                StatsManager.Init();
                _panel = new StatsPanel();
                if (Chainloader.PluginInfos.ContainsKey(SaveManagerGuid))
                {
                    SaveManagerTabBridge.Register(_panel);
                }
                gameObject.AddComponent<UpdateChecker>();
                Log.LogInfo($"{PluginName} v{PluginVersion} 已加载");
            }
            catch (Exception ex)
            {
                Log.LogError($"[CuStats] 初始化失败：{ex}");
                throw;
            }
        }

        private void OnDestroy()
        {
            try { StatsManager.Flush(); } catch { }
            try { _harmony?.UnpatchSelf(); } catch { }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U)) _panel?.Toggle();
            if (_panel != null && _panel.Open && Input.GetKeyDown(KeyCode.Escape)) _panel.Close();
            Triggers.MovementTracker.Tick();
            StatsManager.Tick(Time.unscaledDeltaTime);
        }

        private void OnGUI() => _panel?.Draw();
    }
}
