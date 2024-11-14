using System.Collections.Generic;
using UnityEngine;
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
        public bool isPlayerSpawned = false;

        // Rigidbody
        private Rigidbody rb;
        private Vector3 currentRight;
        private Vector3 currentForward;
        private Vector3 currentUp;


        // Initial vector references
        private Vector3 initialRight; // Reference X-axis direction
        private Vector3 initialForward; // Z-axis (forward) direction at takeoff
        private Vector3 initialUp; // Y-axis (up) direction at takeoff

        // Debuging with gizmo visualization
        //Style_GizmoVisualization gizmoVisualization;

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


            yaw = FindObjectOfType<Style_Yaw>();
            pitch = FindObjectOfType<Style_Pitch>();
            roll = FindObjectOfType<Style_Roll>();

            trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();

            trickDisplay = FindObjectOfType<Style_TrickDisplay>();

            pointsUIManager = FindObjectOfType<Style_PointsUIManager>();

            soundEffectManager = FindObjectOfType<Style_SoundEffectManager>();

            //gizmoVisualization = FindObjectOfType<Style_GizmoVisualization>();
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
            trickDisplay.ShowText();
        }

        private void OnPhotomodeEntered()
        {
            trickDisplay.HideText();
        }

        // Method to detect if the rigidbody has landed back on the ground
        void OnLand()
        {
            isInAir = false;  // Reset state when landing
            wasInAir = false;

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

            // gizmoVisualization.CleanupAxisVisuals();
            // gizmoVisualization.CleanupReferencePlanes();
        }

        // Method to detect if the rigidbody has left the ground
        void OnLeaveGround()
        {
            isInAir = true;

            if (ModConfig.tricksDetectionOn.Value)
            {
                initialUp = Vector3.up;
                initialForward = Vector3.ProjectOnPlane(rb.transform.forward, initialUp);
                initialRight = Vector3.Cross(initialUp, initialForward).normalized;

                yaw.OnLeaveGround(initialUp, initialForward, initialRight);
                pitch.OnLeaveGround(initialUp, initialForward, initialRight);
                roll.OnLeaveGround(initialUp, initialForward, initialRight);
            }

            trickDisplay.ResetText();

            if (hideTextOnLandCoroutine != null)
            {
                //Plugin.Logger.LogInfo($"OnLeaveGround: Stoping hideTextOnLandCoroutine {hideTextOnLandCoroutine.ToString()}");
                StopCoroutine(hideTextOnLandCoroutine);
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
            Plugin.Logger.LogInfo("Player crossed the finish line");
            if (hasFinished)
            {
                // Update current session PB if the current run is better
                if (trickPointsManager.totalRunPoints > trickPointsManager.bestPbCurrentSession)
                {
                    trickPointsManager.bestPbCurrentSession = trickPointsManager.totalRunPoints;
                    if (pointsUIManager.bestPbCurrentSessionText != null)
                    {
                        pointsUIManager.bestPbCurrentSessionText.text = $"Best PB (Current Session): {trickPointsManager.bestPbCurrentSession}";
                    }
                }

                // Update all-time PB if necessary
                if (trickPointsManager.totalRunPoints > trickPointsManager.bestPbAllTime)
                {
                    trickPointsManager.bestPbAllTime = trickPointsManager.totalRunPoints;
                    if (pointsUIManager.bestPbAllTimeText != null)
                    {
                        pointsUIManager.bestPbAllTimeText.text = $"Best PB (All Sessions): {trickPointsManager.bestPbAllTime}";
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
            yaw.ClearVars();
            pitch.ClearVars();
            roll.ClearVars();
            trickDisplay.DestroyComponent();
            pointsUIManager.DestroyComponent();
            tricksList.Clear();
            trickDisplay.displayTextList.Clear();
            StopAllCoroutines();
        }

        void Update()
        {
            if (isPlayerSpawned)
            {
                isInAir = PatchAreAllWheelsInAir.IsInTheAir;

                if (isInAir && ModConfig.tricksDetectionOn.Value)
                {
                    rb = PatchGetRB.Rb;
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
                    pitch.DetectFlipTrick(currentForward, currentRight);
                    roll.DetectRollTrick(currentUp, currentForward);

                    // gizmoVisualization.UpdateAllAxisVisuals(rb);
                    // gizmoVisualization.UpdatePlanePositions(rb);
                }
                else if (wasInAir)
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
