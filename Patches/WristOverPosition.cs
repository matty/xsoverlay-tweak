using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    internal class WristOverPosition
    {
        private static Vector3 position = Vector3.zero;
        private static Quaternion rotation = Quaternion.identity;
        private static Unity_Overlay wrist;

        // On move
        [HarmonyPatch(typeof(Raycaster), "Drop")]
        [HarmonyPrefix]
        public static bool PreDrop(Raycaster __instance)
        {
            if (!IsEnable()) return true;

            if (__instance.HeldOverlay != null)
                if (__instance.HeldOverlay.IsWristOverlay)
                {
                    wrist = __instance.HeldOverlay;
                    position = __instance.HeldOverlay.transform.position;
                }

            return true;
        }

        [HarmonyPatch(typeof(Raycaster), "Drop")]
        [HarmonyPostfix]
        public static void PostDrop(Raycaster __instance)
        {
            if (!IsEnable()) return;

            if (wrist != null)
            {
                wrist.transform.position = position;
                wrist.WorldSpaceSceneImpostor.transform.localPosition = position;
                wrist = null;
            }
        }

        // On load
        [HarmonyPatch(typeof(XSettingsManager), "LoadWristOffsets")]
        [HarmonyPrefix]
        public static bool PreLoadWristOffsets(XSettingsManager __instance)
        {
            if (!IsEnable()) return true;

            position = __instance.Settings.WristOffsets;
            rotation = Quaternion.Euler(__instance.Settings.WristRotation); ;

            return true;
        }

        [HarmonyPatch(typeof(XSettingsManager), "LoadWristOffsets")]
        [HarmonyPostfix]
        public static void PostDLoadWristOffsets(XSettingsManager __instance)
        {
            if (!IsEnable()) return;

            Unity_Overlay wrist = (Unity_Overlay)AccessTools.Field(typeof(XSettingsManager), "SVR_WristOverlay").GetValue(__instance);

            wrist.transform.position = position;
            wrist.transform.rotation = rotation;
            wrist.transform.localPosition = position;
            wrist.transform.localRotation = rotation;
        }

        private static bool IsEnable()
        {
            return XConfig.WristOverPosition.Value;
        }
    }
}
