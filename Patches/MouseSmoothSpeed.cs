using HarmonyLib;
using System.Collections.Generic;

namespace xsoverlay_tweak.Patches
{
    internal class MouseSmoothSpeed
    {
        private static List<Raycaster> Instances = [];

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void SettingChangingListener()
        {
            XConfig.MouseSmoothSpeed.SettingChanged += (sender, args) =>
            {
                foreach (var __instance in Instances)
                    AccessTools.Field(typeof(Raycaster), "InterpolationSpeed").SetValue(__instance, XConfig.MouseSmoothSpeed.Value);
            };
        }

        [HarmonyPatch(typeof(Raycaster), "Start")]
        [HarmonyPostfix]
        public static void ApplyMouseSmoothSpeed(Raycaster __instance)
        {
            Instances.Add(__instance);
            AccessTools.Field(typeof(Raycaster), "InterpolationSpeed").SetValue(__instance, XConfig.MouseSmoothSpeed.Value);
        }
    }
}
