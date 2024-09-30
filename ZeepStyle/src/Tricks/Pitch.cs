using UnityEngine;
using ZeepStyle;

public class Style_Pitch : MonoBehaviour
{
    // Flip (Pitch)
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialForward; // Z-axis (forward) direction at takeoff
    private Vector3 initialUp; // Y-axis (up) direction at takeoff
    private float accumulatedPitch = 0; // Accumulated pitch angle
    private float previousPitch = 0;
    private readonly float flipThreshold = 90.0f; // Detect each 90º flip
    private readonly float flipAlignmentThreshold = 0.6f; // Threshold for X-axis alignment (dot product close to 1 = straight)
    private int flipCount = 0;
    private float lastPitchDelta; // To track the direction of the previous pitch delta


    public void ClearVars()
    {
        accumulatedPitch = 0;
        flipCount = 0;
        lastPitchDelta = 0;
    }

    public void OnLeaveGround(UnityEngine.Vector3 initialUp_,UnityEngine.Vector3 initialForward_, UnityEngine.Vector3 initialRight_)
    {
        initialUp = initialUp_;
        initialForward = initialForward_;
        initialRight = initialRight_;
        
        previousPitch = 0;
        accumulatedPitch = 0;
        flipCount = 0;
        lastPitchDelta = 0;
    }

    public void DetectFlipTrick(Vector3 currentForward_, Vector3 currentRight_)
    {
        // Get the current forward direction (Z-axis)
        Vector3 currentForward = currentForward_;

        // Project current forward direction onto the initial Z-Y plane
        Vector3 forwardInZYPlane = Vector3.ProjectOnPlane(currentForward, Vector3.Cross(initialForward, initialUp));

        // Compute the angle between the projected forward direction and the initial forward direction
        float currentPitch = Vector3.SignedAngle(initialForward, forwardInZYPlane, initialRight);

        if (currentPitch<0)
        {
            currentPitch = 360 + currentPitch;
        }

        int alignmentState = CheckFlipAlignment(currentRight_);

        float pitchDelta = Mathf.DeltaAngle(previousPitch,currentPitch);

        if(alignmentState == 0 || alignmentState == 1)
        {
            // Check if the spin direction has changed
            if (Mathf.Sign(pitchDelta) != Mathf.Sign(lastPitchDelta) && Mathf.Abs(lastPitchDelta) > 0)
            {
                // Direction changed, reset flip counter
                Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
                accumulatedPitch = 0;
                flipCount = 0;
            }

            // Accumulate the pitch rotation
            accumulatedPitch += pitchDelta;

            // Check if we have completed a 90º increment of flip
            if (Mathf.Abs(accumulatedPitch) >= flipThreshold)
            {
                flipCount++;
                accumulatedPitch = 0; // Reset accumulated pitch for the next 90º increment
                if (alignmentState ==0){
                    // Trigger the flip detection (you can add points, log it, etc.)
                    if (pitchDelta>0)
                    {
                        Plugin.Logger.LogInfo("Completed a 90º Front Flip! Total Flips: " + flipCount);
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Completed a 90º Back Flip! Total Flips: " + flipCount);
                    }
                }
                else{
                    if (pitchDelta>0)
                    {
                        Plugin.Logger.LogInfo("Completed a 90º reverse BackFlip! Total Flips: " + flipCount);
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Completed a 90º reverse Front Flip! Total Flips: " + flipCount);
                    }
                }  
            }
        }
        else{
            accumulatedPitch = 0;
            flipCount = 0;
        }

        // Update the previous pitch and last pitch delta for the next frame
        previousPitch = currentPitch;
        lastPitchDelta = pitchDelta; // Store current pitch delta to detect direction change
    }

    private int CheckFlipAlignment(Vector3 currentRight_)
    {
        // Check if the player is sufficiently tilted relative to the initial reference
        Vector3 currentRight = currentRight_; // Current X-axis direction of the rigidbody

        // Compute the dot product between the current and reference X-axis directions
        float alignment = Vector3.Dot(currentRight, initialRight);

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
}