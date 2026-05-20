using HarmonyLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using XSOverlay;

namespace xsoverlay_tweak.Utils
{
    internal class EventBridge
    {
        private static CancellationTokenSource NotificationCancelToken;

        public static bool IsNotificationVisible = false;
        public static bool IsHoverAnyOverlay = false;

        [HarmonyPatch(typeof(DeviceManager), "Start")]
        [HarmonyPostfix]
        public static void Start()
        {
            // Listen to notification push
            XSOEventSystem.OnQueueNotification += (notify) =>
            {
                IsNotificationVisible = true;

                // Cancel any previous notification timer
                NotificationCancelToken?.Cancel();
                NotificationCancelToken = new CancellationTokenSource();
                CancellationToken token = NotificationCancelToken.Token;

                Task.Delay(TimeSpan.FromSeconds(notify.timeout), token).ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                        IsNotificationVisible = false;
                });
            };

            // Listen to hovering overlay change
            {
                XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
                {
                    IsHoverAnyOverlay = true;
                };

                XSOEventSystem.OnReleaseControlOfDesktopCursor += (raycaster) =>
                {
                    IsHoverAnyOverlay = false;
                };
            }
        }
    }
}
