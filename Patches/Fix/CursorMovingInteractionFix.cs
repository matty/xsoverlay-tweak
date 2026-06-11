using HarmonyLib;
using System.Runtime.InteropServices;
using XSOverlay;

namespace xsoverlay_tweak.Patches.Fix
{
    internal class CursorMovingInteractionFix
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_MOVE = 0x0001;

        [HarmonyPatch(typeof(MouseOperations), nameof(MouseOperations.SetCursorPosition))]
        [HarmonyPostfix]
        public static void SetCursorPosition()
        {
            if (!IsEnable()) return;

            mouse_event(MOUSEEVENTF_MOVE, 1, 1, 0, 0);
        }

        private static bool IsEnable()
        {
            return XConfig.CursorMovingInteractionFix.Value;
        }
    }
}