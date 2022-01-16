using System.Collections;
using UnityEngine;
using Stealth.Objects;
using UnityEditor;

namespace Stealth
{
    /// <summary>
    /// Manages the <see cref="GalleryCamera"/>s in the scene.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        private static GalleryCamera[] cameras;

        private void Awake()
        {
            cameras = FindObjectsOfType<GalleryCamera>();
        }

        [MenuItem("Game/Update vision all cameras _F10")]
        public static void UpdateVisionCameras()
        {
            if (!Application.isPlaying)
            {
                cameras = FindObjectsOfType<GalleryCamera>();
            }
            foreach (GalleryCamera cam in cameras)
            {
                cam.ComputeVisionArea();
            }
        }

        public static bool IsPointVisible(Vector2 point)
        {
            foreach (GalleryCamera camera in cameras)
            {
                if (camera.IsPointVisible(point))
                {
                    return true;
                }
            }
            return false;
        }
    }
}