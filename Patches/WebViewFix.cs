using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    internal class WebViewFix
    {
        private static readonly List<Unity_Overlay> DisabledOverlays = [];
        private static Coroutine StopingCoroutine;

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void OnSwitchHoveringOverlay()
        {
            XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
            {
                if (!IsEnable()) return;

                if (StopingCoroutine != null)
                    Plugin.Instance.StopCoroutine(StopingCoroutine);
                StopingCoroutine = Plugin.Instance.StartCoroutine(StopingDelay(overlay));

                if (overlay != null && IsWebView(overlay))
                    overlay.OverlayWebView._webView.WebView.SetRenderingEnabled(true);
            };
        }

        //!! Not work by clicking
        /*[HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPrefix]
        public static bool ClickWebView(Raycaster __instance)
        {
            if (!IsEnable()) return true;

            Plugin.Logger.LogError(__instance.HoveringOverlay.overlayName);
            if (IsWebView(__instance.HoveringOverlay))
            {
                if (StopingCoroutine != null)
                    Plugin.Instance.StopCoroutine(StopingCoroutine);
                StopingCoroutine = Plugin.Instance.StartCoroutine(StopingDelay(__instance.HoveringOverlay));

                __instance.HoveringOverlay.OverlayWebView._webView.WebView.SetRenderingEnabled(true);
            }

            return true;
        }*/

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPostfix]
        public static void WindowSettingsSwitch(Raycaster __instance)
        {
            if (!IsEnable()) return;

            if (__instance.HoveringOverlay.overlayName.Equals("window.toolbar"))
                foreach (Unity_Overlay allOverlay in Overlay_Manager.Instance.AllSceneOverlays)
                    if (allOverlay.overlayName.Equals("window.settings"))
                        allOverlay.OverlayWebView._webView.WebView.SetRenderingEnabled(true);
        }

        private static IEnumerator StopingDelay(Unity_Overlay overlay)
        {
            yield return new WaitForSecondsRealtime(0.17f);

            foreach (Unity_Overlay allOverlay in Overlay_Manager.Instance.AllSceneOverlays)
                if (IsWebView(allOverlay))
                    if (allOverlay != overlay)
                        allOverlay.OverlayWebView._webView.WebView.SetRenderingEnabled(false);
        }

        private static bool IsWebView(Unity_Overlay overlay)
        {
            string overlayName = overlay?.overlayName ?? "";
            return overlay.WebViewHandler != null && overlay.IsPluginApplication && !overlay.IsDesktopOrWindowCapture && !overlayName.Equals("wrist") && !overlayName.Equals("notification");
        }

        private static bool IsEnable()
        {
            return XConfig.WebViewFix.Value;
        }
    }
}
