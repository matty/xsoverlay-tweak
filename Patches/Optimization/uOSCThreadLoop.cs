using HarmonyLib;
using System;
using UnityEngine;
using uOSC;

namespace xsoverlay_tweak.Patches.Optimization
{
    internal class uOSCThreadLoop
    {
        private static Action loopFunc_;

        [HarmonyPatch(typeof(uOscClient), nameof(uOscClient.Send), [typeof(string), typeof(object[])]), HarmonyPatch(typeof(uOscClient), nameof(uOscClient.Send), [typeof(Message)])]
        [HarmonyPostfix]
        public static void Send()
        {
            try
            {
                loopFunc_();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError(ex.StackTrace);
            }
        }

        [HarmonyPatch(typeof(uOSC.DotNet.Thread), "ThreadLoop")]
        [HarmonyPrefix]
        public static bool ThreadLoop(uOSC.DotNet.Thread __instance, bool ___isRunning_, Action ___loopFunc_)
        {
            loopFunc_ = ___loopFunc_;
            return false;
        }
    }
}
