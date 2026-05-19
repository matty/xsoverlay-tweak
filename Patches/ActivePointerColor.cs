using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class ActivePointerColor
    {
        [HarmonyPatch("UpdateHoveringOverlay")]
        [HarmonyPostfix]
        public static void UpdateHoveringOverlay(Raycaster __instance, ref Unity_Overlay ___VisualCursorElementOverlay)
        {
            if (!IsEnable()) return;
            if (!IsHand(__instance)) return;

            if (!IsActiveHand(__instance))
                ___VisualCursorElementOverlay.colorTint = Color.red;
            else if (!__instance.HoveringOverlay.IsLocked)
                ___VisualCursorElementOverlay.colorTint = XSettingsManager.Instance.Settings.AccentColor;
        }

        [HarmonyPatch("DetermineCursorVisibility")]
        [HarmonyPostfix]
        public static void DetermineCursorVisibility(Raycaster __instance, ref Unity_Overlay ___VisualCursorElementOverlay)
        {
            if (!IsEnable()) return;
            if (!IsHand(__instance)) return;

            if (!IsActiveHand(__instance))
                if (___VisualCursorElementOverlay.opacity.Equals(1))
                    ___VisualCursorElementOverlay.opacity = XConfig.ActivePointerOpacity.Value / 100f;
        }

        private static bool IsActiveHand(Raycaster __instance)
        {
            if (PhysicalMouseDetector.IsPhysicalMovement)
                return false;
            else if (DesktopCursorManager.Instance.GetCurrentInputDevice() != __instance)
            {
                if (__instance.HoveringOverlay != null)
                    if (__instance.HoveringOverlay.IsDesktopOrWindowCapture || XConfig.ActivePointerWebView.Value)
                        return false;
            }

            return true;
        }

        private static bool IsEnable()
        {
            return XConfig.ActivePointerColor.Value;
        }

        private static bool IsHand(Raycaster __instance)
        {
            return __instance.HapticDeviceName != Raycaster.HapticDevice.None;
        }
    }
}
