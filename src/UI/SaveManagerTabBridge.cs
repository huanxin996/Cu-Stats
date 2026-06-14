using System;
using HarmonyLib;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.UI
{
    /// <summary>反射对接 SaveManager.ExternalTabRegistry 把统计面板作为分页注册。</summary>
    internal static class SaveManagerTabBridge
    {
        internal static bool Register(StatsPanel panel)
        {
            try
            {
                var type = AccessTools.TypeByName("CasualtiesUnknown.SaveManager.ExternalTabRegistry");
                if (type == null) { ModLog.Warn("SaveManager 无 ExternalTabRegistry 扩展点，跳过注册"); return false; }

                Action draw = panel.DrawEmbedded;
                var registerFn = AccessTools.Method(type, "Register", new[] { typeof(Func<string>), typeof(Action) });
                if (registerFn != null)
                {
                    Func<string> title = () => I18n.T("settings.tab_title");
                    registerFn.Invoke(null, new object[] { title, draw });
                    ModLog.Info("已注册到 SaveManager 设置分页");
                    return true;
                }
                var registerStr = AccessTools.Method(type, "Register", new[] { typeof(string), typeof(Action) });
                if (registerStr != null)
                {
                    registerStr.Invoke(null, new object[] { I18n.T("settings.tab_title"), draw });
                    ModLog.Info("已注册到 SaveManager 设置分页（旧 API）");
                    return true;
                }
                return false;
            }
            catch (Exception ex) { ModLog.Warn("注册 SaveManager 分页失败：" + ex.Message); return false; }
        }
    }
}
