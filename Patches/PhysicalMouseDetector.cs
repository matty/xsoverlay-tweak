using HarmonyLib;
using System;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches
{
    internal class PhysicalMouseDetector
    {
        public static bool IsPhysicalMovement = false;
        private static MouseInputDetector mouseDetector;

        private static readonly Action<Raycaster> TakeControlOverCursorIfNotInControlDelegate = AccessTools.MethodDelegate<Action<Raycaster>>(AccessTools.Method(typeof(Raycaster), "TakeControlOverCursorIfNotInControl"));

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void InitializeEvents()
        {
            mouseDetector = new MouseInputDetector();
            mouseDetector.PhysicalMouseMoved += (x, y) =>
            {
                if (IsEnable())
                    IsPhysicalMovement = true;
            };
        }

        [HarmonyPatch(typeof(Raycaster), "HandleClicksForDesktopWindows")]
        [HarmonyPrefix]
        public static bool ClickToRegainControl(Raycaster __instance)
        {
            if (IsPhysicalMovement)
            {
                IsPhysicalMovement = false;
                TakeControlOverCursorIfNotInControlDelegate(__instance);

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForDesktopWindows")]
        [HarmonyPrefix]
        public static bool TouchToRegainControl(Raycaster __instance)
        {
            if (IsPhysicalMovement)
            {
                IsPhysicalMovement = false;
                TakeControlOverCursorIfNotInControlDelegate(__instance);

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPrefix]
        public static bool TouchWebViewToRegainControl(Raycaster __instance)
        {
            if (IsPhysicalMovement)
            {
                IsPhysicalMovement = false;
                TakeControlOverCursorIfNotInControlDelegate(__instance);

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Raycaster), "SyncedOverlayUpdate")]
        [HarmonyPrefix]
        public static bool BlockSeningNewCursorPostion()
        {
            return !IsPhysicalMovement;
        }

        private static bool IsEnable()
        {
            return XConfig.PhysicalMouseDetector.Value;
        }
    }
}
