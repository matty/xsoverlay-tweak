using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using XSOverlay;
using XSOverlay.WebApp;

namespace xsoverlay_tweak.Patches.CommunityReqeust
{
    internal class LoadLayoutKeyboard
    {
        [HarmonyPatch(typeof(LayoutHandler), "SaveLayout", [])]
        [HarmonyPostfix]
        public static void SaveKeyboardToLayout(LayoutHandler __instance, string ___LayoutAssetPath)
        {
            if (Overlay_Manager.Instance.Keyboard.activeSelf)
            {
                string text = ___LayoutAssetPath + "/Layout_" + __instance.SelectedLayout.ToString() + ".json";
                if (Application.isEditor)
                    text = ___LayoutAssetPath + "/Editor_Layout_" + __instance.SelectedLayout.ToString() + ".json";

                if (!File.Exists(text)) return;

                JObject root = JObject.Parse(File.ReadAllText(text));
                Unity_Overlay keyboard = Overlay_Manager.Instance.Keyboard_Overlay;

                root["keyboard"] = new JObject
                {
                    ["position"] = new JArray(keyboard.transform.localPosition.x, keyboard.transform.localPosition.y, keyboard.transform.localPosition.z),
                    ["rotation"] = new JArray(keyboard.transform.localRotation.x, keyboard.transform.localRotation.y, keyboard.transform.localRotation.z, keyboard.transform.localRotation.w)
                };

                File.WriteAllText(text, root.ToString(Formatting.Indented));
            }
        }

        [HarmonyPatch(typeof(LayoutHandler), "LoadLayout", [])]
        [HarmonyPostfix]
        public static void LoadKeyboardFromLayout(LayoutHandler __instance, string ___LayoutAssetPath)
        {
            string text = ___LayoutAssetPath + "/Layout_" + __instance.SelectedLayout.ToString() + ".json";
            if (Application.isEditor)
                text = ___LayoutAssetPath + "/Editor_Layout_" + __instance.SelectedLayout.ToString() + ".json";

            if (!File.Exists(text)) return;

            JObject root = JObject.Parse(File.ReadAllText(text));
            JToken keyboardData = root["keyboard"];

            if (keyboardData == null)
            {
                if (Overlay_Manager.Instance.Keyboard.activeSelf)
                    ServerClientBridge.Instance.Api.Commands["Keyboard"]("", "", "");
            }
            else
            {
                if (!Overlay_Manager.Instance.Keyboard.activeSelf)
                    ServerClientBridge.Instance.Api.Commands["Keyboard"]("", "", "");

                Task.Run(async () =>
                {
                    await Task.Delay(50);

                    Unity_Overlay keyboard = Overlay_Manager.Instance.Keyboard_Overlay;

                    if (keyboardData["position"] is JArray pos && pos.Count == 3)
                        keyboard.transform.localPosition = new Vector3((float)pos[0], (float)pos[1], (float)pos[2]);

                    if (keyboardData["rotation"] is JArray rot && rot.Count == 4)
                        keyboard.transform.localRotation = new Quaternion((float)rot[0], (float)rot[1], (float)rot[2], (float)rot[3]);
                });
            }
        }
    }
}
