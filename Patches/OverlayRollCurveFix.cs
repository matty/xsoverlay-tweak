using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    internal class OverlayRollCurveFix
    {
        private class RollState { public float LastRotation; }
        private static readonly ConditionalWeakTable<Unity_Overlay, RollState> LastX = new();

        [HarmonyPatch(typeof(WindowMovementManager), nameof(WindowMovementManager.DetermineIfOverlayShouldBeCurved))]
        [HarmonyPrefix]
        public static bool CurveApplyFlicker(ref Unity_Overlay overlay)
        {
            if (!IsEnable()) return true;

            RollState state = LastX.GetOrCreateValue(overlay);
            Quaternion rotation = overlay.transform.rotation;

            if (rotation.eulerAngles.x > 335f) // Bad angle
                if (rotation.eulerAngles.x > state.LastRotation) // Moving down
                    return false;

            state.LastRotation = rotation.eulerAngles.x;

            return true;
        }

        private static bool IsEnable()
        {
            return XConfig.OverlayRollCurveFix.Value && XSettingsManager.Instance.Settings.CurvedOverlays;
        }
    }
}
