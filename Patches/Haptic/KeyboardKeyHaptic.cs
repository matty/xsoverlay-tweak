using HarmonyLib;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.Haptic
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class KeyboardKeyHaptic
    {
        private class HoverData
        {
            public GameObject OldHoverObject;
        }
        private static readonly ConditionalWeakTable<Raycaster, HoverData> HoverDictionary = new();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Initialize(Raycaster __instance)
        {
            if (!EventBridge.IsRaycasterHand(__instance)) return;

            HoverDictionary.Add(__instance, new());
        }

        [HarmonyPatch("OnGuiHover")]
        [HarmonyPostfix]
        public static void PlayHapticOnHoverButton(Raycaster __instance, List<RaycastResult> ___PointerResult)
        {
            if (!IsEnable() || !EventBridge.IsRaycasterHand(__instance) || __instance?.HoveringOverlay?.IsPluginApplication == true || __instance.HeldOverlay != null) return;

            if (HoverDictionary.TryGetValue(__instance, out HoverData Data))
            {
                bool IsFound = false;

                foreach (RaycastResult item in ___PointerResult)
                {
                    if (item.gameObject.TryGetComponent(out Button component) && component.interactable)
                    {
                        GameObject HoverObject = component.gameObject;

                        if (Data.OldHoverObject != HoverObject)
                            AdvancedHaptics.Rumble(__instance.HapticDeviceName == Raycaster.HapticDevice.Left, 0.001f, 320f, XConfig.KeyboardKeyHaptic.Value / 100f);

                        IsFound = true;
                        Data.OldHoverObject = HoverObject;

                        break;
                    }
                }

                if (!IsFound)
                    Data.OldHoverObject = null;
            }
        }

        private static bool IsEnable()
        {
            return XConfig.KeyboardKeyHaptic.Value != 0;
        }
    }
}
