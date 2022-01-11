using System;
using System.Collections.Generic;
using General.Menu;
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

        //stores the number of deactivated cameras
        private int m_deactivatedCameras = 0;
        
        public static List<String> cameraNames = new List<String>();
        public static List<Polygon2D> cameraPolygons = new List<Polygon2D>();
        public static List<Boolean> playerVisibility = new List<Boolean>();
        public static Boolean cameraVisionChanged=false;

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
                foreach (var polygon in cameraPolygons)
                {
                    if (IsPlayerInPolygon(polygon))
                        count++;
                }
                Debug.Log(count + "cameras are currently seeing the player");
                
                cameraVisionChanged = false;
                player.transform.hasChanged = false;
            }
        }
        
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
        /// Resets the level.
        /// </summary>
        public void FailLevel()
        {
            throw new NotImplementedException();
        }    
        
        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateCamerasText()
        {
            m_camerasText.text = "Deactivated Cameras: " + m_deactivatedCameras;
        }
        
        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateTimeText()
        {
            m_timeLabel.GetComponentInChildren<Text>().text = string.Format("Time: {0:0.}s", Time.time - puzzleStartTime);
        }
    }
}