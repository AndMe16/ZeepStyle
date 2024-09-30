using UnityEngine;
using ZeepStyle;

public class Style_ZoverZYPlane : MonoBehaviour
{
    // Flip (Pitch)
    private Vector3 initialRightDirection; // Reference X-axis direction
    private Quaternion initialRotation; // Store the initial rotation as a quaternion
    private float accumulatedFlipAngle; // To accumulate the rotation angle around the X-axis
    private readonly float flipThreshold = 90.0f; // Detect each 90º flip
    private readonly float flipAlignmentThreshold = 0.6f; // Threshold for X-axis alignment (dot product close to 1 = straight)
    private int flipCount = 0;
    private float lastFlipAngleDelta; // To track the direction of the previous flip angle delta

    public void ClearVars()
    {
        accumulatedFlipAngle = 0;
        flipCount = 0;
        lastFlipAngleDelta = 0;
    }

    public void OnLeaveGround(Rigidbody rb)
    {
        initialRightDirection = rb.transform.right; // Capture the reference X-axis direction
        initialRotation = rb.transform.localRotation; // Capture the initial rotation as a quaternion
        accumulatedFlipAngle = 0;
        flipCount = 0;
        lastFlipAngleDelta = 0; // Initialize the flip angle delta
    }

    public void DetectFlipTrick(Rigidbody rb)
    {
        // Get the delta quaternion between current and initial rotation
        Quaternion deltaRotation = rb.transform.localRotation * Quaternion.Inverse(initialRotation);

        // Extract the pitch angle around the X-axis from the delta quaternion
        // This will give the rotation angle in degrees around the local X-axis
        //float flipAngle = Quaternion.Angle(Quaternion.identity, deltaRotation);
        float flipAngle = GetPitchDeltaFromQuaternion(deltaRotation);

        int alignmentState = CheckFlipAlignment(rb);

        if(alignmentState == 0 || alignmentState == 1)
        {
            Plugin.Logger.LogInfo($"Current flip angle: {flipAngle}");

            // // Check if the flip direction has changed
            // if (Mathf.Sign(flipAngle) != Mathf.Sign(lastFlipAngleDelta) && Mathf.Abs(lastFlipAngleDelta) > 0)
            // {
            //     // Direction changed, reset flip counter
            //     Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
            //     accumulatedFlipAngle = 0;
            //     flipCount = 0;
            // }

            // Accumulate the flip rotation
            accumulatedFlipAngle += flipAngle - lastFlipAngleDelta; // Add the change in flip angle

            // Check if we have completed a 90º increment of flip
            if (Mathf.Abs(accumulatedFlipAngle) >= flipThreshold)
            {
                flipCount++;
                accumulatedFlipAngle = 0; // Reset accumulated pitch for the next 90º increment

                if (alignmentState == 0)
                {
                    Plugin.Logger.LogInfo("Completed a 90º Flip! Total Flips: " + flipCount);
                }
                else
                {
                    Plugin.Logger.LogInfo("Completed an inverse 90º Flip! Total Flips: " + flipCount);
                }
            }
        }
        else
        {
            accumulatedFlipAngle = 0;
            flipCount = 0;
        }
    

        // Store the flip angle delta for the next frame
        lastFlipAngleDelta = flipAngle;
    }

    private int CheckFlipAlignment(Rigidbody rb)
    {
        // Check if the player is sufficiently tilted relative to the initial reference
        Vector3 currentRightDirection = rb.transform.right; // Current X-axis direction of the rigidbody

        // Compute the dot product between the current and reference X-axis directions
        float alignment = Vector3.Dot(currentRightDirection, initialRightDirection);

        if (Mathf.Abs(alignment) < flipAlignmentThreshold)
        {
            return 2; // Skip flip detection if the player is not straight
        }
        if (alignment < 0)
        {
            return 1; // Flipping backwards
        }
        return 0; // Spinning normally
    }
    private float GetPitchDeltaFromQuaternion(Quaternion deltaRotation)
    {
        // Extract the pitch (X-axis rotation) delta directly from quaternion
        float angle;  
        angle = deltaRotation.eulerAngles.x;
        Plugin.Logger.LogInfo($"Angle:{angle}");
        return angle; // Multiply by sign to track the direction
    }
}