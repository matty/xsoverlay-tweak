using HarmonyLib;
using System.Threading.Tasks;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class HideBattery
    {
        private static OverlayWebView WristWebView;

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void Init()
        {
            XConfig.HideBattery.SettingChanged += (sender, args) =>
            {
                ExecuteJava();
            };
        }

        [HarmonyPatch(typeof(Overlay_Manager), "OnRegisterWebviewOverlay")]
        [HarmonyPostfix]
        public static void RestoreWristState(OverlayWebView wv)
        {
            if (wv.UserInterfaceSelection == OverlayWebView.UserInterfacePaths.Wrist)
            {
                WristWebView = wv;

                wv._webView.WebView.LoadProgressChanged += (sender, args) =>
                {
                    if (args.Type == ProgressChangeType.Finished)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(1000);

                            ExecuteJava();
                        });
                    }
                };
            }
        }

        private static void ExecuteJava()
        {
            string jsCode = $@"
                (function() {{
                    var elements = document.getElementsByClassName('battery-widget');
                    for (var i = 0; i < elements.length; i++) {{
                        elements[i].style.display = '{(IsEnable() ? "none" : "block")}';
                    }}
                }})();";

            WristWebView._webView.WebView.ExecuteJavaScript(jsCode, (result) =>
            {
                //Plugin.Logger.LogError($"[{wv.UserInterfaceSelection}] {result}");
            });
        }

        public static bool IsEnable()
        {
            return XConfig.HideBattery.Value;
        }
    }
}
