using HarmonyLib;
using System.IO;
using System.Threading.Tasks;
using XSOverlay;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.CommunityRequest
{
    internal class WindowToolbarKeyboard
    {
        private static bool WasEnable = false;

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void AddWindowToolbarKeybordButton(ApiHandler __instance)
        {
            XConfig.WindowToolbarKeyboard.SettingChanged += (sender, args) =>
            {
                EditToolbarJsFile();

                XSTools.ExecuteOnMainThread(async () =>
                {
                    await Task.Delay(200);

                    OverlayWebView toolbarWebView = Overlay_Manager.Instance.WindowToolbar.GetComponentInChildren<Unity_Overlay>(true).OverlayWebView;

                    toolbarWebView.DisableOnStart = false;
                    toolbarWebView._webView.WebView.Reload();
                    toolbarWebView._webView.WebView.SetRenderingEnabled(true);

                    ChangeToolbarWidth(toolbarWebView, false);
                });
            };
        }

        [HarmonyPatch(typeof(OverlayWebView), "Awake")]
        [HarmonyPrefix]
        public static void ChangeWindowToolbarDimension(OverlayWebView __instance)
        {
            if (__instance.UserInterfaceSelection == OverlayWebView.UserInterfacePaths.WindowToolbar)
            {
                EditToolbarJsFile();
                ChangeToolbarWidth(__instance, true);
            }
        }

        [HarmonyPatch(typeof(Overlay_Manager), "EnableKeyboard")]
        [HarmonyPostfix]
        public static void SpawnKeyboardPostionFix(Overlay_Manager __instance, KeyboardGlobalManager ___keyboardManager)
        {
            if (!IsEnable()) return;

            if (EventBridge.CurrentHoveringOverlay?.overlayName == "window.toolbar" && ___keyboardManager?.HasKeyboardBeenOpened == true)
            {
                __instance.Keyboard_Overlay.transform.position = OverlaySwitcher.Instance.ToolBarWindowOverlay.transform.position;
                __instance.Keyboard_Overlay.transform.rotation = OverlaySwitcher.Instance.ToolBarWindowOverlay.transform.rotation;
                __instance.Keyboard_Overlay.transform.position += __instance.Keyboard_Overlay.transform.forward * -0.2f;
            }
        }

        private static void ChangeToolbarWidth(OverlayWebView webView, bool isAwake)
        {
            float targetWidth = webView.Width;
            bool isChanged = false;

            if (IsEnable())
            {
                isChanged = true;
                WasEnable = true;
                targetWidth += 110;
            }
            else if (WasEnable)
            {
                isChanged = true;
                WasEnable = false;
                targetWidth -= 110;
            }

            if (isChanged)
            {
                webView.Width = targetWidth;

                if (!isAwake)
                    webView.UpdateResolution(new UnityEngine.Resolution { width = (int)webView.Width, height = (int)webView.Height });
            }
        }

        private static void EditToolbarJsFile()
        {
            string filePath = @".\XSOverlay_Data\StreamingAssets\Plugins\Applications\_UI\Default\_Shared\js\toolbar.js";
            if (!File.Exists(filePath)) return;

            string content = File.ReadAllText(filePath);
            string original = "var windowToolbarLookup = {\r\n    \"WindowSettings\": \"gear-fill\",";
            string edited = "var windowToolbarLookup = {\r\n    \"Keyboard\": \"keyboard-fill\",\r\n\t\"WindowSettings\": \"gear-fill\",";

            if (content.Contains(original) && IsEnable())
            {
                string patched = content.Replace(original, edited);
                File.WriteAllText(filePath, patched);
            }
            else if (content.Contains(edited) && !IsEnable())
            {
                string patched = content.Replace(edited, original);
                File.WriteAllText(filePath, patched);
            }
        }

        private static bool IsEnable()
        {
            return XConfig.WindowToolbarKeyboard.Value;
        }
    }
}
