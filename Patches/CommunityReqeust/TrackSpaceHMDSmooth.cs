using HarmonyLib;
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
        }

        private static readonly ConditionalWeakTable<Unity_Overlay, SmoothPose> _smoothedPoses = new();

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

            if (!_smoothedPoses.TryGetValue(__instance, out var smooth))
            {
                smooth = new SmoothPose();
                _smoothedPoses.Add(__instance, smooth);
            }

            Transform target = __instance.WorldSpaceSceneImpostor.transform;

            if (!smooth.Initialized || __instance.QueuedToForcePositionUpdate)
            {
                smooth.Position = target.position;
                smooth.Rotation = target.rotation;
                smooth.Initialized = true;
            }

            float posDamp = XSettingsManager.Instance.Settings.PositionDampening;
            float rotDamp = XSettingsManager.Instance.Settings.RotationDampening;

            // If grabbed, Raycaster.Grab() is already smoothing the WorldSpaceSceneImpostor towards the ray.
            // Follow the impostor rigidly (high dampening) to avoid double-interpolation during grabs.
            float currentPosDamp = __instance.IsHeld ? posDamp : posDamp / 5f;
            float currentRotDamp = __instance.IsHeld ? rotDamp : rotDamp / 5f;

            smooth.Position = Vector3.Lerp(smooth.Position, target.position, Time.deltaTime * currentPosDamp);
            smooth.Rotation = Quaternion.Slerp(smooth.Rotation, target.rotation, Time.deltaTime * currentRotDamp);

            // Use Absolute tracking to allow software-side smoothing; TrackedDeviceRelative is rigid at driver level.
            __instance.overlay.overlayTransformType = VROverlayTransformType.VROverlayTransform_Absolute;

            // Backup local offset (XSOverlay uses transform.position/rotation as relative offset storage for HMD overlays)
            Vector3 originalPos = __instance.transform.position;
            Quaternion originalRot = __instance.transform.rotation;

            __instance.transform.position = smooth.Position;
            __instance.transform.rotation = smooth.Rotation;

            __instance.SetTransformAbsolute(OVR_Pose_Handler.instance.trackingSpace, __instance.transform);

            // Restore local offset so Raycaster and internal state logic aren't broken by world coordinates
            __instance.transform.position = originalPos;
            __instance.transform.rotation = originalRot;

            /*var optsField = AccessTools.Field(typeof(Unity_Overlay), "opts");
            object optsValue = optsField.GetValue(__instance);
            if (optsValue != null)
            {
                AccessTools.Field(optsValue.GetType(), "pos").SetValue(optsValue, __instance.transform.position);
                AccessTools.Field(optsValue.GetType(), "rot").SetValue(optsValue, __instance.transform.rotation);
                if (optsValue.GetType().IsValueType) optsField.SetValue(__instance, optsValue);
            }*/
        }

        private static bool IsEnable()
        {
            return XConfig.TrackSpaceHMDSmooth.Value;
        }
    }
}