using HarmonyLib;
using UnityEngine;

namespace xsoverlay_tweak.Patches
{
    internal class DefaultCaptureOverlayTexture
    {
        private static Texture2D newTexture;

        [HarmonyPatch(typeof(WindowComponentManager), "SetupWindow")]
        [HarmonyPostfix]
        public static void CreateOverlayTexture(ref Unity_Overlay ___ThisOverlay)
        {
            if (!IsEnable()) return;

            newTexture ??= new(___ThisOverlay.renderTexWidthOverride, ___ThisOverlay.renderTexHeightOverride, TextureFormat.RGB24, false);

            ___ThisOverlay.overlayTexture = newTexture;
            ___ThisOverlay.ForceUpdateLatestTextureResult();
        }

        private static bool IsEnable()
        {
            return XConfig.DefaultCaptureOverlayTexture.Value;
        }
    }
}
