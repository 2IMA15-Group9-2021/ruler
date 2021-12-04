using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stealth
{
    /// <summary>
    /// Manages the overall flow of a single level of the Stealth game.
    /// </summary>
    public class StealthController : MonoBehaviour
    {
        [SerializeField]
        private StealthHelper helper;

        /// <summary>
        /// Initializes the level and starts gameplay.
        /// </summary>
        public void InitializeLevel()
        {
            throw new NotImplementedException();

            // Calculate overlapping areas of camera views
            // Construct data structure for efficient point (player) location
            // Enable player control
        }

        /// <summary>
        /// Checks whether the player is visible by a camera.
        /// </summary>
        private void Update()
        {
            
        }

        /// <summary>
        /// Advances to the next level.
        /// </summary>
        public void AdvanceLevel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the level.
        /// </summary>
        public void FailLevel()
        {
            throw new NotImplementedException();
        }    
    }
}