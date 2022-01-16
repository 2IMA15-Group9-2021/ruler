using System;
using System.Collections;
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
        private Text timeLabel;

        [SerializeField]
        private RectTransform gameOverLabel;

        [SerializeField]
        private float levelResetDelay = 1;
        
        [SerializeField]
        private ButtonContainer advanceButton;

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private string nextSceneName = "stealthVictory";

        private FinishArea finishArea;

        // store starting time of level
        private float puzzleStartTime;

        private bool gameEnded;

        private Vector3 initialPlayerPosition;


        /// <summary>
        /// Initializes the level and starts gameplay.
        /// </summary>
        private void InitializeLevel()
        {
            initialPlayerPosition = playerController.transform.position;
            ResetLevel();
        }

        private void ResetLevel()
        {
            advanceButton.Disable();
            gameOverLabel.gameObject.SetActive(false);

            playerController.transform.position = initialPlayerPosition;
            playerController.enabled = true;

            CameraManager.UpdateVisionCameras();
            CameraManager.EnableAllCameras();
        }

        private void Awake()
        {
            finishArea = FindObjectOfType<FinishArea>();
        }

        void Start()
        {
            puzzleStartTime = Time.time;
            InitializeLevel();
        }

        private void OnEnable()
        {
            finishArea.PlayerEnteredGoal += OnPlayerEnteredGoal;
            finishArea.PlayerExitedGoal += OnPlayerExitedGoal;
        }

        private void OnDisable()
        {
            finishArea.PlayerEnteredGoal -= OnPlayerEnteredGoal;
            finishArea.PlayerExitedGoal -= OnPlayerExitedGoal;
        }

        private void OnPlayerEnteredGoal()
        {
            advanceButton.Enable();
        }

        private void OnPlayerExitedGoal()
        {
            advanceButton.Disable();
        }

        /// <summary>
        /// Checks whether the player is visible by a camera.
        /// </summary>
        private void Update()
        {
            UpdateTimeText();

            if (CameraManager.IsPointVisible(playerController.transform.position))
            {
                StartCoroutine(FailLevel());
            }
        }

        /// <summary>
        /// Advances to the next level.
        /// </summary>
        /// <remarks>
        /// This method should be registered with a button in the editor.
        /// </remarks>
        public void AdvanceLevel()
        {
            SceneManager.LoadScene(nextSceneName);
        }

        /// <summary>
        /// Resets the level.
        /// </summary>
        private IEnumerator FailLevel()
        {
            gameEnded = true;

            playerController.enabled = false;
            gameOverLabel.gameObject.SetActive(true);

            yield return new WaitForSeconds(levelResetDelay);

            ResetLevel();
        }
        
        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateTimeText()
        {
            timeLabel.text = $"Time: {(int)Mathf.Floor(Time.time - puzzleStartTime)}s";
        }
    }
}