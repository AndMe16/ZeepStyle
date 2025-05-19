using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeepSDK.Level;
using ZeepSDK.Multiplayer;
using ZeepSDK.PhotoMode;
using ZeepSDK.Racing;
using ZeepStyle.src.Patches;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.PointsUIManager;
using ZeepStyle.src.TrickDisplayManager;
using ZeepStyle.src.Tricks;

namespace ZeepStyle.src.TrickManager
{
    public class Style_TrickManager : MonoBehaviour
    {
        // States
        private bool isInAir = false;
        private bool wasInAir = false;
        private bool isInParaglider = false;
        public bool isPlayerSpawned = false;
        public bool isInPhotomode = false;

        // Rigidbody
        private Rigidbody rb;
        private Vector3 currentRight;
        private Vector3 currentForward;
        private Vector3 currentUp;


        // Initial vector references
        private Vector3 initialRight; // Reference X-axis direction
        private Vector3 initialForward; // Z-axis (forward) direction at takeoff
        private Vector3 initialUp; // Y-axis (up) direction at takeoff
        private Vector3 initialForwardVelocity; // Initial forward velocity at takeoff

        // Debuging with gizmo visualization
        //Style_GizmoVisualization gizmoVisualization;

        // Post Landing
        private float timeSinceLanding = Mathf.Infinity;
        const float landingSuppressionTime = 0.25f; // Quarter second
        const float landingUISuppressionTime = 2f;

        // Trick Points
        Style_TrickPointsManager trickPointsManager;

        // Trick Display
        Style_TrickDisplay trickDisplay;
        Coroutine hideTextOnLandCoroutine;
        public Coroutine hideTextOnAirCoroutine;

        // Type of rotation
        Style_Yaw yaw;
        Style_Pitch pitch;
        Style_Roll roll;

        // Trick list
        public List<Trick> tricksList = [];

        // PB Points UI
        Style_PointsUIManager pointsUIManager;

        // Sound Effects
        Style_SoundEffectManager soundEffectManager;

        void Start()
        {
            RacingApi.PlayerSpawned += OnPlayerSpawned;
            RacingApi.Quit += OnQuit;
            RacingApi.QuickReset += OnQuickReset;

            RacingApi.Crashed += OnCrashed;
            MultiplayerApi.DisconnectedFromGame += OnDisconnectedFromGame;
            RacingApi.RoundEnded += OnRoundEnded;
            PhotoModeApi.PhotoModeEntered += OnPhotomodeEntered;
            PhotoModeApi.PhotoModeExited += OnPhotomodeExited;
            RacingApi.LevelLoaded += OnLevelLoaded;
            Patch_HeyYouHitATrigger.OnHeyYouHitATrigger += OnCrossedFinish;
            PatchRestartLevel.OnRestart += OnRestartLevel;
            PatchPauseHandler_Pause.OnPause += PatchPauseHandler_OnPause_OnUnpause;


            yaw = FindObjectOfType<Style_Yaw>();
            pitch = FindObjectOfType<Style_Pitch>();
            roll = FindObjectOfType<Style_Roll>();

            trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();

            trickDisplay = FindObjectOfType<Style_TrickDisplay>();

            pointsUIManager = FindObjectOfType<Style_PointsUIManager>();

            soundEffectManager = FindObjectOfType<Style_SoundEffectManager>();

            //gizmoVisualization = FindObjectOfType<Style_GizmoVisualization>();
        }

        private void PatchPauseHandler_OnPause_OnUnpause(PauseHandler handler)
        {
            if (handler.IsPaused)
            {
                soundEffectManager.StopSound("HighSpeedSpin_Sound");
            }
        }

        private void OnRestartLevel(GameMaster master)
        {
            isPlayerSpawned = false;

            ResetVars();
        }

        private void OnRoundEnded()
        {
            isPlayerSpawned = false;

            ResetVars();
        }

        private void OnDisconnectedFromGame()
        {
            isPlayerSpawned = false;

            ResetVars();
        }

        private void OnCrashed(CrashReason reason)
        {
            isPlayerSpawned = false;

            if (tricksList != null && tricksList.Count > 0)
            {
                Plugin.Logger.LogInfo("Player crashed with tricks");
                soundEffectManager.PlaySound("Crash_sound");
            }

            ResetVars();

        }

        private void OnQuickReset()
        {
            isPlayerSpawned = false;

            ResetVars();
        }

        private void OnQuit()
        {
            isPlayerSpawned = false;

            ResetVars();
        }

        private void OnPlayerSpawned()
        {
            isPlayerSpawned = true;

            ResetVars();

            rb = PatchGetRB.Rb;
            trickPointsManager.ResetTotalRunPoints();
            trickDisplay.CreateDisplay();
            pointsUIManager.CreateUI();
            trickPointsManager.UpdateCurrentRunPoints(0);
            Plugin.Logger.LogInfo("Player spawned");
        }

        private void OnPhotomodeExited()
        {
            Plugin.Logger.LogInfo("Photomode exited");
            isInPhotomode = false;
            trickDisplay.ShowText();
            pointsUIManager.ShowText();
        }

        private void OnPhotomodeEntered()
        {
            Plugin.Logger.LogInfo("Photomode entered");
            isInPhotomode = true;
            trickDisplay.HideText();
            pointsUIManager.HideText();
        }

        // Method to detect if the rigidbody has landed back on the ground
        void OnLand()
        {
            Plugin.Logger.LogInfo("OnLand: Player has landed!");

            isInAir = false;  // Reset state when landing
            wasInAir = false;

            timeSinceLanding = 0f;

            yaw.ClearVars();
            pitch.ClearVars();
            roll.ClearVars();

            if (hideTextOnAirCoroutine != null)
            {
                trickDisplay.StopHideTextOnAirCoroutine();
            }
            if (hideTextOnLandCoroutine != null)
            {
                //Plugin.Logger.LogInfo($"OnLand: Stoping hideTextOnLandCoroutine {hideTextOnLandCoroutine.ToString()}");
                StopCoroutine(hideTextOnLandCoroutine);
            }
            //Plugin.Logger.LogInfo("OnLand: Starting Coroutine trickDisplay.HideTextAfterSeconds(2)");
            hideTextOnLandCoroutine = StartCoroutine(trickDisplay.HideTextAfterSeconds(2));
            //Plugin.Logger.LogInfo($"OnLand: Coroutine trickDisplay.HideTextAfterSeconds(2) started: {hideTextOnLandCoroutine.ToString()}");
            if (tricksList != null && tricksList.Count > 0)
            {
                int totalPoints = trickPointsManager.CalculateTotalJumpPoints(tricksList);
                int totalRunPoints = trickPointsManager.AddToTotalRunPoints(totalPoints);
                trickDisplay.LandingDisplay(totalPoints);
                trickPointsManager.UpdateCurrentRunPoints(totalRunPoints);
                soundEffectManager.PlaySound("Landing_Sound");
            }
            tricksList.Clear();   // Clear the list of tricks
            trickDisplay.displayTextList.Clear();

            soundEffectManager.StopSound("HighSpeedSpin_Sound");
            yaw.spinSpeedBuffer.Clear();

            // gizmoVisualization.CleanupAxisVisuals();
            // gizmoVisualization.CleanupReferencePlanes();
        }

        // Method to detect if the rigidbody has left the ground
        void OnLeaveGround()
        {

            if (ModConfig.tricksDetectionOn.Value)
            {
                Plugin.Logger.LogInfo("OnLeaveGround: Player is airborne!");
                // Calculate the initial up, forward, and right vectors
                initialUp = Vector3.up;

                // Calculate the initial forward velocity and forward direction
                initialForwardVelocity = Vector3.ProjectOnPlane(rb.velocity, initialUp).normalized;
                initialForward = Vector3.ProjectOnPlane(rb.transform.forward, initialUp).normalized;

                // Check if the player is facing the opposite direction
                float alignment = Vector3.Dot(initialForwardVelocity, initialForward);
                if (alignment < 0)
                {
                    Plugin.Logger.LogInfo("OnLeaveGround: Player is facing the opposite direction!");
                    initialForwardVelocity = -initialForwardVelocity;
                }

                initialRight = Vector3.Cross(initialUp, initialForwardVelocity).normalized;

                yaw.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
                pitch.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
                roll.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
            }

            if (timeSinceLanding > landingUISuppressionTime)
            {
                trickDisplay.ResetText();
                if (hideTextOnLandCoroutine != null)
                {
                    //Plugin.Logger.LogInfo($"OnLeaveGround: Stoping hideTextOnLandCoroutine {hideTextOnLandCoroutine.ToString()}");
                    StopCoroutine(hideTextOnLandCoroutine);
                }
            }

            // gizmoVisualization.CreateAxisVisuals(rb);
            // gizmoVisualization.CreateReferencePlanes(initialRotation, rb);
        }

        private void OnLevelLoaded()
        {
            trickPointsManager.currentHash = LevelApi.CurrentHash;
            Plugin.Logger.LogInfo($"Current Level Hash: {trickPointsManager.currentHash}");
            trickPointsManager.LoadLevelPB(trickPointsManager.currentHash);
            trickPointsManager.ResetCurrentSessionPoints();
        }

        private void OnCrossedFinish(bool hasFinished)
        {
            // Plugin.Logger.LogInfo("Player crossed the finish line");
            if (hasFinished)
            {
                if (tricksList != null && tricksList.Count > 0)
                {
                    Plugin.Logger.LogInfo("Player finished with tricks");
                    int totalPoints = trickPointsManager.CalculateTotalJumpPoints(tricksList);
                    _ = trickPointsManager.AddToTotalRunPoints(totalPoints);
                }

                if (trickPointsManager.totalRunPoints > 0)
                {
                    soundEffectManager.PlaySound("Finish_Sound");
                }

                // Update current session PB if the current run is better
                if (trickPointsManager.totalRunPoints > trickPointsManager.bestPbCurrentSession)
                {
                    Plugin.Logger.LogInfo($"New PB (Current Session): {trickPointsManager.totalRunPoints} > {trickPointsManager.bestPbCurrentSession}");
                    trickPointsManager.bestPbCurrentSession = trickPointsManager.totalRunPoints;
                    if (pointsUIManager.pointsInfoText != null)
                    {
                        pointsUIManager.UpdatePointsInfoText();
                    }
                }

                // Update all-time PB if necessary
                if (trickPointsManager.totalRunPoints > trickPointsManager.bestPbAllTime)
                {
                    Plugin.Logger.LogInfo($"New PB (All Sessions): {trickPointsManager.totalRunPoints} > {trickPointsManager.bestPbAllTime}");
                    trickPointsManager.bestPbAllTime = trickPointsManager.totalRunPoints;
                    if (pointsUIManager.pointsInfoText != null)
                    {
                        pointsUIManager.UpdatePointsInfoText();
                    }
                    trickPointsManager.SaveLevelPB(trickPointsManager.currentHash);
                }
            }
            else
            {
                isPlayerSpawned = false;
                ResetVars();
            }
        }

        private void ResetVars()
        {
            isInAir = false;  // Reset state when landing
            wasInAir = false;
            isInParaglider = false;
            yaw.ClearVars();
            pitch.ClearVars();
            roll.ClearVars();
            trickDisplay.DestroyComponent();
            pointsUIManager.DestroyComponent();
            tricksList.Clear();
            trickDisplay.displayTextList.Clear();
            StopAllCoroutines();
            soundEffectManager.StopSound("HighSpeedSpin_Sound");
            yaw.spinSpeedBuffer.Clear();
        }

        void FixedUpdate()
        {
            if (isPlayerSpawned && SceneManager.GetActiveScene().name == "GameScene")
            {
                isInAir = PatchAreAllWheelsInAir.IsInTheAir;

                // Check if the player is in the paraglider
                isInParaglider = PatchSetZeepkistState.currentState == 3;

                isInAir = !isInParaglider && isInAir;

                if (isInAir && ModConfig.tricksDetectionOn.Value)
                {
                    timeSinceLanding += Time.deltaTime;

                    if (timeSinceLanding < landingSuppressionTime)
                    {
                        return;
                    }


                    rb = PatchGetRB.Rb;
                    if (rb == null)
                    {
                        return;
                    }
                    currentRight = rb.transform.right;
                    currentForward = rb.transform.forward;
                    currentUp = rb.transform.up;

                    if (!wasInAir)
                    {
                        OnLeaveGround();
                        //Plugin.Logger.LogInfo("Player is Airborne!");
                    }

                    // Tricks
                    yaw.DetectSpinTrick(currentForward, currentUp);
                    pitch.DetectFlipTrick(currentForward, currentRight, currentUp);
                    roll.DetectRollTrick(currentUp, currentForward);

                    // gizmoVisualization.UpdateAllAxisVisuals(rb);
                    // gizmoVisualization.UpdatePlanePositions(rb);
                }
                else if (wasInAir && !isInAir)
                {
                    OnLand();
                    //Plugin.Logger.LogInfo("Player is no longer Airborne!");
                }
                wasInAir = isInAir;
            }
        }

        private void OnDestroy()
        {
            RacingApi.PlayerSpawned -= OnPlayerSpawned;
            RacingApi.Quit -= OnQuit;
            RacingApi.QuickReset -= OnQuickReset;
            RacingApi.Crashed -= OnCrashed;
            MultiplayerApi.DisconnectedFromGame -= OnDisconnectedFromGame;
            RacingApi.RoundEnded -= OnRoundEnded;
            PhotoModeApi.PhotoModeEntered -= OnPhotomodeEntered;
            PhotoModeApi.PhotoModeExited -= OnPhotomodeExited;
            RacingApi.LevelLoaded -= OnLevelLoaded;
            Patch_HeyYouHitATrigger.OnHeyYouHitATrigger -= OnCrossedFinish;
            PatchRestartLevel.OnRestart -= OnRestartLevel;
            PatchPauseHandler_Pause.OnPause -= PatchPauseHandler_OnPause_OnUnpause;
        }
    }
    public class Trick
    {
        public string trickName;
        public string rotation;
        public bool isInverse;
        public bool isPositiveDelta;
    }
}
