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
        private GameObject timeLabel;
        
        [SerializeField]
        private ButtonContainer advanceButton;

        [SerializeField]
        private Text cameraText;

        [SerializeField]
        private GameObject player;

        // store starting time of level
        private float puzzleStartTime;

        // stores the current level index
        private int levelCounter = -1;

        //stores the number of deactivated cameras
        private int deactivatedCameras = 0;

        public static List<GalleryCamera> cameraList = new List<GalleryCamera>();
        public static List<String> cameraNames = new List<String>();
        public static List<Polygon2D> cameraPolygons = new List<Polygon2D>();
        public static List<Boolean> playerVisibility = new List<Boolean>();
        public Boolean cameraVisionChanged=false;


        /// <summary>
        /// Initializes the level and starts gameplay.
        /// </summary>
        public void InitializeLevel()
        {
            advanceButton.Disable();
        }
        
        // Use this for initialization
        void Start()
        {
        }

        /// <summary>
        /// Checks whether the player is visible by a camera.
        /// </summary>
        private void Update()
        {
            //UpdateTimeText();
            
            //if (player.transform.hasChanged || cameraVisionChanged)
            //{
            //    int count = 0;
            //    foreach (var camera in cameraList)
            //    {
            //        if (!camera.disabled)
            //        {
            //            if (IsPlayerInPolygon(camera.visionArea))
            //                count++;
            //        }
                    
            //    }
            //    Debug.Log(count + "cameras are currently seeing the player");
                
            //    cameraVisionChanged = false;
            //    player.transform.hasChanged = false;
            //}
        }

        ///// <summary>
        ///// Advances to the next level.
        ///// </summary>
        //public void AdvanceLevel()
        //{
        //    levelCounter++;

        //    // 5 levels?
        //    if (levelCounter < 5)
        //    {
        //        InitializeLevel();
        //    }
        //    else
        //    {
        //        SceneManager.LoadScene("stealthVictory");
        //    }
            
        //    puzzleStartTime = Time.time;
        //}

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
            cameraText.text = "Deactivated Cameras: " + deactivatedCameras;
        }
        
        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateTimeText()
        {
            timeLabel.GetComponentInChildren<Text>().text = string.Format("Time: {0:0.}s", Time.time - puzzleStartTime);
        }
    }
}