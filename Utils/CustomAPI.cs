using HarmonyLib;
using System;
using System.Threading.Tasks;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;

namespace xsoverlay_tweak.Utils
{
    internal class CustomAPI
    {
        public static event Action<bool> OnToggleMediaPlayer;
        public static event Action<bool> OnClickToggleMediaPlayer;

        [HarmonyPatch(typeof(Overlay_Manager), "OnRegisterWebviewOverlay")]
        [HarmonyPostfix]
        public static void InjectWristCustomAPI(OverlayWebView wv)
        {
            if (wv.UserInterfaceSelection == OverlayWebView.UserInterfacePaths.Wrist)
            {
                string jsCode = @"
                    (function() {
                        MiniToolbar.MediaPlayer.addEventListener(""click"", function (e) {
                            setTimeout(function () { 
                                Api.Send('Tweak_ClickToggleMediaPlayer', ShowMediaPlayer, null);
                            }, 10);

                            e.preventDefault();
                        });
                        
                        const original = OnToggleMediaPlayer;
                        OnToggleMediaPlayer = function(override) {
                            original(override);
                            Api.Send('Tweak_ToggleMediaPlayer', ShowMediaPlayer, null);
                        };
                    })();
                ";

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

        [HarmonyPatch(typeof(ApiHandler), "InitializeAPI")]
        [HarmonyPostfix]
        public static void AddCustomAPI(ApiHandler __instance)
        {
            __instance.Commands.Add("Tweak_ToggleMediaPlayer", delegate (string sender, string jsonData, string data)
            {
                OnToggleMediaPlayer.Invoke(bool.Parse(jsonData));
            });

            __instance.Commands.Add("Tweak_ClickToggleMediaPlayer", delegate (string sender, string jsonData, string data)
            {
                OnClickToggleMediaPlayer.Invoke(bool.Parse(jsonData));
            });
        }
    }
}
