using HarmonyLib;
using System.IO;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Utils
{
    internal class CutomSettings
    {
        public static XSettings Settings = new();

        public bool IsPerformanceMonitorOpened = false;
        public bool IsMediaPlayerOpened = false;

        public static void SaveSettings()
        {
            string filePath = Application.persistentDataPath + "/config/";

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            string contents = JsonUtility.ToJson(Settings, prettyPrint: true);
            File.WriteAllText(System.IO.Path.Combine(filePath, "tweakSettings.json"), contents);
        }

        [HarmonyPatch(typeof(XSettingsManager), "LoadSettings")]
        [HarmonyPostfix]
        public static void LoadWristStateSettings()
        {
            string filePath = Application.persistentDataPath + "/config/tweakSettings.json";

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                Settings = JsonUtility.FromJson<XSettings>(json);
            }
        }

        [System.Serializable]
        public class XSettings
        {
            public bool IsPerformanceMonitorOpened = false;
            public bool IsMediaPlayerOpened = false;
        }
    }
}
