using HarmonyLib;
using System;
using UnityEngine;
using Valve.VR;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.Fix
{
    internal class SteamVR_BetaFix
    {
        private static readonly Version SteamVR_TargetVersion = new(2, 16);
        private static bool IsOverlayClipping = false;

        /// <summary>
        /// Push Pointer slightly closer to the player's face than Hover Overlay
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="___VisualCursorElement"></param>
        /// <param name="___VisualCursorElementClickAnimation"></param>
        [HarmonyPatch(typeof(Raycaster), "SetVisualCursorTransform")]
        [HarmonyPostfix]
        public static void FixPointerClipping(
            Raycaster __instance,
            ref GameObject ___VisualCursorElement,
            ref Vector3 ___CurrentRayPosition,
            ref Vector3 ___RayHitPoint,
            ref Vector3 ___CurrentRayDirection)
        {
            if (!IsEnable() || !IsOverlayClipping || !EventBridge.IsRaycasterHand(__instance)) return;

            // Push back hit point 3mm
            Vector3 position = (___RayHitPoint = (___CurrentRayPosition + ___CurrentRayDirection * __instance.FinalSteamVRRaycastResults.fDistance) - (___CurrentRayDirection * 0.003f));
            ___VisualCursorElement.transform.position = position;
        }

        [HarmonyPatch(typeof(OpenVR), nameof(OpenVR.Init))]
        [HarmonyPostfix]
        public static void SteamVRConnected(CVRSystem __result, ref EVRInitError peError)
        {
            if (peError == EVRInitError.None && __result != null)
                if (Version.TryParse(__result.GetRuntimeVersion(), out Version currentVersion))
                    IsOverlayClipping = currentVersion > SteamVR_TargetVersion;
        }

        private static bool IsEnable()
        {
            return true;
        }
    }
}
