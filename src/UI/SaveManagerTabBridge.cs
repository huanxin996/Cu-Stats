using System;
using HarmonyLib;
using CasualtiesUnknown.Stats.Core;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.UI
{
    /// <summary>反射对接 SaveManager.ExternalTabRegistry 把统计面板嵌入侧栏分页。</summary>
    internal static class SaveManagerTabBridge
    {
        internal static bool Register(StatsPanel panel)
        {
            try
            {
                var type = AccessTools.TypeByName("CasualtiesUnknown.SaveManager.ExternalTabRegistry");
                if (type == null) { ModLog.Warn("SaveManager 无 ExternalTabRegistry 扩展点，跳过注册"); return false; }
                Action draw = panel.DrawEmbedded;
                Func<string> title = () => I18n.T("settings.tab_title");
                Func<string> status = () =>
                {
                    string saveKey = StatsManager.Store?.CurrentSaveKey ?? "unknown";
                    return I18n.F("status.bottom", saveKey);
                };
                var fn3 = AccessTools.Method(type, "Register", new[] { typeof(Func<string>), typeof(Action), typeof(Func<string>) });
                if (fn3 != null) { fn3.Invoke(null, new object[] { title, draw, status }); ModLog.Info("已注册到 SaveManager 设置侧栏"); return true; }
                var fn = AccessTools.Method(type, "Register", new[] { typeof(Func<string>), typeof(Action) });
                if (fn == null) return false;
                fn.Invoke(null, new object[] { title, draw });
                ModLog.Info("已注册到 SaveManager 设置侧栏");
                return true;
            }
            catch (Exception ex) { ModLog.Warn("注册 SaveManager 侧栏失败：" + ex.Message); return false; }
        }
    }
}
