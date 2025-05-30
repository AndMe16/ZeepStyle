using UnityEngine;
using ZeepStyle.PointsManager;
using ZeepStyle.SoundEffectsManager;
using ZeepStyle.TrickDisplayManager;
using ZeepStyle.TrickManager;

namespace ZeepStyle.Tricks;

public class StylePitch : MonoBehaviour
{
    private const float
        FlipAlignmentThreshold =
            0.7f; // Threshold for X-axis alignment with right direction (dot product close to 1 = straight)

    private const float FlipThreshold = 80.0f; // Detect each 90º flip

    private const float
        SideFlipAlignmentThreshold =
            0.8f; // Threshold for X-axis alignment with an up direction (dot product close to 1 = side)

    private const float SideflipThreshold = 80.0f; // Detect each 90º sideflip
    
    private const float FlipMagnitudeThreshold = 0.3f;
    
    
    private float accumulatedPitchFlip; // Accumulated pitch angle for normal flips
    private float accumulatedPitchSideflip; // Accumulated pitch angle for side flips
    private int flipCount;

    private Vector3 initialForward; // Z-axis (forward) direction at takeoff

    // Flip (Pitch)
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialUp; // Y-axis (up) direction at takeoff
    private float lastPitchDelta; // To track the direction of the previous pitch delta
    private float previousPitch;
    private Vector3 referencePlaneNormal; // Normal of the plane defined by initialForward and initialUp
    private int sideflipCount;
    private StyleSoundEffectManager soundEffectManager;

    private StyleTrickDisplay trickDisplay;
    private StyleTrickPointsManager trickPointsManager;

    private void Start()
    {
        trickDisplay = FindObjectOfType<StyleTrickDisplay>();
        trickPointsManager = FindObjectOfType<StyleTrickPointsManager>();
        soundEffectManager = FindObjectOfType<StyleSoundEffectManager>();
    }

    public void ClearVars()
    {
        accumulatedPitchFlip = 0;
        accumulatedPitchSideflip = 0;
        flipCount = 0;
        sideflipCount = 0;
        lastPitchDelta = 0;
    }

    public void OnLeaveGround(Vector3 initialUpIn, Vector3 initialForwardIn, Vector3 initialRightIn)
    {
        initialUp = initialUpIn;
        initialForward = initialForwardIn;
        initialRight = initialRightIn;
        referencePlaneNormal =
            Vector3.Cross(initialForward, initialUp)
                .normalized; // Normal of the plane defined by initialForward and initialUp

        previousPitch = 0;
        accumulatedPitchFlip = 0;
        accumulatedPitchSideflip = 0;
        flipCount = 0;
        sideflipCount = 0;
        lastPitchDelta = 0;
    }

    public bool DetectFlipTrick(Vector3 currentForward, Vector3 currentRight, Vector3 currentUp)
    {
        // Get the current forward direction (Z-axis)

        // Project current forward direction onto the initial Z-Y plane
        var forwardInZyPlane = Vector3.ProjectOnPlane(currentForward.normalized, referencePlaneNormal);
        
        var generalFlipAlignmentState = CheckGeneralFlipAlignment(currentForward, forwardInZyPlane);

        if (generalFlipAlignmentState is 0 or 1)
        {
            forwardInZyPlane = forwardInZyPlane.normalized;

            // Compute the angle between the projected forward direction and the initial forward direction
            var currentPitch = Vector3.SignedAngle(initialForward, forwardInZyPlane, initialRight);

            if (currentPitch < 0) currentPitch = 360 + currentPitch;

            var flipAlignmentState = CheckFlipAlignment(currentRight);
            var sideflipAlignmentState = CheckSideFlipAlignment(currentUp);

            var pitchDelta = Mathf.DeltaAngle(previousPitch, currentPitch);

            if (flipAlignmentState is 0 or 1)
            {
                // Check if the spin direction has changed
                if (!Mathf.Approximately(Mathf.Sign(pitchDelta), Mathf.Sign(lastPitchDelta)) &&
                    Mathf.Abs(lastPitchDelta) > 0)
                {
                    // Direction changed, reset flip counter
                    //Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
                    accumulatedPitchFlip = 0;
                    flipCount = 0;
                }

                // Accumulate the pitch rotation
                accumulatedPitchFlip += pitchDelta;

                // Check if we have completed a 90º increment of flip
                if (Mathf.Abs(accumulatedPitchFlip) >= FlipThreshold)
                {
                    flipCount++;
                    accumulatedPitchFlip = 0; // Reset accumulated pitch for the next 90º increment

                    if (flipCount % 4 == 0 && flipCount != 0)
                    {
                        string trickName;
                        bool isPositiveDelta;
                        var isInverse = flipAlignmentState != 0;
                        if (pitchDelta > 0)
                        {
                            isPositiveDelta = true;
                            trickName = "Frontflip";
                        }
                        else
                        {
                            isPositiveDelta = false;
                            trickName = "Backflip";
                        }

                        var rotationsStr = $"{flipCount / 4}";
                        Trick trick = new()
                        {
                            TrickName = trickName,
                            Rotation = rotationsStr,
                            IsInverse = isInverse,
                            IsPositiveDelta = isPositiveDelta
                        };
                        var points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_3_Sound");
                        return true; // Return true to indicate a flip trick was detected
                    }
                }
            }
            else
            {
                accumulatedPitchFlip = 0;
                flipCount = 0;
            }

            if (sideflipAlignmentState is 0 or 1)
            {
                // Check if the spin direction has changed
                if (!Mathf.Approximately(Mathf.Sign(pitchDelta), Mathf.Sign(lastPitchDelta)) &&
                    Mathf.Abs(lastPitchDelta) > 0)
                {
                    // Direction changed, reset flip counter
                    //Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
                    accumulatedPitchSideflip = 0;
                    sideflipCount = 0;
                }

                // Accumulate the pitch rotation
                accumulatedPitchSideflip += pitchDelta;

                // Check if we have completed a 90º increment of flip
                if (Mathf.Abs(accumulatedPitchSideflip) >= SideflipThreshold)
                {
                    sideflipCount++;
                    accumulatedPitchSideflip = 0; // Reset accumulated pitch for the next 90º increment

                    if (sideflipCount % 4 == 0 && sideflipCount != 0)
                    {
                        const string trickName = "Sideflip";
                        const bool isPositiveDelta = true;
                        var isInverse = !(pitchDelta > 0);
                        var rotationsStr = $"{sideflipCount / 4}";
                        Trick trick = new()
                        {
                            TrickName = trickName,
                            Rotation = rotationsStr,
                            IsInverse = isInverse,
                            IsPositiveDelta = isPositiveDelta
                        };
                        var points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_3_Sound");
                        return true; // Return true to indicate a flip trick was detected
                    }
                }
            }
            else
            {
                accumulatedPitchSideflip = 0;
                sideflipCount = 0;
            }
            
            // Update the previous pitch and last pitch delta for the next frame
            previousPitch = currentPitch;
            lastPitchDelta = pitchDelta; // Store current pitch delta to detect direction change
        }
        else
        {
            previousPitch = 0;
            lastPitchDelta = 0;
        }

        return false; // No flip trick detected
    }

    private int CheckFlipAlignment(Vector3 currentRight)
    {
        // Check if the player is sufficiently tilted relative to the initial reference

        // Compute the dot product between the current right and reference X-axis directions
        var alignment = Vector3.Dot(currentRight, initialRight);

        if (Mathf.Abs(alignment) <
            FlipAlignmentThreshold) return 2; // Skip flip detection if the player is not straight
        return alignment < 0
            ? 1
            : // Flipping backwards
            0; // Spinning normally
    }

    private int CheckSideFlipAlignment(Vector3 currentUp)
    {
        // Check if the player is sufficiently tilted relative to the initial reference

        // Compute the dot product between the current up and reference X-axis directions
        var alignment = Vector3.Dot(currentUp, initialRight);

        if (Mathf.Abs(alignment) <
            SideFlipAlignmentThreshold) return 2; // Skip sideflip detection if the player is not sideways

        return alignment < 0
            ? 1
            : // Sidefliping 
            0; // Sidefliping
    }

    private int CheckGeneralFlipAlignment(Vector3 currentForward, Vector3 alignmentReference)
    {
        // Check if the player is sufficiently tilted relative to the initial referencePlane

        // Project the currentForward to the Initial referencePlane to get the reference alignment vector
        var magnitude = alignmentReference.magnitude;

        if (magnitude < FlipMagnitudeThreshold)
        {
            return 2;
        }
        // Compute the dot product between the current up and reference X-axis directions
        var alignment = Vector3.Dot(currentForward, alignmentReference);
        
        if (Mathf.Abs(alignment) <
            FlipAlignmentThreshold) return 2; // Skip flip detection if the player is not straight
        return alignment < 0
            ? 1
            : // Flipping backwards
            0; // Flipping normally
        
    }
    
}