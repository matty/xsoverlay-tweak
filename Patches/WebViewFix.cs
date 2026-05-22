using HarmonyLib;
using System.Collections.Generic;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    internal class WebViewFix
    {
        private static readonly List<Unity_Overlay> DisabledOverlays = [];

        [HarmonyPatch(typeof(DeviceManager), "Start")]
        [HarmonyPostfix]
        public static void OnSwitchHoveringOverlay()
        {
            XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
            {
                if (!IsEnable()) return;

                if (overlay != null) // Swith in
                {
                    if (IsWebView(overlay))
                        overlay.OverlayWebView._webView.WebView.SetRenderingEnabled(true);
                }
                else // Switch out
                {
                    Unity_Overlay CachedHoveringOverlay = (Unity_Overlay)AccessTools.Field(typeof(Raycaster), "CachedHoveringOverlay").GetValue(raycaster);
                    if (IsWebView(CachedHoveringOverlay))
                        CachedHoveringOverlay.OverlayWebView._webView.WebView.SetRenderingEnabled(false);
                }
            };
        }

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPrefix]
        public static bool Pre(Raycaster __instance)
        {
            if (!IsEnable()) return true;

            foreach (Unity_Overlay overlay in Overlay_Manager.Instance.AllSceneOverlays)
                if (IsWebView(overlay))
                    if (overlay != __instance.HoveringOverlay)
                    {
                        overlay.OverlayWebView._webView.WebView.SetRenderingEnabled(false);
                        DisabledOverlays.Add(overlay);
                    }

            foreach (Unity_Overlay overlay in Overlay_Manager.Instance.AllSceneOverlays)
                if (IsWebView(overlay))
                    if (overlay == __instance.HoveringOverlay)
                        overlay.OverlayWebView._webView.WebView.SetRenderingEnabled(true);

            return true;
        }

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPostfix]
        public static void Post()
        {
            if (!IsEnable()) return;

            foreach (Unity_Overlay item in DisabledOverlays)
            {
                if (item.overlayName.Equals("window.settings"))
                    item.OverlayWebView._webView.WebView.SetRenderingEnabled(true);
            }

            DisabledOverlays.Clear();
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
