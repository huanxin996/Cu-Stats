using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CasualtiesUnknown.Stats.Util;

namespace CasualtiesUnknown.Stats.UI
{
    /// <summary>主菜单注入"统计"按钮：克隆 PreRunScript.loadButton 挂到菜单根节点；仅 SaveManager 不在场时启用。</summary>
    internal static class MenuButtonInjector
    {
        private const string InjectedName = "CuStats_MenuButton";

        private static Action _onClick;

        internal static bool Active { get; private set; }

        internal static void Setup(Action onClick)
        {
            _onClick = onClick;
            Active = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (PreRunScript.instance != null) OnPreRunStarted(PreRunScript.instance);
        }

        internal static void Dispose()
        {
            try { SceneManager.sceneLoaded -= OnSceneLoaded; } catch { }
            Active = false;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "PreGen") return;
            if (PreRunScript.instance == null) return;
            try { InjectOnce(PreRunScript.instance); }
            catch (Exception ex) { ModLog.Warn("sceneLoaded 注入失败：" + ex.Message); }
        }

        internal static void OnPreRunStarted(PreRunScript pre)
        {
            try { InjectOnce(pre); }
            catch (Exception ex) { ModLog.Warn("菜单按钮注入失败：" + ex.Message); }
        }

        private static void InjectOnce(PreRunScript pre)
        {
            if (_onClick == null) return;
            if (pre == null || pre.loadButton == null) return;
            var parent = pre.transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == InjectedName) return;
            }

            var clone = UnityEngine.Object.Instantiate(pre.loadButton.gameObject, parent, false);
            clone.name = InjectedName;
            clone.SetActive(true);

            var btn = clone.GetComponent<Button>();
            if (btn != null)
            {
                int n = btn.onClick.GetPersistentEventCount();
                for (int i = 0; i < n; i++)
                {
                    btn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }
                btn.onClick.RemoveAllListeners();
                btn.interactable = true;
                btn.onClick.AddListener(() =>
                {
                    try { _onClick?.Invoke(); }
                    catch (Exception ex) { ModLog.Warn("按钮点击异常：" + ex.Message); }
                });
            }

            DestroyLocalizers(clone);
            CleanupChildren(clone);
            ReplaceText(clone, I18n.T("app.name"));

            var rt = clone.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(280f, 96f);
                rt.anchoredPosition = new Vector2(-300f, -20f);
                rt.localScale = Vector3.one;
            }
            clone.transform.SetAsLastSibling();
        }

        private static void DestroyLocalizers(GameObject clone)
        {
            var components = clone.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            foreach (var c in components)
            {
                if (c == null) continue;
                var name = c.GetType().Name;
                if (name == null) continue;
                if (name.IndexOf("Localiz", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("LocaleText", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UnityEngine.Object.Destroy(c);
                }
            }
        }

        private static void CleanupChildren(GameObject clone)
        {
            var rootTransform = clone.transform;
            for (int i = rootTransform.childCount - 1; i >= 0; i--)
            {
                var child = rootTransform.GetChild(i);
                bool hasText = child.GetComponentInChildren<Text>(true) != null;
                if (!hasText)
                {
                    var allMb = child.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (var mb in allMb)
                    {
                        if (mb == null) continue;
                        var fn = mb.GetType().FullName;
                        if (fn != null && fn.StartsWith("TMPro.TextMesh"))
                        {
                            hasText = true;
                            break;
                        }
                    }
                }
                if (!hasText) UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private static void ReplaceText(GameObject clone, string newText)
        {
            bool first = true;
            foreach (var t in clone.GetComponentsInChildren<Text>(includeInactive: true))
            {
                if (t == null) continue;
                t.text = first ? newText : "";
                first = false;
            }
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
            {
                if (mb == null) continue;
                var type = mb.GetType();
                if (type.FullName == null || !type.FullName.StartsWith("TMPro.TextMesh")) continue;
                var prop = type.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) continue;
                try { prop.SetValue(mb, first ? newText : "", null); first = false; }
                catch { }
            }
        }
    }

    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class StatsMenuPreRunStartPatch
    {
        private static void Postfix(PreRunScript __instance)
        {
            if (!MenuButtonInjector.Active) return;
            MenuButtonInjector.OnPreRunStarted(__instance);
        }
    }
}
