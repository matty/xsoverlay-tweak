using HarmonyLib;
using System.Reflection;
using UnityEngine;
using XSOverlay;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches
{
    internal class RefreshRate
    {
        private static readonly MethodInfo GetHMDRefreshRate = AccessTools.Method(typeof(DeviceManager), "GetHMDRefreshRate");

        private static int HMDRefreshRate = 90;
        private static float LastGrabTime;
        private static float GrabbedDistance = 0f;

        [HarmonyPatch(typeof(DeviceManager), "Start")]
        [HarmonyPostfix]
        public static void Start(DeviceManager __instance)
        {
            // Listen to refresh rate change
            XConfig.RefreshRate.SettingChanged += (sender, args) =>
            {
                if (IsEnable())
                    GetHMDRefreshRate.Invoke(__instance, null);
            };

            // Listen to edit mode change
            XSOEventSystem.OnToggleLayoutMode += (isEditMode) =>
            {
                if (IsEnable())
                    if (XConfig.OnlyInLayoutMod.Value)
                        GetHMDRefreshRate.Invoke(__instance, null);
            };

            // Listen to hovering overlay change
            {
                XSOEventSystem.OnSwitchHoveringOverlay += (raycaster, overlay) =>
                {
                    EventBridge.IsHoverAnyOverlay = true;

                    if (IsEnable())
                        if (XConfig.OnlyHoverOverlay.Value)
                            GetHMDRefreshRate.Invoke(__instance, null);

                    if (IsEfficiencyModeEnable())
                        GetHMDRefreshRate.Invoke(__instance, null);
                };

                XSOEventSystem.OnReleaseControlOfDesktopCursor += (raycaster) =>
                {
                    EventBridge.IsHoverAnyOverlay = false;

                    if (IsEnable())
                        if (XConfig.OnlyHoverOverlay.Value)
                            GetHMDRefreshRate.Invoke(__instance, null);

                    if (IsEfficiencyModeEnable())
                        GetHMDRefreshRate.Invoke(__instance, null);
                };
            }
        }

        [HarmonyPatch(typeof(DeviceManager), "GetHMDRefreshRate")]
        [HarmonyPrefix]
        public static bool PatchHMDRefreshRate(DeviceManager __instance, ref Unity_SteamVR_Handler ___svr, ref bool ___HMDRefreshRateDetermined, ref int ___OldRefreshRate, ref int ___HMDRefreshRate)
        {
            if (___svr.isSteamVRConnected)
            {
                // Original
                {
                    ___HMDRefreshRate = __instance.FetchHMDRefreshRate();
                    HMDRefreshRate = ___HMDRefreshRate;

                    if (!___HMDRefreshRateDetermined)
                    {
                        XSTools.log("Detected Headset Refresh Rate: " + ___HMDRefreshRate);
                        ___HMDRefreshRateDetermined = true;
                    }

                    if (HMDRefreshRate != ___OldRefreshRate)
                    {
                        ___HMDRefreshRateDetermined = false;
                    }

                    ___OldRefreshRate = ___HMDRefreshRate;
                }

                // Modify
                {
                    int targetFrameRate = HMDRefreshRate;

                    if (IsEnable() && IsOnlyHoverOverlay() && IsOnlyInLayoutMode())
                        targetFrameRate = XConfig.RefreshRate.Value.Equals(500) ? -1 : XConfig.RefreshRate.Value;

                    if (EfficiencyMode.IsInEfficiencyMode)
                        targetFrameRate = 15;

                    XSTools.ExecuteOnMainThread(delegate
                    {
                        Application.targetFrameRate = targetFrameRate;
                    });
                }
            }

            return false;
        }

        // Fix Push/Pull speed
        [HarmonyPatch(typeof(Raycaster), "Grab")]
        [HarmonyPostfix]
        public static void Grab(ref float ___GrabbedDistance)
        {
            if (!IsEnable()) return;

            float currentTime = Time.unscaledTime;
            if (currentTime - LastGrabTime < (1f / HMDRefreshRate))
                ___GrabbedDistance = GrabbedDistance;
            else
            {
                GrabbedDistance = ___GrabbedDistance;
                LastGrabTime = currentTime;
            }
        }

        // Fix Scrolling speed
        [HarmonyPatch(typeof(Raycaster), "HandleScrolling")]
        [HarmonyPrefix]
        public static bool HandleScrolling(Raycaster __instance, ref MouseInputDevice ___InputDevice, ref int ___ScrollClicksPerSecond, ref float ____tickAccumulator, ref Vector2 ___CursorUVNormalized)
        {
            if (!IsEnable()) return true;

            float currentFPS = 1.0f / Time.unscaledDeltaTime;
            float scrollSpeedMultiplier = XSettingsManager.Instance.Settings.ScrollSpeed * (currentFPS / HMDRefreshRate);

            float num = 0.2f;
            float y = ___InputDevice.NormalizedScrollAxis.y;
            float num2 = Mathf.Abs(y);
            if (num2 <= num || (float)___ScrollClicksPerSecond <= 0f)
            {
                return false;
            }

            if (__instance.HoveringOverlay.IsDesktopOrWindowCapture)
            {
                ____tickAccumulator += num2 * (float)___ScrollClicksPerSecond * scrollSpeedMultiplier * Time.unscaledDeltaTime;
                int num3 = (int)____tickAccumulator;
                if (num3 > 0)
                {
                    ____tickAccumulator -= num3;
                    MouseOperations.Scroll(((y > 0f) ? 1 : (-1)) * num3, XInputManager.sim);
                }
            }
            else if (__instance.HoveringOverlay.IsPluginApplication)
            {
                float num4 = y * scrollSpeedMultiplier * Time.unscaledDeltaTime;
                __instance.HoveringOverlay.WebViewHandler.WebView.Scroll(new Vector2(0f, 0f - num4), ___CursorUVNormalized);
            }

            return false;
        }

        private static bool IsEnable()
        {
            return XConfig.RefreshRate.Value != DeviceManager.Instance.HMDRefreshRate;
        }

        private static bool IsOnlyHoverOverlay()
        {
            return !XConfig.OnlyHoverOverlay.Value || EventBridge.IsHoverAnyOverlay;
        }

        private static bool IsOnlyInLayoutMode()
        {
            return !XConfig.OnlyInLayoutMod.Value || Overlay_Manager.Instance.editMode;
        }

        private static bool IsEfficiencyModeEnable()
        {
            return XConfig.EfficiencyMode.Value;
        }
    }
}
