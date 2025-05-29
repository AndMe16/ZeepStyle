using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeepSDK.Level;
using ZeepSDK.Multiplayer;
using ZeepSDK.PhotoMode;
using ZeepSDK.Racing;
using ZeepStyle.Patches;
using ZeepStyle.PointsManager;
using ZeepStyle.PointsUIManager;
using ZeepStyle.SoundEffectsManager;
using ZeepStyle.TrickDisplayManager;
using ZeepStyle.Tricks;

namespace ZeepStyle.TrickManager;

public class StyleTrickManager : MonoBehaviour
{
    private const float LandingSuppressionTime = 0.25f; // Quarter second
    private const float LandingUISuppressionTime = 2f;
    public bool isPlayerSpawned;
    public bool isInPhotomode;
    private Vector3 currentForward;
    private Vector3 currentRight;
    private Vector3 currentUp;
    public Coroutine HideTextOnAirCoroutine;
    private Coroutine hideTextOnLandCoroutine;
    private Vector3 initialForward; // Z-axis (forward) direction at takeoff
    private Vector3 initialForwardVelocity; // Initial forward velocity at takeoff


    // Initial vector references
    private Vector3 initialRight; // Reference X-axis direction

    private Vector3 initialUp; // Y-axis (up) direction at takeoff

    // States
    private bool isInAir;
    private bool isInParaglider;
    private StylePitch pitch;

    // PB Points UI
    private StylePointsUIManager pointsUIManager;

    // Rigidbody
    private Rigidbody rb;
    private StyleRoll roll;

    // Sound Effects
    private StyleSoundEffectManager soundEffectManager;

    // Debuging with gizmo visualization
    //Style_GizmoVisualization gizmoVisualization;

    // Post Landing
    private float timeSinceLanding = Mathf.Infinity;

    // Trick Display
    private StyleTrickDisplay trickDisplay;

    // Trick Points
    private StyleTrickPointsManager trickPointsManager;

    // Trick list
    public readonly List<Trick> TricksList = [];
    private bool wasInAir;

    // Type of rotation
    private StyleYaw yaw;

    private void Start()
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
        PatchHeyYouHitATrigger.OnHeyYouHitATrigger += OnCrossedFinish;
        PatchRestartLevel.OnRestart += OnRestartLevel;
        PatchPauseHandlerPause.OnPause += PatchPauseHandler_OnPause_OnUnpause;


        yaw = FindObjectOfType<StyleYaw>();
        pitch = FindObjectOfType<StylePitch>();
        roll = FindObjectOfType<StyleRoll>();

        trickPointsManager = FindObjectOfType<StyleTrickPointsManager>();

        trickDisplay = FindObjectOfType<StyleTrickDisplay>();

        pointsUIManager = FindObjectOfType<StylePointsUIManager>();

        soundEffectManager = FindObjectOfType<StyleSoundEffectManager>();

        //gizmoVisualization = FindObjectOfType<Style_GizmoVisualization>();
    }

    private void FixedUpdate()
    {
        if (!isPlayerSpawned || SceneManager.GetActiveScene().name != "GameScene") return;
        isInAir = PatchAreAllWheelsInAir.isInTheAir;

        // Check if the player is in the paraglider
        isInParaglider = PatchSetZeepkistState.currentState == 3;

        isInAir = !isInParaglider && isInAir;

        if (isInAir && ModConfig.tricksDetectionOn.Value)
        {
            timeSinceLanding += Time.deltaTime;

            if (timeSinceLanding < LandingSuppressionTime) return;


            rb = PatchGetRb.rb;
            if (!rb) return;
            currentRight = rb.transform.right;
            currentForward = rb.transform.forward;
            currentUp = rb.transform.up;

            if (!wasInAir) OnLeaveGround();
            //Plugin.Logger.LogInfo("Player is Airborne!");
            // Tricks
            var detectedSpin = yaw.DetectSpinTrick(currentForward, currentUp);
            var detectedFlip = pitch.DetectFlipTrick(currentForward, currentRight, currentUp);
            var detectedRoll = roll.DetectRollTrick(currentUp, currentForward);

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
        PatchHeyYouHitATrigger.OnHeyYouHitATrigger -= OnCrossedFinish;
        PatchRestartLevel.OnRestart -= OnRestartLevel;
        PatchPauseHandlerPause.OnPause -= PatchPauseHandler_OnPause_OnUnpause;
    }

    private void PatchPauseHandler_OnPause_OnUnpause(PauseHandler handler)
    {
        if (handler.IsPaused) soundEffectManager.StopSound("HighSpeedSpin_Sound");
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

        if (TricksList is { Count: > 0 })
        {
            Plugin.logger.LogInfo("Player crashed with tricks");
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

        rb = PatchGetRb.rb;
        trickPointsManager.ResetTotalRunPoints();
        trickDisplay.CreateDisplay();
        pointsUIManager.CreateUI();
        trickPointsManager.UpdateCurrentRunPoints(0);
        Plugin.logger.LogInfo("Player spawned");
    }

    private void OnPhotomodeExited()
    {
        Plugin.logger.LogInfo("Photomode exited");
        isInPhotomode = false;
        trickDisplay.ShowText();
        pointsUIManager.ShowText();
    }

    private void OnPhotomodeEntered()
    {
        Plugin.logger.LogInfo("Photomode entered");
        isInPhotomode = true;
        trickDisplay.HideText();
        pointsUIManager.HideText();
    }

    // Method to detect if the rigidbody has landed back on the ground
    private void OnLand()
    {
        Plugin.logger.LogInfo("OnLand: Player has landed!");

        isInAir = false; // Reset state when landing
        wasInAir = false;

        timeSinceLanding = 0f;

        yaw.ClearVars();
        pitch.ClearVars();
        roll.ClearVars();

        if (HideTextOnAirCoroutine != null) trickDisplay.StopHideTextOnAirCoroutine();
        if (hideTextOnLandCoroutine != null)
            //Plugin.Logger.LogInfo($"OnLand: Stoping hideTextOnLandCoroutine {hideTextOnLandCoroutine.ToString()}");
            StopCoroutine(hideTextOnLandCoroutine);
        //Plugin.Logger.LogInfo("OnLand: Starting Coroutine trickDisplay.HideTextAfterSeconds(2)");
        hideTextOnLandCoroutine = StartCoroutine(trickDisplay.HideTextAfterSeconds(2));
        //Plugin.Logger.LogInfo($"OnLand: Coroutine trickDisplay.HideTextAfterSeconds(2) started: {hideTextOnLandCoroutine.ToString()}");
        if (TricksList is { Count: > 0 })
        {
            var totalPoints = trickPointsManager.CalculateTotalJumpPoints(TricksList);
            var totalRunPoints = trickPointsManager.AddToTotalRunPoints(totalPoints);
            trickDisplay.LandingDisplay(totalPoints);
            trickPointsManager.UpdateCurrentRunPoints(totalRunPoints);
            soundEffectManager.PlaySound("Landing_Sound");
        }

        TricksList?.Clear(); // Clear the list of tricks
        trickDisplay.displayTextList.Clear();

        soundEffectManager.StopSound("HighSpeedSpin_Sound");
        yaw.SpinSpeedBuffer.Clear();

        // gizmoVisualization.CleanupAxisVisuals();
        // gizmoVisualization.CleanupReferencePlanes();
    }

    // Method to detect if the rigidbody has left the ground
    private void OnLeaveGround()
    {
        if (ModConfig.tricksDetectionOn.Value)
        {
            Plugin.logger.LogInfo("OnLeaveGround: Player is airborne!");
            // Calculate the initial up, forward, and right vectors
            initialUp = Vector3.up;

            // Calculate the initial forward velocity and forward direction
            initialForwardVelocity = Vector3.ProjectOnPlane(rb.velocity, initialUp).normalized;
            initialForward = Vector3.ProjectOnPlane(rb.transform.forward, initialUp).normalized;

            // Check if the player is facing the opposite direction
            var alignment = Vector3.Dot(initialForwardVelocity, initialForward);
            if (alignment < 0)
            {
                Plugin.logger.LogInfo("OnLeaveGround: Player is facing the opposite direction!");
                initialForwardVelocity = -initialForwardVelocity;
            }

            initialRight = Vector3.Cross(initialUp, initialForwardVelocity).normalized;

            yaw.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
            pitch.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
            roll.OnLeaveGround(initialUp, initialForwardVelocity, initialRight);
        }

        if (!(timeSinceLanding > LandingUISuppressionTime)) return;
        trickDisplay.ResetText();
        if (hideTextOnLandCoroutine != null)
            //Plugin.Logger.LogInfo($"OnLeaveGround: Stoping hideTextOnLandCoroutine {hideTextOnLandCoroutine.ToString()}");
            StopCoroutine(hideTextOnLandCoroutine);

        // gizmoVisualization.CreateAxisVisuals(rb);
        // gizmoVisualization.CreateReferencePlanes(initialRotation, rb);
    }

    private void OnLevelLoaded()
    {
        trickPointsManager.currentHash = LevelApi.CurrentHash;
        Plugin.logger.LogInfo($"Current Level Hash: {trickPointsManager.currentHash}");
        trickPointsManager.LoadLevelPb(trickPointsManager.currentHash);
        trickPointsManager.ResetCurrentSessionPoints();
    }

    private void OnCrossedFinish(bool hasFinished)
    {
        // Plugin.Logger.LogInfo("Player crossed the finish line");
        if (hasFinished)
        {
            if (TricksList is { Count: > 0 })
            {
                Plugin.logger.LogInfo("Player finished with tricks");
                var totalPoints = trickPointsManager.CalculateTotalJumpPoints(TricksList);
                _ = trickPointsManager.AddToTotalRunPoints(totalPoints);
            }

            if (trickPointsManager.totalRunPoints > 0) soundEffectManager.PlaySound("Finish_Sound");

            // Update current session PB if the current run is better
            if (trickPointsManager.totalRunPoints > trickPointsManager.bestPbCurrentSession)
            {
                Plugin.logger.LogInfo(
                    $"New PB (Current Session): {trickPointsManager.totalRunPoints} > {trickPointsManager.bestPbCurrentSession}");
                trickPointsManager.bestPbCurrentSession = trickPointsManager.totalRunPoints;
                if (pointsUIManager.pointsInfoText) pointsUIManager.UpdatePointsInfoText();
            }

            // Update all-time PB if necessary
            if (trickPointsManager.totalRunPoints <= trickPointsManager.bestPbAllTime) return;
            Plugin.logger.LogInfo(
                $"New PB (All Sessions): {trickPointsManager.totalRunPoints} > {trickPointsManager.bestPbAllTime}");
            trickPointsManager.bestPbAllTime = trickPointsManager.totalRunPoints;
            if (pointsUIManager.pointsInfoText) pointsUIManager.UpdatePointsInfoText();
            trickPointsManager.SaveLevelPb(trickPointsManager.currentHash);
        }
        else
        {
            isPlayerSpawned = false;
            ResetVars();
        }
    }

    private void ResetVars()
    {
        isInAir = false; // Reset state when landing
        wasInAir = false;
        isInParaglider = false;
        yaw.ClearVars();
        pitch.ClearVars();
        roll.ClearVars();
        trickDisplay.DestroyComponent();
        pointsUIManager.DestroyComponent();
        TricksList.Clear();
        trickDisplay.displayTextList.Clear();
        StopAllCoroutines();
        soundEffectManager.StopSound("HighSpeedSpin_Sound");
        yaw.SpinSpeedBuffer.Clear();
    }
}

public class Trick
{
    public bool IsInverse;
    public bool IsPositiveDelta;
    public string Rotation;
    public string TrickName;
}