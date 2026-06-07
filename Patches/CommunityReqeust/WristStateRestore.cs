using HarmonyLib;
using System.Threading.Tasks;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class WristStateSave
    {

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void ListenForChanging()
        {
            XSOEventSystem.OnStartStopPerformanceMonitor += (enable) =>
            {
                CutomSettings.Settings.IsPerformanceMonitorOpened = enable;
                CutomSettings.SaveSettings();
            };

            CustomAPI.OnClickToggleMediaPlayer += (enable) =>
            {
                CutomSettings.Settings.IsMediaPlayerOpened = enable;
                CutomSettings.SaveSettings();
            };
        }

        [HarmonyPatch(typeof(Overlay_Manager), "OnRegisterWebviewOverlay")]
        [HarmonyPostfix]
        public static void RestoreWristState(OverlayWebView wv)
        {
            if (wv.UserInterfaceSelection == OverlayWebView.UserInterfacePaths.Wrist)
            {
                string jsCode = string.Format(@"(function() {{
                        if ({0}) {{
                            MiniToolbar.PerformanceStats.click();
                        }};

                        if ({1} && !GetShowMediaPlayer()) {{
                            MiniToolbar.MediaPlayer.click();
                        }};
                    }})()", CutomSettings.Settings.IsPerformanceMonitorOpened.ToString().ToLower(), CutomSettings.Settings.IsMediaPlayerOpened.ToString().ToLower());

                wv._webView.WebView.LoadProgressChanged += (sender, args) =>
                {
                    if (args.Type == ProgressChangeType.Finished)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(1000);

                            wv._webView.WebView.ExecuteJavaScript(jsCode, (result) =>
                            {
                                //Plugin.Logger.LogError($"[{wv.UserInterfaceSelection}] {result}");
                            });
                        });
                    }
                };
            }
        }
    }
}
