using HarmonyLib;
using System.Collections;
using UnityEngine;
using Valve.VR;
using XSOverlay;

namespace xsoverlay_tweak.Patches.Haptic
{
    internal class ToggleEditModeHaptic
    {
        private static Coroutine HapticCoroutine;

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void InitializeEvents()
        {
            XSOEventSystem.OnToggleLayoutMode += (IsShow) =>
            {
                if (!IsEnable()) return;

                if (HapticCoroutine != null)
                    Plugin.Instance.StopCoroutine(HapticCoroutine);

                if (IsShow)
                    HapticCoroutine = Plugin.Instance.StartCoroutine(PlayShowHaptic());
                else
                    HapticCoroutine = Plugin.Instance.StartCoroutine(PlayHideHaptic());
            };
        }

        private static IEnumerator PlayShowHaptic()
        {
            float strength = 100;
            var wait = new WaitForSecondsRealtime(0.01f);

            while (strength < 2000)
            {
                OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, (ushort)strength);
                OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, (ushort)strength);

                yield return wait;

                strength += 200;
            }

            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, 6000);
            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, 6000);

            HapticCoroutine = null;
        }

        private static IEnumerator PlayHideHaptic()
        {
            var wait = new WaitForSecondsRealtime(0.1f);

            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, 6000);
            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, 6000);

            yield return wait;

            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, 3000);
            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, 3000);

            yield return wait;

            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, 1000);
            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, 1000);

            yield return wait;

            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.leftIndex, 0U, 400);
            OpenVR.System.TriggerHapticPulse(OVR_Pose_Handler.instance.rightIndex, 0U, 400);

            HapticCoroutine = null;
        }

        private static bool IsEnable()
        {
            return XConfig.ToggleEditModeHaptic.Value;
        }
    }
}
