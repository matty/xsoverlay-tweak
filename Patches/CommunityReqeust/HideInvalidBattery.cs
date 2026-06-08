using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using XSOverlay.Websockets.API;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class HideInvalidBattery
    {
        [HarmonyPatch(typeof(ApiHandler), "OnRequestDeviceInformation")]
        [HarmonyPrefix]
        public static bool UpdateDevices(ApiHandler __instance, string sender, string data)
        {
            if (!IsEnable()) return true;

            Dictionary<uint, DeviceManager.Device> Devices = DeviceManager.Instance.Devices;

            foreach (KeyValuePair<uint, DeviceManager.Device> item in Devices)
                if (item.Value.battery == 0 || !item.Value.isSupported)
                    Devices.Remove(item.Key);

            IOrderedEnumerable<DeviceManager.Device> orderedEnumerable = from x in Devices.Values.ToList<DeviceManager.Device>()
                                                                         orderby x.classification, x.id == OVR_Pose_Handler.instance.leftIndex descending, x.id
                                                                         select x;

            __instance.SendMessage("UpdateDeviceInformation", JsonConvert.SerializeObject(orderedEnumerable), null, sender);

            return false;
        }

        public static bool IsEnable()
        {
            return XConfig.HideInvalidBattery.Value;
        }
    }
}
