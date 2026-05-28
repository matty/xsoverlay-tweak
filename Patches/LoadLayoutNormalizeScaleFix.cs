using HarmonyLib;
using System.Runtime.CompilerServices;

namespace xsoverlay_tweak.Patches
{
    internal class LoadLayoutNormalizeScaleFix
    {
        private class OverlayData
        {
            public float LayoutWidthInMeters;
        }

        private static readonly ConditionalWeakTable<Unity_Overlay, OverlayData> OverlayDictionary = new();

        [HarmonyPatch(typeof(WindowComponentManager), "SetupWindow")]
        [HarmonyPostfix]
        public static void CreateOverlayTexture(ref Unity_Overlay ___ThisOverlay)
        {
            if (!IsEnable()) return;

            OverlayDictionary.Add(___ThisOverlay, new OverlayData { LayoutWidthInMeters = ___ThisOverlay.LayoutWidthInMeters });
        }

        [HarmonyPatch(typeof(WindowComponentManager), "SetNormalizedScale")]
        [HarmonyPostfix]
        public static void NormalizeScaleFix(ref Unity_Overlay ___ThisOverlay)
        {
            if (!IsEnable()) return;

            if (OverlayDictionary.TryGetValue(___ThisOverlay, out OverlayData Data))
            {
                ___ThisOverlay.widthInMeters = Data.LayoutWidthInMeters;
                ___ThisOverlay.overlay.overlayWidthInMeters = Data.LayoutWidthInMeters;

                OverlayDictionary.Remove(___ThisOverlay);
            }
        }

        private static bool IsEnable()
        {
            return XConfig.LoadLayoutScaleFix.Value;
        }
    }
}
