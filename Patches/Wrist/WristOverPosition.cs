using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches.Wrist
{
    internal class WristOverPosition
    {
        private static Vector3 position = Vector3.zero;
        private static Quaternion rotation = Quaternion.identity;
        private static Unity_Overlay wrist;

        //** Pre OnMove
        [HarmonyPatch(typeof(Raycaster), "Drop")]
        [HarmonyPrefix]
        public static void PreDrop(Raycaster __instance)
        {
            if (!IsEnable()) return;

            if (__instance.HeldOverlay != null)
                if (__instance.HeldOverlay.IsWristOverlay)
                {
                    wrist = __instance.HeldOverlay;

                    Transform transform = XSettingsManager.Instance.WristDefaultPointLeft;

                    if (Vector3.Distance(__instance.HeldOverlay.transform.position, transform.position) > 0.075f * 3f)
                    {
                        Vector3 vector = __instance.HeldOverlay.transform.position - transform.position;
                        position = transform.transform.position + vector.normalized * 0.075f * 3f;
                    }
                    else
                        position = __instance.HeldOverlay.transform.position;
                }
        }

        //** Post OnMove
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

        //?? Pre OnLoad
        [HarmonyPatch(typeof(XSettingsManager), "LoadWristOffsets")]
        [HarmonyPrefix]
        public static void PreLoadWristOffsets(XSettingsManager __instance)
        {
            if (!IsEnable()) return;

            Transform transform = XSettingsManager.Instance.WristDefaultPointLeft;

            if (Vector3.Distance(__instance.Settings.WristOffsets, transform.position) > 0.075f * 3f)
            {
                Vector3 vector = __instance.Settings.WristOffsets - transform.position;
                position = transform.transform.position + vector.normalized * 0.075f * 3f;
            }
            else
                position = __instance.Settings.WristOffsets;

            rotation = Quaternion.Euler(__instance.Settings.WristRotation); ;
        }

        //?? Post OnLoad
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
