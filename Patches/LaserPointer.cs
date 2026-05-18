using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace xsoverlay_tweak.Patches
{
    [HarmonyPatch(typeof(Raycaster))]
    internal class LaserPointer
    {
        private static Unity_Overlay Laser;
        private static float distance;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(Raycaster __instance, ref GameObject ___VisualCursorElement, ref Unity_Overlay ___VisualCursorElementOverlay)
        {
            if (Laser != null) return;

            Laser = GameObject.Instantiate(___VisualCursorElementOverlay);
            Object.Destroy(Laser.GetComponent<UI_RelativeTransformManipulator>());

            Laser.name = $"{___VisualCursorElementOverlay.name}.laser";
            Laser.overlayName = $"{___VisualCursorElementOverlay.overlayName}.laser";
            Laser.overlayKey = $"{___VisualCursorElementOverlay.overlayKey.ToLower()}.laser";
            Laser.overlayTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            Laser.AutoUpdateOverlayTexture = false;

            Laser.gameObject.SetActive(true);
        }

        [HarmonyPatch("SetVisualCursorTransform")]
        [HarmonyPostfix]
        public static void SetVisualCursorTransform(Raycaster __instance, ref Vector3 ___CurrentRayPosition, ref Vector3 ___RayHitPoint, ref Vector3 ___CurrentRayDirection)
        {
            if (Laser == null) return;
            if (DesktopCursorManager.Instance.GetCurrentInputDevice() != __instance) return;

            distance = Vector3.Distance(___CurrentRayPosition, ___RayHitPoint);

            var headPos = Overlay_Manager.Instance.head.position;

            Laser.transform.position = ___CurrentRayPosition + (___CurrentRayDirection * (distance / 2));
            Laser.transform.up = ___CurrentRayDirection;
            //Laser.transform.localScale = new Vector3(1, distance, 1);

            Laser.colorTint = Color.green;


            int Tickness = 300;

            Laser.overlayTexture = new Texture2D(1, (int)distance * Tickness, TextureFormat.RGB24, false);
            Laser.overlay.overlayTexture = Laser.overlayTexture;
            Laser.overlay.overlayWidthInMeters = distance / Tickness;
        }

        /*[HarmonyPatch("ReleasePointer")]
        [HarmonyPostfix]
        public static void ReleasePointer(Raycaster __instance)
        {
            
        }*/
    }
}
