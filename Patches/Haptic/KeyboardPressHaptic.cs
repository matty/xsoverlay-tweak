using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using XSOverlay;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.Haptic
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class KeyboardPressHaptic : MonoBehaviour
    {
        [HarmonyPatch("OnGuiPressDown")]
        [HarmonyPostfix]
        public static void PlayHapticOnPressButton(Raycaster __instance, SteamVR_Input_Sources inputSource, ref List<RaycastResult> ___PointerResult, MouseInputDevice ___InputDevice)
        {
            if (!IsEnable() || !IsHand(__instance)) return;
            if (___PointerResult == null || inputSource != ___InputDevice.InputSource || __instance.HoveringOverlay == null || __instance.HoveringOverlay.IsPluginApplication || ___InputDevice.ScaleGracePeriodActive)
                return;

            for (int i = 0; i < ___PointerResult.Count; i++)
            {
                bool ShouldHaptic = false;
                HapticButton hapticButton = ___PointerResult[i].gameObject.GetComponent<HapticButton>();

                if (hapticButton != null)
                {
                    GameObject.Destroy(hapticButton);
                    ___PointerResult[i].gameObject.AddComponent<KeyboardPressHaptic>();
                    ShouldHaptic = true;
                }
                else if (___PointerResult[i].gameObject.GetComponent<KeyboardPressHaptic>() != null)
                    ShouldHaptic = true;

                if (ShouldHaptic)
                {
                    AdvancedHaptics.Rumble(__instance.HapticDeviceName == Raycaster.HapticDevice.Left, 0.001f, 40, XConfig.KeyboardPressHaptic.Value / 100f);

                    break;
                }
            }

            return;
        }

        private static bool IsHand(Raycaster instance)
        {
            return instance.HapticDeviceName != Raycaster.HapticDevice.None;
        }

        private static bool IsEnable()
        {
            return XConfig.KeyboardPressHaptic.Value != 0;
        }
    }
}
