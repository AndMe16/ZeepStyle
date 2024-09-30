using UnityEngine;
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

    // Quaternions
    private Quaternion initialRotation;

    // Debuging with gizmo visualization
    Style_GizmoVisualization gizmoVisualization;

    // Type of rotation
    Style_ZoverXZPlane zoverXZPlane;
    Style_ZoverZYPlane zoverZYPlane;

    void Start()
    {
        RacingApi.PlayerSpawned += OnPlayerSpawned;
        RacingApi.Quit += OnQuit;
        RacingApi.QuickReset += OnQuickReset;
        RacingApi.Crashed += OnCrashed;
        RacingApi.CrossedFinishLine += OnCrossedFinishLine;

        zoverXZPlane = FindObjectOfType<Style_ZoverXZPlane>();
        zoverZYPlane = FindObjectOfType<Style_ZoverZYPlane>();

        gizmoVisualization = FindObjectOfType<Style_GizmoVisualization>();
    }

    private void OnCrossedFinishLine(float time)
    {
        isDead = true;
        OnLand();
    }

    private void OnCrashed(CrashReason reason)
    {
        isDead = true;
        OnLand();
    }

    private void OnQuickReset()
    {
        isPlayerSpawned = false;
        OnLand();
        Plugin.Logger.LogInfo("Player quick reset");
    }

    private void OnQuit()
    {
        isPlayerSpawned = false;
        OnLand();
        Plugin.Logger.LogInfo("Player quited");
    }

    private void OnPlayerSpawned()
    {
        isPlayerSpawned = true;
        OnLand();
        isDead = false;
        rb = PatchGetRB.Rb;
        Plugin.Logger.LogInfo("Player spawned");
    }

    // Method to detect if the rigidbody has landed back on the ground
    void OnLand()
    {
        isInAir = false;  // Reset state when landing
        wasInAir = false; 

        zoverXZPlane.ClearVars();
        zoverZYPlane.ClearVars();
        

        gizmoVisualization.CleanupAxisVisuals();
        gizmoVisualization.CleanupReferencePlanes();
    }

    // Method to detect if the rigidbody has left the ground
    void OnLeaveGround()
    {
        isInAir = true;
        initialRotation = rb.rotation;
        
        zoverXZPlane.OnLeaveGround(rb);
        zoverZYPlane.OnLeaveGround(rb);

        gizmoVisualization.CreateAxisVisuals(rb);
        gizmoVisualization.CreateReferencePlanes(initialRotation, rb);
    }

    void Update()
    {
        if (isPlayerSpawned && !isDead)
        {
            isInAir = PatchAreAllWheelsInAir.IsInTheAir;
            
            if (isInAir)
            {
                rb = PatchGetRB.Rb;
                if (!wasInAir)
                {
                    OnLeaveGround();
                    Plugin.Logger.LogInfo("Player is Airborne!");
                }

                // Tricks
                zoverXZPlane.DetectSpinTrick(rb);
                zoverZYPlane.DetectFlipTrick(rb);

                gizmoVisualization.UpdateAllAxisVisuals(rb);
                gizmoVisualization.UpdatePlanePositions(rb);
            }
            else if(wasInAir){
                OnLand();
                Plugin.Logger.LogInfo("Player is no longer Airborne!");
            }
            wasInAir = isInAir; 
        } 
    }
    
}
