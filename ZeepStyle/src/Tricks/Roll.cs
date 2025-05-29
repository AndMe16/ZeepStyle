using UnityEngine;
using ZeepStyle.PointsManager;
using ZeepStyle.SoundEffectsManager;
using ZeepStyle.TrickDisplayManager;
using ZeepStyle.TrickManager;

namespace ZeepStyle.Tricks;

public class StyleRoll : MonoBehaviour
{
    private const float
        RollAlignmentThreshold = 0.5f; // Threshold for X-axis alignment (dot product close to 1 = straight)

    private const float RollThreshold = 80.0f; // Detect each 90º roll
    private float accumulatedRoll; // Accumulated roll angle

    private Vector3 initialForward; // Z-axis (forward) direction at takeoff

    // roll (roll)
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialUp; // Y-axis (up) direction at takeoff
    private float lastRollDelta; // To track the direction of the previous roll delta
    private float previousRoll;
    private Vector3 referencePlaneNormal; // Normal of the plane defined by initialRight and initialUp
    private int rollCount;
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
        accumulatedRoll = 0;
        rollCount = 0;
        lastRollDelta = 0;
    }

    public void OnLeaveGround(Vector3 initialUpIn, Vector3 initialForwardIn, Vector3 initialRightIn)
    {
        initialUp = initialUpIn;
        initialForward = initialForwardIn;
        initialRight = initialRightIn;
        referencePlaneNormal =
            Vector3.Cross(initialRight, initialUp)
                .normalized; // Normal of the plane defined by initialRight and initialUp

        previousRoll = 0;
        accumulatedRoll = 0;
        rollCount = 0;
        lastRollDelta = 0;
    }

    public bool DetectRollTrick(Vector3 currentUp, Vector3 currentForward)
    {
        // Get the current up direction (Y-axis)

        // Project current up direction onto the initial X-Y plane
        var upInXYPlane = Vector3.ProjectOnPlane(currentUp, referencePlaneNormal).normalized;

        // Compute the angle between the projected up direction and the initial up direction
        var currentRoll = Vector3.SignedAngle(initialUp, upInXYPlane, initialForward);

        if (currentRoll < 0) currentRoll = 360 + currentRoll;

        var alignmentState = CheckrollAlignment(currentForward);

        var rollDelta = Mathf.DeltaAngle(previousRoll, currentRoll);

        if (alignmentState is 0 or 1)
        {
            // Check if the roll direction has changed
            if (!Mathf.Approximately(Mathf.Sign(rollDelta), Mathf.Sign(lastRollDelta)) && Mathf.Abs(lastRollDelta) > 0)
            {
                // Direction changed, reset roll counter
                //Plugin.Logger.LogInfo("Roll direction changed! Resetting roll counter.");
                accumulatedRoll = 0;
                rollCount = 0;
            }

            // Accumulate the roll rotation
            accumulatedRoll += rollDelta;

            // Check if we have completed a 90º increment of roll
            if (Mathf.Abs(accumulatedRoll) >= RollThreshold)
            {
                rollCount++;
                accumulatedRoll = 0; // Reset accumulated roll for the next 90º increment

                if (rollCount % 2 == 0 && rollCount != 0)
                {
                    var isInverse = alignmentState != 0;
                    var isPositiveDelta = rollDelta > 0;
                    const string trickName = "Roll";
                    var rotationsStr = $"{rollCount * 90}";
                    Trick trick = new()
                    {
                        TrickName = trickName,
                        Rotation = rotationsStr,
                        IsInverse = isInverse,
                        IsPositiveDelta = isPositiveDelta
                    };
                    var points = trickPointsManager.CalculatePoints(trick);
                    trickDisplay.DisplayTrick(trick, points);
                    soundEffectManager.PlaySound("SimpleTrick_2_Sound");
                    return true; // Roll trick detected
                }
            }
        }
        else
        {
            accumulatedRoll = 0;
            rollCount = 0;
        }

        // Update the previous roll and last roll delta for the next frame
        previousRoll = currentRoll;
        lastRollDelta = rollDelta; // Store current roll delta to detect direction change
        return false; // No roll trick detected
    }

    private int CheckrollAlignment(Vector3 currentForward)
    {
        // Check if the player is sufficiently tilted relative to the initial reference

        // Compute the dot product between the current and reference X-axis directions
        var alignment = Vector3.Dot(currentForward, initialForward);

        if (Mathf.Abs(alignment) <
            RollAlignmentThreshold) return 2; // Skip roll detection if the player is not straight
        return alignment < 0
            ? 1
            : // rolling backwards
            0; // rolling normally
    }
}