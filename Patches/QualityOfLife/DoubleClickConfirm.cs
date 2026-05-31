using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        [HarmonyPatch(typeof(Raycaster), "HandleClicksForDesktopWindows")]
        [HarmonyPrefix]
        public static bool WaitToConfrimDoubleClick(Raycaster __instance, ref ClickActions clickActions, ref MouseInputDevice ___InputDevice, ref bool ___HadMouseInputDown)
        {
            if (!IsEnable()) return true;

            DoubleClickConfirmState DoubleClickState = InstanceState.GetOrCreateValue(__instance);

            if (___InputDevice.InputSource == clickActions.InputSource)
                if (!___HadMouseInputDown)
                    if (__instance.CanClickDesktopCursor)
                        if (!clickActions.IsHoldingMouseClick)
                        {
                            bool IsDouble = false;
                            float Delay = Time.time - DoubleClickState.lastClickTime;

                            if (Delay <= XSettingsManager.Instance.Settings.DoubleClickDelay * 2)
                            {
                                if (Delay > GetDoubleClickTime() / 1000f)
                                    IsDouble = true;
                                DoubleClickState.lastClickTime = 0f;
                            }
                            else
                                DoubleClickState.lastClickTime = Time.time;

                            if (IsDouble)
                            {
                                switch (clickActions.ActionIndex)
                                {
                                    case 0:
                                        XInputManager.sim.Mouse.LeftButtonDoubleClick();
                                        break;
                                    case 1:
                                        XInputManager.sim.Mouse.RightButtonDoubleClick();
                                        break;
                                    case 2:
                                        MouseOperations.MMouseClick(XInputManager.sim);
                                        MouseOperations.MMouseClick(XInputManager.sim);
                                        break;
                                }

                                return false;
                            }
                        }

            return true;
        }

        private static bool IsEnable()
        {
            return XConfig.DoubleClickConfirm.Value && XSettingsManager.Instance.Settings.InputMethod == InputMethods.EmulateMouse;
        }
    }
}
