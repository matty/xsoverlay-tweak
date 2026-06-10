using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using uWindowCapture;
using XSOverlay;
using xsoverlay_tweak.Utils;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class WindowToolbarGesture
    {
        private class WindowData
        {
            public UwcWindow LastWindow;
            public bool IsChanged = false;
            public int ScrollIndex = 0;
        }
        private static readonly ConditionalWeakTable<Unity_Overlay, WindowData> LastWindow = new();

        [HarmonyPatch(typeof(UpdateDateTime), "Awake")]
        [HarmonyPostfix]
        public static void ScrollWindowToolbarToChangeWindow()
        {
            EventBridge.OnHandleScrolling += (ScrollAxis, normalizedPoint) =>
            {
                Unity_Overlay targetOverlay = Overlay_Manager.Instance.WindowToolbarMover.ParentOverlay;

                if (LastWindow.TryGetValue(targetOverlay, out WindowData Data))
                {
                    if (Math.Abs(ScrollAxis.y) > 0.3f)
                    {
                        if (!Data.IsChanged)
                        {
                            Data.IsChanged = true;

                            var currentWindowList = UwcWindowList.Instance.CurrentWindowList;
                            int maxWindow = currentWindowList.Count;

                            if (maxWindow == 0) return;

                            if (ScrollAxis.y < 0)
                                Data.ScrollIndex--;
                            else
                                Data.ScrollIndex++;

                            if (Data.ScrollIndex >= maxWindow)
                                Data.ScrollIndex = 0;
                            else if (Data.ScrollIndex < 0)
                                Data.ScrollIndex = maxWindow - 1;

                            var targetItem = System.Linq.Enumerable.ElementAtOrDefault(currentWindowList.Values, Data.ScrollIndex);
                            if (targetItem == null) return;

                            int windowTargetId = targetItem.Value.id;

                            WindowComponentManager windowComponent = targetOverlay.overlayRootObject.GetComponentInChildren<WindowComponentManager>(true);
                            windowComponent?.SetOverlayCaptureTarget(targetOverlay, windowTargetId);
                        }
                    }
                    else if (Data.IsChanged)
                        Data.IsChanged = false;
                }
            };
        }

        [HarmonyPatch(typeof(Raycaster), "HandleTouchInputForWebApplications")]
        [HarmonyPrefix]
        public static bool RightClickWindowToolbar(Raycaster __instance, ClickActions clickActions)
        {
            if (EventBridge.CurrentHoveringOverlay?.overlayName == "window.toolbar")
                if (EventBridge.IsActiveHand(__instance) && clickActions.ActionIndex == 1)
                {
                    Unity_Overlay targetOverlay = Overlay_Manager.Instance.WindowToolbarMover.ParentOverlay;

                    if (LastWindow.TryGetValue(targetOverlay, out WindowData Data))
                    {
                        WindowComponentManager windowComponent = targetOverlay.overlayRootObject.GetComponentInChildren<WindowComponentManager>(true);
                        windowComponent.SetOverlayCaptureTarget(targetOverlay, Data.LastWindow);

                        return false;
                    }
                }

            return true;
        }

        [HarmonyPatch(typeof(WindowComponentManager), nameof(WindowComponentManager.SetOverlayCaptureTarget), [typeof(Unity_Overlay), typeof(int)])]
        [HarmonyPrefix]
        public static bool RememberCaptureTarget(WindowComponentManager __instance, Unity_Overlay overlay, int windowTargetId)
        {
            if (LastWindow.TryGetValue(overlay, out WindowData Data))
                Data.LastWindow = __instance.WindowAPI.window;
            else
                LastWindow.Add(overlay, new WindowData { LastWindow = __instance.WindowAPI.window });

            return true;
        }

        [HarmonyPatch(typeof(WindowComponentManager), nameof(WindowComponentManager.SetOverlayCaptureTarget), [typeof(Unity_Overlay), typeof(UwcWindow)])]
        [HarmonyPrefix]
        public static bool RememberCaptureTarget(WindowComponentManager __instance, Unity_Overlay overlay, UwcWindow window)
        {
            if (LastWindow.TryGetValue(overlay, out WindowData Data))
                Data.LastWindow = __instance.WindowAPI.window;
            else
                LastWindow.Add(overlay, new WindowData { LastWindow = __instance.WindowAPI.window });

            return true;
        }
    }
}
