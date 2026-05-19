using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class EmulateMouseClickAnimation
    {
        [HarmonyPatch(nameof(Raycaster.HandleClicksForDesktopWindows))]
        [HarmonyPostfix]
        public static void PostHandleClicksForDesktopWindows(Raycaster __instance, ref GameObject ___VisualCursorElementClickAnimation, ref Unity_Overlay ___VisualCursorElementClickAnimationOverlay)
        {
            if (!IsEnable()) return;

            ___VisualCursorElementClickAnimation.transform.rotation = Quaternion.LookRotation(___VisualCursorElementClickAnimation.transform.position - Overlay_Manager.Instance.head.position);
            ___VisualCursorElementClickAnimationOverlay.gameObject.SetActive(value: true);
        }

        private static bool IsEnable()
        {
            return XConfig.EmulateMouseClickAnimation.Value;
        }
    }
}
