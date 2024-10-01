using UnityEngine;
using ZeepStyle;

public class Style_Yaw : MonoBehaviour
{
    // Spin (Yaw)
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialForward; // Z-axis (forward) direction at takeoff
    private Vector3 initialUp; // Y-axis (up) direction at takeoff
    private float previousYaw; // To track the Y-axis (yaw) rotation
    private float accumulatedYaw; // To accumulate yaw rotation
    private readonly float spinThreshold = 80.0f; // Detect each 90º spin
    private readonly float spinAlignmentThreshold = 0.5f; // Threshold for Y-axis alignment (dot product close to 1 = upright)
    private int spinCount = 0;
    private float lastYawDelta; // To track the direction of the previous yaw delta

    public void ClearVars()
    {
        accumulatedYaw = 0;
        spinCount = 0;
        lastYawDelta = 0;
    }


    public void OnLeaveGround(Vector3 initialUp_,Vector3 initialForward_, Vector3 initialRight_)
    {
        initialUp = initialUp_;
        initialForward = initialForward_;
        initialRight = initialRight_;

        previousYaw = 0; // Capture the initial yaw (Y-axis) rotation
        accumulatedYaw = 0;
        spinCount = 0;
        lastYawDelta = 0; // Initialize the yaw delta
    }

    public void DetectSpinTrick(Vector3 currentForward_, Vector3 currentUp_)
    {
        // Get the current forward direction (Z-axis)
        Vector3 currentForward = currentForward_;

        // Project current forward direction onto the initial X-Z plane
        Vector3 forwardInXZPlane = Vector3.ProjectOnPlane(currentForward, Vector3.Cross(initialRight, initialForward)); 
        
        // Compute the angle between the projected forward direction and the initial forward direction
        float currentYaw = Vector3.SignedAngle(initialForward, forwardInXZPlane, initialUp);

        if (currentYaw<0)
        {
            currentYaw = 360 + currentYaw;
        }
        
        int alignmentState = CheckSpinAlignment(currentUp_);
        
        float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);
        if(alignmentState == 0 || alignmentState == 1)
        {
            // Check if the spin direction has changed
            if (Mathf.Sign(yawDelta) != Mathf.Sign(lastYawDelta) && Mathf.Abs(lastYawDelta) > 0)
            {
                // Direction changed, reset spin counter
                // Plugin.Logger.LogInfo("Spin direction changed! Resetting spin counter.");
                accumulatedYaw = 0;
                spinCount = 0;
            }

            // Accumulate the yaw rotation
            accumulatedYaw += yawDelta;

            // Check if we have completed a 90º increment of spin
            if (Mathf.Abs(accumulatedYaw) >= spinThreshold)
            {
                spinCount++;
                accumulatedYaw = 0; // Reset accumulated yaw for the next 90º increment
                // if (alignmentState ==0){
                //     // Trigger the spin detection (you can add points, log it, etc.)
                //     Plugin.Logger.LogInfo("Completed a 90º Spin! Total Spins: " + spinCount);
                // }
                // else{
                //     // Trigger the spin detection (you can add points, log it, etc.)
                //     Plugin.Logger.LogInfo("Completed an inverse 90º Spin! Total Spins: " + spinCount);
                // }  
            }
        }
        else{
            accumulatedYaw = 0;
            spinCount = 0;
        }

        // Update the previous yaw and last yaw delta for the next frame
        previousYaw = currentYaw;
        lastYawDelta = yawDelta; // Store current yaw delta to detect direction change

        // Display Trick Names
        if (((spinCount % 2) == 0) && spinCount!=0)
        {
            string trickName;
            if (alignmentState ==0){
                trickName = $"{spinCount*90} Spin";
            }
            else{
                trickName = $"Inverse {spinCount*90} Spin";
            }
            
            FindObjectOfType<Style_TrickDisplay>().DisplayTrick(trickName);
        }
    }

    private int CheckSpinAlignment(Vector3 currentUp_)
    {
        // Check if the player is sufficiently tilted relative to the initial reference
        Vector3 currentUpDirection = currentUp_; // Current Y-axis direction of the rigidbody

        // Compute the dot product between the current and reference Y-axis directions
        float alignment = Vector3.Dot(currentUpDirection, initialUp);

        if (Mathf.Abs(alignment) < spinAlignmentThreshold)
        {
            return 2; // Skip spin detection if the player is not upright
        }
        if(alignment < 0)
        {
            return 1; // Spining upside down
        }
        return 0; // Spining normally
    }
}