using System;
using System.Collections.Generic;
using General.Menu;
using Stealth.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util.Geometry.Polygon;

namespace Stealth.Controller
{
    /// <summary>
    /// Manages the overall flow of a single level of the Stealth game.
    /// </summary>
    public class StealthController : MonoBehaviour
    {
        [SerializeField]
        private StealthHelper helper;
        
        [SerializeField]
        private GameObject m_timeLabel;
        
        // store starting time of level
        private float puzzleStartTime;
        
        [SerializeField]
        private ButtonContainer m_advanceButton;

        [SerializeField]
        private Text m_camerasText;
        
        // stores the current level index
        private int m_levelCounter = -1;

        // stores the number of deactivated cameras
        private int m_deactivatedCameras = 0;

        // stores the maximum number of cameras that may be disabled
        [SerializeField]
        private int m_deactivationLimit = 0;

        // A flag that denotes if the maximum number of cameras has been switched off
        public bool deactivateLimitReached = false;

        public static List<GalleryCamera> cameraList = new List<GalleryCamera>();
        public Boolean cameraVisionChanged=false;

        [SerializeField]
        private GameObject player;

        /// <summary>
        /// Initializes the level and starts gameplay.
        /// </summary>
        public void InitializeLevel()
        {
           
            m_advanceButton.Disable();

            // Calculate overlapping areas of camera views
            // Construct data structure for efficient point (player) location
            // Enable player control
        }
        
        // Use this for initialization
        void Start()
        {
            AdvanceLevel();
            foreach (GalleryCamera camera in FindObjectsOfType<GalleryCamera>())
            {
                cameraList.Add(camera);
            }
        }

        /// <summary>
        /// Checks whether the player is visible by a camera.
        /// </summary>
        private void Update()
        {
            UpdateTimeText();
            
            if (player.transform.hasChanged || cameraVisionChanged)
            {
                int count = 0;
                int disabledCount = 0;
                foreach (var camera in cameraList)
                {
                    // Only check detection if this camera is disabled
                    if (!camera.disabled)
                    {
                        if (IsPlayerInPolygon(camera.visionPoly))
                        {
                            count++;
                        } 
                    }
                    else
                    {
                        // If the camera was disabled, keep track of this
                        disabledCount++;
                    }
                    
                }
                Debug.Log(count + "cameras are currently seeing the player");

                // If at least one camera sees the player object, the level is failed, and the player must restart
                if (count > 0)
                {
                    FailLevel();
                }
                m_deactivatedCameras = disabledCount;
                deactivateLimitReached = disabledCount >= m_deactivationLimit;
                UpdateCamerasText();
                cameraVisionChanged = false;
                player.transform.hasChanged = false;
            }
        }
        
        /// <summary>
        /// Checks if the player object is inside the given polygon
        /// </summary>
        /// <param name="polygon">The polygon for which to check if the player object is inside it</param>
        /// <returns>True if the centre of the player object is inside the polygon, False otherwise.</returns>
        bool IsPlayerInPolygon(Polygon2D polygon)
        {
            List<Vector2> vertices = new List<Vector2>();
            foreach (var x in polygon.Vertices)
            {
                vertices.Add(x);
            }
            var position = player.transform.position;
            int i, j;
            bool result=false;
            for (i = 0, j = vertices.Count-1; i < vertices.Count; j = i++)
            {
                if (((vertices[i].y >= position.y) != (vertices[j].y >= position.y)) &&
                    (position.x <=
                     (vertices[j].x - vertices[i].x) * (position.y - vertices[i].y) / (vertices[j].y - vertices[i].y) +
                     vertices[i].x))
                    result = !result;
            }
            
            return result;
        }

        /// <summary>
        /// Advances to the next level.
        /// </summary>
        public void AdvanceLevel()
        {
            m_levelCounter++;

            // 5 levels?
            if (m_levelCounter < 5)
            {
                InitializeLevel();
            }
            else
            {
                SceneManager.LoadScene("stealthVictory");
            }
            
            puzzleStartTime = Time.time;
        }

        /// <summary>
        /// Resets the level. Sets m_deactivatedCameras to 0 to avoid softlocking, 
        /// re-enables all cameras, and clears the list of cameras.
        /// Then, it reloads the current scene.
        /// </summary>
        public void FailLevel()
        {
            m_deactivatedCameras = 0;
            foreach (var camera in cameraList)
            {
                camera.disabled = false;
            }
            cameraList.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }    
        
        /// <summary>
        /// Update the text field with the number of deactivated cameras
        /// </summary>
        private void UpdateCamerasText()
        {
            m_camerasText.text = "Deactivated Cameras: " + m_deactivatedCameras + " / " + m_deactivationLimit;
        }
        
        /// <summary>
        /// Update the text field with the time taken for the current level
        /// </summary>
        private void UpdateTimeText()
        {
            m_timeLabel.GetComponentInChildren<Text>().text = string.Format("Time: {0:0.}s", Time.time - puzzleStartTime);
        }
    }
}