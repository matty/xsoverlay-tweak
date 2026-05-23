using HarmonyLib;

namespace xsoverlay_tweak.Patches
{
    [HarmonyPatch(typeof(WindowComponentManager))]
    internal class AlwaysHideCursor
    {
        // Cache the private field once for high-speed access
        private static readonly AccessTools.FieldRef<WindowComponentManager, bool> WindowCursorRef = AccessTools.FieldRefAccess<WindowComponentManager, bool>("WindowCanShowDesktopCursor");

        [HarmonyPatch("OnSwitchHoveringOverlay")]
        [HarmonyPostfix]
        public static void CheckShowCursorPrefix(WindowComponentManager __instance)
        {
            if (!IsEnable()) return;

            // Set the private boolean to false every frame before the original method checks it
            WindowCursorRef(__instance) = false;
        }

        private static bool IsEnable()
        {
            return XConfig.AlwaysHideCursor.Value || XConfig.WindowsCursorPointer.Value;
        }
    }
}