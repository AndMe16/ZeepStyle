using System;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.Multiplayer;
using ZeepSDK.PhotoMode;
using ZeepSDK.Racing;
using ZeepStyle;


public class Style_TrickManager : MonoBehaviour
{
    // States
    private bool isInAir = false;
    private bool wasInAir = false;
    private bool isPlayerSpawned = false;
    private bool isDead = false;

    // Rigidbody
    private Rigidbody rb;
    private Vector3 currentRight;
    private Vector3 currentForward;
    private Vector3 currentUp;

    // Quaternions
    private Quaternion initialRotation;

    // Initial vector references
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialForward; // Z-axis (forward) direction at takeoff
    private Vector3 initialUp; // Y-axis (up) direction at takeoff

    // Debuging with gizmo visualization
    Style_GizmoVisualization gizmoVisualization;

    // Trick Display
    Style_TrickDisplay trickDisplay;
    Coroutine hideTextOnLandCoroutine;
    public Coroutine hideTextOnAirCoroutine;

    // Type of rotation
    Style_Yaw yaw;
    Style_Pitch pitch;
    Style_Roll roll;

    // Trick list
    public List<Trick> tricksList = new List<Trick>();

    void Start()
    {
        RacingApi.PlayerSpawned += OnPlayerSpawned;
        RacingApi.Quit += OnQuit;
        RacingApi.QuickReset += OnQuickReset;
        RacingApi.Crashed += OnCrashed;
        RacingApi.CrossedFinishLine += OnCrossedFinishLine;
        MultiplayerApi.DisconnectedFromGame += OnDisconnectedFromGame;
        RacingApi.RoundEnded += OnRoundEnded;
        PhotoModeApi.PhotoModeEntered += OnPhotomodeEntered;
        PhotoModeApi.PhotoModeExited += OnPhotomodeExited;

        yaw = FindObjectOfType<Style_Yaw>();
        pitch = FindObjectOfType<Style_Pitch>();
        roll = FindObjectOfType<Style_Roll>();

        trickDisplay = FindObjectOfType<Style_TrickDisplay>();

        gizmoVisualization = FindObjectOfType<Style_GizmoVisualization>();
    }

    private void OnRoundEnded()
    {
        isPlayerSpawned = false;
        OnLand();
        trickDisplay.DestroyComponent();
        StopAllCoroutines();
    }

    private void OnDisconnectedFromGame()
    {
        isPlayerSpawned = false;
        OnLand();
        trickDisplay.DestroyComponent();
        StopAllCoroutines();
    }

    private void OnCrossedFinishLine(float time)
    {
        isDead = true;
        OnLand();
        trickDisplay.DestroyComponent();
        StopAllCoroutines();
    }

    private void OnCrashed(CrashReason reason)
    {
        isDead = true;
        OnLand();
        trickDisplay.DestroyComponent();
        StopAllCoroutines();
    }

    private void OnQuickReset()
    {
        isPlayerSpawned = false;
        OnLand();
        trickDisplay.DestroyComponent();
        Plugin.Logger.LogInfo("Player quick reset");
        StopAllCoroutines();
    }

    private void OnQuit()
    {
        isPlayerSpawned = false;
        OnLand();
        trickDisplay.DestroyComponent();
        Plugin.Logger.LogInfo("Player quited");
        StopAllCoroutines();
    }

    private void OnPlayerSpawned()
    {
        isPlayerSpawned = true;
        OnLand();
        isDead = false;
        rb = PatchGetRB.Rb;
        trickDisplay.CreateDisplay();
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
            StopCoroutine(hideTextOnAirCoroutine);
        }
        if (hideTextOnLandCoroutine != null)
        {
            StopCoroutine(hideTextOnLandCoroutine);
        }
        hideTextOnLandCoroutine = StartCoroutine(trickDisplay.HideTextAfterSeconds(2));
        tricksList.Clear();   // Clear the list of tricks
        trickDisplay.displayTextList.Clear();

        // gizmoVisualization.CleanupAxisVisuals();
        // gizmoVisualization.CleanupReferencePlanes();
    }

    // Method to detect if the rigidbody has left the ground
    void OnLeaveGround()
    {
        isInAir = true;
        initialRotation = rb.rotation;
        initialUp = Vector3.up;
        initialForward = Vector3.ProjectOnPlane(rb.transform.forward, initialUp);
        initialRight = Vector3.Cross(initialUp, initialForward).normalized;

        yaw.OnLeaveGround(initialUp,initialForward,initialRight);
        pitch.OnLeaveGround(initialUp,initialForward,initialRight);
        roll.OnLeaveGround(initialUp,initialForward,initialRight);

        trickDisplay.ResetText();
        
        if (hideTextOnLandCoroutine != null)
        {
            StopCoroutine(hideTextOnLandCoroutine);
        }

        // gizmoVisualization.CreateAxisVisuals(rb);
        // gizmoVisualization.CreateReferencePlanes(initialRotation, rb);
    }

    void Update()
    {
        if (isPlayerSpawned && !isDead)
        {
            isInAir = PatchAreAllWheelsInAir.IsInTheAir;
            
            if (isInAir)
            {
                rb = PatchGetRB.Rb;
                currentRight = rb.transform.right;
                currentForward = rb.transform.forward;
                currentUp = rb.transform.up;

                if (!wasInAir)
                {
                    OnLeaveGround();
                    Plugin.Logger.LogInfo("Player is Airborne!");
                }

                // Tricks
                yaw.DetectSpinTrick(currentForward,currentUp);
                pitch.DetectFlipTrick(currentForward, currentRight);
                roll.DetectRollTrick(currentUp,currentForward);

                // gizmoVisualization.UpdateAllAxisVisuals(rb);
                // gizmoVisualization.UpdatePlanePositions(rb);
            }
            else if(wasInAir){
                OnLand();
                Plugin.Logger.LogInfo("Player is no longer Airborne!");
            }
            wasInAir = isInAir; 
        } 
    }
}
public class Trick
{
    public string trickName;
    public string rotation;
    public bool isInverse;
    public bool isPositiveDelta;
}
