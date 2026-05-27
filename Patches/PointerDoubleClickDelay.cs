using HarmonyLib;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class PointerDoubleClickDelay
    {
        [HarmonyPatch("SetVisualCursorTransform")]
        [HarmonyPrefix]
        public static bool BlockPointerMovement(ref MouseInputDevice ___InputDevice)
        {
            if (!IsEnable()) return true;

            return !___InputDevice.ClickFreezeActive;
        }

        public static bool IsEnable()
        {
            return XConfig.PointerDoubleClickDelay.Value;
        }
    }
}