using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    internal class OverlayRollFlickerFix
    {
        private class RollState { public float lastX; }
        private static readonly ConditionalWeakTable<Unity_Overlay, RollState> LastX = new();

        [HarmonyPatch(typeof(WindowMovementManager), nameof(WindowMovementManager.HandleWindowRollAndRotation))]
        [HarmonyPostfix]
        public static void MoveDownFromAboveFlicker(ref Transform overlayTransform, ref Unity_Overlay overlayToPoint)
        {
            if (!IsEnable()) return;

            RollState state = LastX.GetOrCreateValue(overlayToPoint);
            Quaternion rotation = overlayTransform.rotation;

            if (rotation.x < 0.001f && rotation.x > -0.2f)
                if (state.lastX < rotation.x)
                    overlayTransform.rotation = new(0f, rotation.y, rotation.z, rotation.w);

            state.lastX = overlayTransform.rotation.x;
        }

        [HarmonyPatch(typeof(WindowMovementManager), nameof(WindowMovementManager.DetermineIfOverlayShouldBeCurved))]
        [HarmonyPostfix]
        public static void CurveApplyFlicker(ref Unity_Overlay overlay)
        {
            if (!IsEnable()) return;

            if (!overlay.IsHeld)
            {
                Quaternion rotation = overlay.transform.rotation;

                if (rotation.x < 0f && rotation.x > -0.2f)
                    overlay.transform.rotation = new(0f, rotation.y, rotation.z, rotation.w);
            }
        }

        private static bool IsEnable()
        {
            return XConfig.OverlayRollFlickerFix.Value;
        }
    }
}
