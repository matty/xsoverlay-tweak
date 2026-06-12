using HarmonyLib;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Valve.VR;
using XSOverlay;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class TrackSpaceHMDSmooth
    {
        private class SmoothPose
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public bool Initialized;
            public bool IsMoving;
        }
        private static readonly ConditionalWeakTable<Unity_Overlay, SmoothPose> smoothedPoses = new();

        private static bool IsRecenter = false;
        private static Coroutine RecenterCoroutine;

        private static readonly float oneCentimetre = 0.01f;
        private static readonly float oneDegree = 1.0f;

        [HarmonyPatch(typeof(Unity_Overlay), "UpdateOverlay")]
        [HarmonyPostfix]
        public static void SmoothHMDMovement(Unity_Overlay __instance)
        {
            if (!IsEnable()) return;
            if (__instance.deviceToTrack != Unity_Overlay.OverlayTrackedDevice.HMD) return;
            if (!__instance.isVisible || __instance.IsHidden || __instance.IsPaused) return;

            if (__instance.WorldSpaceSceneImpostor == null)
            {
                __instance.SetUpWorldSpaceAttachedDummy();
                if (__instance.WorldSpaceSceneImpostor == null) return;
            }

            if (!smoothedPoses.TryGetValue(__instance, out var Data))
            {
                Data = new SmoothPose();
                smoothedPoses.Add(__instance, Data);
            }

            Transform target = __instance.WorldSpaceSceneImpostor.transform;

            if (!Data.Initialized || __instance.QueuedToForcePositionUpdate)
            {
                Data.Position = target.position;
                Data.Rotation = target.rotation;
                Data.Initialized = true;
                Data.IsMoving = false;
            }

            Quaternion targetRotation = target.rotation;
            if (XConfig.TrackSpaceHMDLockRoll.Value)
                targetRotation = Quaternion.LookRotation(targetRotation * Vector3.forward, Vector3.up);

            float posDamp = XSettingsManager.Instance.Settings.PositionDampening;
            float rotDamp = XSettingsManager.Instance.Settings.RotationDampening;

            float dist = Vector3.Distance(Data.Position, target.position);
            float angle = Quaternion.Angle(Data.Rotation, targetRotation);

            float distThreshold = oneCentimetre * XConfig.TrackSpaceHMDDistThreshold.Value;
            float angleThreshold = oneDegree * XConfig.TrackSpaceHMDAngleThreshold.Value;
            float stopThreshold = XConfig.TrackSpaceHMDStopThreshold.Value;

            bool isChildOverlay = __instance.overlayName == "window.toolbar" || __instance.overlayName == "window.settings";
            Unity_Overlay parentOverlay = isChildOverlay ? Overlay_Manager.Instance.WindowToolbarMover.ParentOverlay : null;

            // Logic derived from Raycaster.Grab: Determine if we follow the target or wait for threshold.
            if (isChildOverlay && parentOverlay != null && smoothedPoses.TryGetValue(parentOverlay, out var parentData))
                Data.IsMoving = parentData.IsMoving;  // Inherit movement state from parent to keep UI elements synced.
            else if (__instance.IsHeld || parentOverlay?.IsHeld == true)
                Data.IsMoving = true;
            else if (dist > distThreshold || angle > angleThreshold)
                Data.IsMoving = true;
            else if (dist < (oneCentimetre * stopThreshold) && angle < (oneDegree * stopThreshold))
                Data.IsMoving = false;

            if (Data.IsMoving || IsRecenter)
            {
                // If the window is being held, Grab() is already applying smoothing. 
                // Rigidly (high dampening) to avoid "lagging" behind the hand.
                // Otherwise, we use a reduced dampening factor to provide a stable, "lazy" follow effect for HMD movement.
                bool isHeld = __instance.IsHeld || parentOverlay?.IsHeld == true;
                float dampMultiplier = isHeld ? 1f : 0.2f;

                // Use Slerp for position to maintain spherical consistency around the head.
                // This prevents the "vibrating" or linear shortcutting feel during fast HMD rotations.
                Data.Position = Vector3.Slerp(Data.Position, target.position, Time.deltaTime * posDamp * dampMultiplier);
                Data.Rotation = Quaternion.Slerp(Data.Rotation, targetRotation, Time.deltaTime * rotDamp * dampMultiplier);
            }

            // Use Absolute tracking to allow software-side smoothing; TrackedDeviceRelative is rigid at driver level.
            __instance.overlay.overlayTransformType = VROverlayTransformType.VROverlayTransform_Absolute;

            // Backup local offset (XSOverlay uses transform.position/rotation as relative offset storage for HMD overlays)
            Vector3 originalPos = __instance.transform.position;
            Quaternion originalRot = __instance.transform.rotation;

            __instance.transform.position = Data.Position;
            __instance.transform.rotation = Data.Rotation;

            __instance.SetTransformAbsolute(OVR_Pose_Handler.instance.trackingSpace, __instance.transform);

            // Restore local offset so Raycaster and internal state logic aren't broken by world coordinates
            if (!__instance.IsHeld)
            {
                __instance.transform.position = originalPos;
                __instance.transform.rotation = originalRot;
            }
        }

        [HarmonyPatch(typeof(Overlay_Manager), nameof(Overlay_Manager.RecieveRecenterWindows), []), HarmonyPatch(typeof(Overlay_Manager), nameof(Overlay_Manager.RecieveRecenterWindows), [typeof(HandSource)])]
        [HarmonyPostfix]
        public static void ListenForRecenter()
        {
            if (!IsEnable()) return;

            IsRecenter = true;

            if (RecenterCoroutine != null)
                Plugin.Instance.StopCoroutine(RecenterCoroutine);

            RecenterCoroutine = Plugin.Instance.StartCoroutine(StopRecenter());
        }

        private static IEnumerator StopRecenter()
        {
            yield return new WaitForSecondsRealtime(1f);

            IsRecenter = false;
            RecenterCoroutine = null;
        }

        private static bool IsEnable()
        {
            return XConfig.TrackSpaceHMDSmooth.Value;
        }
    }
}