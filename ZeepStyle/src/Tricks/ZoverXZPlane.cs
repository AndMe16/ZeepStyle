using UnityEngine;
using ZeepStyle;

public class Style_ZoverXZPlane : MonoBehaviour
{
    // Spin (Yaw)
    private Vector3 initialUpDirection; // Reference Y-axis direction
    private float previousYaw; // To track the Y-axis (yaw) rotation
    private float accumulatedYaw; // To accumulate yaw rotation
    private readonly float spinThreshold = 90.0f; // Detect each 90º spin
    private readonly float spinAlignmentThreshold = 0.6f; // Threshold for Y-axis alignment (dot product close to 1 = upright)
    private int spinCount = 0;
    private float lastYawDelta; // To track the direction of the previous yaw delta

    public void ClearVars()
    {
        accumulatedYaw = 0;
        spinCount = 0;

        lastYawDelta = 0;
    }


    public void OnLeaveGround(Rigidbody rb)
    {
        initialUpDirection = Vector3.up;
        previousYaw = rb.transform.localEulerAngles.y; // Capture the initial yaw (Y-axis) rotation
        accumulatedYaw = 0;
        spinCount = 0;
        lastYawDelta = 0; // Initialize the yaw delta
    }

    public void DetectSpinTrick(Rigidbody rb)
    {
        // Get current yaw (Y-axis rotation)
        float currentYaw = rb.transform.localEulerAngles.y;
        
        int alignmentState = CheckSpinAlignment(rb);
        // Calculate the yaw difference since the last frame
        float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);
        if(alignmentState == 0 || alignmentState == 1)
        {
            // Check if the spin direction has changed
            if (Mathf.Sign(yawDelta) != Mathf.Sign(lastYawDelta) && Mathf.Abs(lastYawDelta) > 0)
            {
                // Direction changed, reset spin counter
                Plugin.Logger.LogInfo("Spin direction changed! Resetting spin counter.");
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
                if (alignmentState ==0){
                    // Trigger the spin detection (you can add points, log it, etc.)
                    Plugin.Logger.LogInfo("Completed a 90º Spin! Total Spins: " + spinCount);
                }
                else{
                    // Trigger the spin detection (you can add points, log it, etc.)
                    Plugin.Logger.LogInfo("Completed an inverse 90º Spin! Total Spins: " + spinCount);
                }  
            }
        }
        else{
            accumulatedYaw = 0;
            spinCount = 0;
        }

        // Update the previous yaw and last yaw delta for the next frame
        previousYaw = currentYaw;
        lastYawDelta = yawDelta; // Store current yaw delta to detect direction change
    }

    private int CheckSpinAlignment(Rigidbody rb)
    {
        // Check if the player is sufficiently tilted relative to the initial reference
        Vector3 currentUpDirection = rb.transform.up; // Current Y-axis direction of the rigidbody

        // Compute the dot product between the current and reference Y-axis directions
        float alignment = Vector3.Dot(currentUpDirection, initialUpDirection);

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