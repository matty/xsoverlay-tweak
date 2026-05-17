using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;

namespace xsoverlay_tweak.Patches.Setting
{
    internal class SettingPage
    {
        [Serializable]
        public class TweakSettings
        {
            public bool XSOverlayTweak_EnableRefreshRate;
            public int XSOverlayTweak_RefreshRate;

            public bool XSOverlayTweak_AlwayUpdateCursor;
            public bool XSOverlayTweak_AlwaysHideCursor;
            public bool XSOverlayTweak_PhysicalMouseDetector;

            public bool XSOverlayTweak_ActivePointerColor;
            public int XSOverlayTweak_ActivePointerOpacity;
            public int XSOverlayTweak_PointerScaleMultiply;
            public bool XSOverlayTweak_PointerDoubleClickDelay;

            public bool XSOverlayTweak_MouseNavigation;
            public bool XSOverlayTweak_MouseNavigationUseModifiedKey;
        }

        [HarmonyPatch(typeof(Overlay_Manager), "OnRegisterWebviewOverlay")]
        [HarmonyPostfix]
        public static void WebviewOverlay(ref OverlayWebView wv)
        {
            if (wv.UserInterfaceSelection == OverlayWebView.UserInterfacePaths.Settings)
                InjectSettingsModule(wv);
        }

        [HarmonyPatch(typeof(ApiHandler), "OnRequestCurrentSettings")]
        [HarmonyPostfix]
        public static void OnRequestCurrentSettings(string sender)
        {
            TweakSettings settings = new()
            {
                //?? RefreshRate
                XSOverlayTweak_EnableRefreshRate = XConfig.EnableRefreshRate.Value,
                XSOverlayTweak_RefreshRate = XConfig.RefreshRate.Value,

                //?? Cursor
                XSOverlayTweak_AlwayUpdateCursor = XConfig.AlwayUpdateCursor.Value,
                XSOverlayTweak_AlwaysHideCursor = XConfig.AlwaysHideCursor.Value,
                XSOverlayTweak_PhysicalMouseDetector = XConfig.PhysicalMouseDetector.Value,

                //?? Pointer
                XSOverlayTweak_ActivePointerColor = XConfig.ActivePointerColor.Value,
                XSOverlayTweak_ActivePointerOpacity = XConfig.ActivePointerOpacity.Value,
                XSOverlayTweak_PointerScaleMultiply = XConfig.PointerScaleMultiply.Value,
                XSOverlayTweak_PointerDoubleClickDelay = XConfig.PointerDoubleClickDelay.Value,

                //?? Mouse Navigation
                XSOverlayTweak_MouseNavigation = XConfig.MouseNavigation.Value,
                XSOverlayTweak_MouseNavigationUseModifiedKey = XConfig.MouseNavigationUseModifiedKey.Value
            };

            var data = JsonUtility.ToJson(settings);
            ServerClientBridge.Instance.Api.SendMessage("UpdateSettings", data, null, sender);
        }

        [HarmonyPatch(typeof(XSettingsManager)), HarmonyPatch(nameof(XSettingsManager.SetSetting))]
        [HarmonyPrefix]
        public static bool SetSetting(string name, string value, string value1, bool sendAnalytics = true)
        {
            switch (name)
            {
                //?? RefreshRate
                case "XSOverlayTweak_EnableRefreshRate":
                    XConfig.EnableRefreshRate.Value = bool.Parse(value);
                    break;
                case "XSOverlayTweak_RefreshRate":
                    XConfig.RefreshRate.Value = int.Parse(value);
                    break;

                //?? Cursor
                case "XSOverlayTweak_AlwayUpdateCursor":
                    XConfig.AlwayUpdateCursor.Value = bool.Parse(value);
                    break;
                case "XSOverlayTweak_AlwaysHideCursor":
                    XConfig.AlwaysHideCursor.Value = bool.Parse(value);
                    break;
                case "XSOverlayTweak_PhysicalMouseDetector":
                    XConfig.PhysicalMouseDetector.Value = bool.Parse(value);
                    break;

                //?? Pointer
                case "XSOverlayTweak_ActivePointerColor":
                    XConfig.ActivePointerColor.Value = bool.Parse(value);
                    break;
                case "XSOverlayTweak_ActivePointerOpacity":
                    XConfig.ActivePointerOpacity.Value = int.Parse(value);
                    break;
                case "XSOverlayTweak_PointerScaleMultiply":
                    XConfig.PointerScaleMultiply.Value = int.Parse(value);
                    break;
                case "XSOverlayTweak_PointerDoubleClickDelay":
                    XConfig.PointerDoubleClickDelay.Value = bool.Parse(value);
                    break;

                //?? Mouse Navigation
                case "XSOverlayTweak_MouseNavigation":
                    XConfig.MouseNavigation.Value = bool.Parse(value);
                    break;
                case "XSOverlayTweak_MouseNavigationUseModifiedKey":
                    XConfig.MouseNavigationUseModifiedKey.Value = bool.Parse(value);
                    break;

                //?? About
                case "XSOverlayTweak_CheckForUpdate":
                    Utils.Update.CheckForUpdate();
                    break;
                case "XSOverlayTweak_OpenGitHub":
                    Utils.Update.OpenGitHubPage();
                    break;
            }

            return true;
        }

        public static void InjectSettingsModule(OverlayWebView wv)
        {

            // JS for inserting the actual settings page
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("xsoverlay_tweak.Patches.Setting.setting.js");
            using var reader = new StreamReader(stream);
            var jsContent = reader.ReadToEnd();
            string jsCode = $"(function() {{ {jsContent} }})();";

            // Lisen for WebView loaded
            wv._webView.WebView.LoadProgressChanged += (sender, args) =>
            {
                if (args.Type == ProgressChangeType.Finished)
                {
                    wv._webView.WebView.ExecuteJavaScript(jsCode, (result) =>
                    {
                        //Plugin.Logger.LogError($"[{wv.UserInterfaceSelection}] {result}");
                    });
                }
            };

        }
    }
}
