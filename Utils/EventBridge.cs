using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Utils
{
    internal class EventBridge
    {
        private static Coroutine NotificationCoroutine;

        public static bool IsNotificationVisible = false;
        public static bool IsHoverAnyOverlay = false;

        private static readonly MethodInfo GetHMDRefreshRate = AccessTools.Method(typeof(DeviceManager), "GetHMDRefreshRate");

        [HarmonyPatch(typeof(DeviceManager), "Start")]
        [HarmonyPostfix]
        public static void Start(DeviceManager __instance)
        {
            // Listen to notification push
            XSOEventSystem.OnQueueNotification += (notify) =>
            {
                IsNotificationVisible = true;

                if (NotificationCoroutine != null)
                    Plugin.Instance.StopCoroutine(NotificationCoroutine);
                NotificationCoroutine = Plugin.Instance.StartCoroutine(NotificationTimer(notify.timeout));

                GetHMDRefreshRate.Invoke(__instance, null);
            };

            // Listen to hovering overlay change
            {
                XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
                {
                    IsHoverAnyOverlay = true;
                    GetHMDRefreshRate.Invoke(__instance, null);
                };

                XSOEventSystem.OnTakeControlOfDesktopCursor += (raycaster) =>
                {
                    IsHoverAnyOverlay = true;
                    GetHMDRefreshRate.Invoke(__instance, null);
                };

                XSOEventSystem.OnReleaseControlOfDesktopCursor += (raycaster) =>
                {
                    IsHoverAnyOverlay = false;
                    GetHMDRefreshRate.Invoke(__instance, null);
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
