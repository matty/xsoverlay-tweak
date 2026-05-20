using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using XSOverlay;
using XSOverlay.Websockets.API;

namespace xsoverlay_tweak.Utils
{
    internal class EventBridge
    {
        private static Coroutine NotificationCoroutine;

        public static bool IsNotificationVisible = false;
        public static bool IsHoverAnyOverlay = false;

        public static event Action<Objects.NotificationObject> OnQueueNotification;
        public static event Action<Raycaster, Unity_Overlay> OnSwitchHoveringOverlay;
        public static event Action<Raycaster> OnReleaseControlOfDesktopCursor;


        [HarmonyPatch(typeof(DeviceManager), "Start")]
        [HarmonyPostfix]
        public static void Start()
        {
            // Listen to notification push
            XSOEventSystem.OnQueueNotification += (notify) =>
            {
                IsNotificationVisible = true;

                if (NotificationCoroutine != null)
                    Plugin.Instance.StopCoroutine(NotificationCoroutine);

                NotificationCoroutine = Plugin.Instance.StartCoroutine(NotificationTimer(notify.timeout));

                OnQueueNotification?.Invoke(notify);
            };

            // Listen to hovering overlay change
            {
                XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
                {
                    IsHoverAnyOverlay = true;
                    OnSwitchHoveringOverlay?.Invoke(raycaster, overlay);
                };

                XSOEventSystem.OnReleaseControlOfDesktopCursor += (raycaster) =>
                {
                    IsHoverAnyOverlay = false;
                    OnReleaseControlOfDesktopCursor?.Invoke(raycaster);
                };
            }

        }

        private static IEnumerator NotificationTimer(float timeout)
        {
            yield return new WaitForSecondsRealtime(timeout);
            IsNotificationVisible = false;
            NotificationCoroutine = null;
        }
    }
}
