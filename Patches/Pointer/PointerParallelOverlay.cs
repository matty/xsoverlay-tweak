using HarmonyLib;
using UnityEngine;
using Valve.VR;
using XSOverlay;
using xsoverlay_tweak.Patches.CommunityRequest;
using xsoverlay_tweak.Patches.Cursor;
using xsoverlay_tweak.Patches.QualityOfLife;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.Pointer
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class PointerParallelOverlay
    {
        [HarmonyPatch("SetVisualCursorTransform")]
        [HarmonyPostfix]
        public static void CursorParallelToCurveoverlay(Raycaster __instance, ref MouseInputDevice ___InputDevice, ref VROverlayIntersectionResults_t rayHitResults, ref GameObject ___VisualCursorElement)
        {
            if (!EventBridge.IsRaycasterHand(__instance)) return;

            if (IsEnable() || WindowsCursorPointer.CursorDictionary.TryGetValue(__instance, out WindowsCursorPointer.CursorData Data) && Data.IsCursor)
            {
                Unity_Overlay overlay = __instance.HoveringOverlay;
                PullTriggerPointerLock.InstanceState.TryGetValue(__instance, out PullTriggerPointerLock.RaycasterState ClickState);

                if (overlay != null && !___InputDevice.ClickFreezeActive && (ClickState == null || !ClickState.IsBlock))
                {
                    Transform transform = overlay.transform;
                    Quaternion rotation = overlay.transform.rotation;

                    if (overlay?.WorldSpaceSceneImpostor != null) // Overlay attached to device
                    {
                        transform = overlay.WorldSpaceSceneImpostor.transform;
                        rotation = overlay.WorldSpaceSceneImpostor.transform.rotation;

                        if (OverlayAttachSmooth.OverlayStatus.TryGetValue(overlay, out var SmoothData)) // Attached device rolling lock
                            if (SmoothData.LockRoll)
                                rotation = SmoothData.Rotation;
                    }

                    if (overlay.overlayCurveRadius.Equals(0)) // Overlay not curve
                    {
                        ___VisualCursorElement.transform.rotation = rotation;
                    }
                    else // Cursor faces up to the overlay curved surface
                    {
                        Vector3 localNormal = new(rayHitResults.vNormal.v0, rayHitResults.vNormal.v1, rayHitResults.vNormal.v2);
                        Vector3 worldNormal = transform.TransformDirection(localNormal);

                        worldNormal.x = -worldNormal.x; // Mirror X in world space to align with Unity's coordinate system for the cursor plate.

                        // Calculate the tilt required to stay parallel to the curved surface at this specific point.
                        Quaternion surfaceTilt = Quaternion.FromToRotation(Vector3.forward, worldNormal);

                        // Apply the surface tilt to the overlay's base world rotation.
                        ___VisualCursorElement.transform.rotation = rotation * surfaceTilt;
                    }

                    // Push B slightly closer to the player's face than A (1 millimeter is enough to win the depth fight)
                    float zBias = 0.001f;
                    ___VisualCursorElement.transform.transform.position += overlay.transform.forward * -zBias; // -Z is forward toward the headset in OpenVR space
                }
            }
        }

        private static bool IsEnable()
        {
            return true;
        }
    }
}
