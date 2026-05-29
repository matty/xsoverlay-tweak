using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches.QualityOfLife
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class DoubleClickConfirm
    {
        public class DoubleClickConfirmState
        {
            public float lastClickTime = 0f;
        }
        public static readonly ConditionalWeakTable<Raycaster, DoubleClickConfirmState> InstanceState = new();

        [HarmonyPatch(typeof(Raycaster), "HandleClicksForDesktopWindows")]
        [HarmonyPostfix]
        public static void WaitToConfrimDoubleClick(Raycaster __instance, ref ClickActions clickActions, ref MouseInputDevice ___InputDevice, ref bool ___HadMouseInputDown)
        {
            if (!IsEnable()) return;

            DoubleClickConfirmState DoubleClickState = InstanceState.GetOrCreateValue(__instance);

            if (___InputDevice.InputSource == clickActions.InputSource)
                if (!___HadMouseInputDown)
                    if (__instance.CanClickDesktopCursor)
                        if (!clickActions.IsHoldingMouseClick)
                        {
                            bool IsDouble = false;
                            if (Time.time - DoubleClickState.lastClickTime <= XSettingsManager.Instance.Settings.DoubleClickDelay * 1.5)
                            {
                                IsDouble = true;
                                DoubleClickState.lastClickTime = 0f;
                            }
                            else
                                DoubleClickState.lastClickTime = Time.time;

                            switch (clickActions.ActionIndex)
                            {
                                case 0:
                                    if (IsDouble)
                                        MouseOperations.LMouseClick(XInputManager.sim);
                                    break;
                                case 1:
                                    if (IsDouble)
                                        MouseOperations.RMouseClick(XInputManager.sim);
                                    break;
                                case 2:
                                    if (IsDouble)
                                        MouseOperations.MMouseClick(XInputManager.sim);
                                    break;
                            }
                        }
        }

        private static bool IsEnable()
        {
            return XConfig.DoubleClickConfirm.Value && XSettingsManager.Instance.Settings.InputMethod == InputMethods.EmulateMouse;
        }
    }
}
