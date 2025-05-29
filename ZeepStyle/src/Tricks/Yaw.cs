using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeepStyle.PointsManager;
using ZeepStyle.SoundEffectsManager;
using ZeepStyle.TrickDisplayManager;
using ZeepStyle.TrickManager;

namespace ZeepStyle.Tricks;

public class StyleYaw : MonoBehaviour
{
    private const int BufferSize = 10; // Number of frames to average
    private const float PlayThreshold = 600f; // Speed at which the sound starts playing
    private const float VolumeThreshold = 600f; // Speed at which volume scaling starts
    private const float MaxVolumeSpeed = 700f; // Speed at which volume reaches maximum
    private const float MaxReasonableSpinSpeed = 800f; // Maximum reasonable spin speed (degrees per second)

    private const float
        SpinAlignmentThreshold = 0.4f; // Threshold for Y-axis alignment (dot product close to 1 = upright)

    private const float SpinThreshold = 80.0f; // Detect each 90º spin after the first spinThreshold degrees
    public readonly Queue<float> SpinSpeedBuffer = new();
    private float accumulatedYaw; // To accumulate yaw rotation

    private Vector3 initialForward; // Z-axis (forward) direction at takeoff

    // Spin (Yaw)
    private Vector3 initialRight; // Reference X-axis direction
    private Vector3 initialUp; // Y-axis (up) direction at takeoff
    private float lastYawDelta; // To track the direction of the previous yaw delta
    private float previousYaw; // To track the Y-axis (yaw) rotation
    private Vector3 referencePlaneNormal; // Normal of the plane defined by initialRight and initialForward
    private StyleSoundEffectManager soundEffectManager;
    private bool soundPlayed; // To ensure, the sound is only triggered once
    private int spinCount;

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
        accumulatedYaw = 0;
        spinCount = 0;
        lastYawDelta = 0;
        soundPlayed = false; // Reset sound state
        SpinSpeedBuffer.Clear(); // Clear the spin speed buffer
    }


    public void OnLeaveGround(Vector3 initialUpIn, Vector3 initialForwardIn, Vector3 initialRightIn)
    {
        initialUp = initialUpIn;
        initialForward = initialForwardIn;
        initialRight = initialRightIn;
        referencePlaneNormal =
            Vector3.Cross(initialRight, initialForward)
                .normalized; // Normal of the plane defined by initialRight and initialForward

        previousYaw = 0; // Capture the initial yaw (Y-axis) rotation
        accumulatedYaw = 0;
        spinCount = 0;
        lastYawDelta = 0; // Initialize the yaw delta
    }

    public bool DetectSpinTrick(Vector3 currentForward, Vector3 currentUp)
    {
        // Get the current forward direction (Z-axis)
        // Project the current forward direction onto the initial X-Z plane
        var forwardInXZPlane = Vector3.ProjectOnPlane(currentForward, referencePlaneNormal).normalized;

        // Compute the angle between the projected forward direction and the initial forward direction
        var currentYaw = Vector3.SignedAngle(initialForward, forwardInXZPlane, initialUp);

        var alignmentState = CheckSpinAlignment(currentUp);

        var yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);

        if (alignmentState is 0 or 1)
        {
            // Calculate the spin speed based on the yaw delta and time since the last frame
            var spinSpeed = 0f;
            if (Time.deltaTime > Mathf.Epsilon)
            {
                var rawSpinSpeed = Mathf.Abs(yawDelta / Time.deltaTime);

                spinSpeed = rawSpinSpeed < MaxReasonableSpinSpeed
                    ? rawSpinSpeed
                    : 0f; // Ignore unreasonable spin speeds
            }

            // HandleSpinSound(spinSpeed); // Call the sound effect manager to handle spin sound
            HandleSpinSound(spinSpeed);

            // Check if the spin direction has changed
            if (!Mathf.Approximately(Mathf.Sign(yawDelta), Mathf.Sign(lastYawDelta)) && Mathf.Abs(lastYawDelta) > 0)
            {
                // Direction changed, reset spin counter
                // Plugin.Logger.LogInfo("Spin direction changed! Resetting spin counter.");
                accumulatedYaw = 0;
                spinCount = 0;
            }

            // Accumulate the yaw rotation
            accumulatedYaw += yawDelta;

            // Check if we have completed a 90º increment of spin
            if (Mathf.Abs(accumulatedYaw) >= SpinThreshold)
            {
                spinCount++;
                accumulatedYaw = 0; // Reset accumulated yaw for the next 90º increment
                // Display Trick Names
                if (spinCount % 2 == 0 && spinCount != 0)
                {
                    var isInverse = alignmentState != 0;

                    var isPositiveDelta = yawDelta > 0;

                    const string trickName = "Spin";
                    var rotationsStr = $"{spinCount * 90}";
                    Trick trick = new()
                    {
                        TrickName = trickName,
                        Rotation = rotationsStr,
                        IsInverse = isInverse,
                        IsPositiveDelta = isPositiveDelta
                    };
                    var points = trickPointsManager.CalculatePoints(trick);
                    trickDisplay.DisplayTrick(trick, points);
                    soundEffectManager.PlaySound("SimpleTrick_1_Sound");
                    return true; // Spin detected
                }
            }
        }
        else
        {
            accumulatedYaw = 0;
            spinCount = 0;
        }

        // Update the previous yaw and last yaw delta for the next frame
        previousYaw = currentYaw;
        lastYawDelta = yawDelta; // Store current yaw delta to detect direction change
        return false; // No spin detected
    }

    private int CheckSpinAlignment(Vector3 currentUp)
    {
        // Check if the player is sufficiently tilted relative to the initial reference

        // Compute the dot product between the current and reference Y-axis directions
        var alignment = Vector3.Dot(currentUp, initialUp);

        if (Mathf.Abs(alignment) < SpinAlignmentThreshold) return 2; // Skip spin detection if the player is not upright
        return alignment < 0
            ? 1
            : // Spinning upside down
            0; // Spinning normally
    }

    private void HandleSpinSound(float spinSpeed)
    {
        if (spinSpeed != 0)
        {
            // Add spin speed to the buffer
            SpinSpeedBuffer.Enqueue(spinSpeed);

            // Remove the oldest speed if buffer exceeds the size
            if (SpinSpeedBuffer.Count > BufferSize) SpinSpeedBuffer.Dequeue();
        }

        // Calculate the moving average speed
        if (SpinSpeedBuffer.Count == 0) return; // No speeds to average
        var averageSpinSpeed = SpinSpeedBuffer.Average();

        // Check if the average spin speed exceeds the play threshold
        if (averageSpinSpeed > PlayThreshold)
        {
            if (!soundPlayed)
            {
                //Plugin.Logger.LogInfo($"Playing sound at speed: {averageSpinSpeed}");
                soundEffectManager.PlaySound("HighSpeedSpin_Sound");
                soundPlayed = true;
            }

            // Adjust volume based on speed
            if (!(averageSpinSpeed > VolumeThreshold)) return;
            //Plugin.Logger.LogInfo($"Setting volume at speed: {averageSpinSpeed}");
            var normalizedSpeed = Mathf.Clamp(averageSpinSpeed, VolumeThreshold, MaxVolumeSpeed);
            var volume = (normalizedSpeed - VolumeThreshold) / (MaxVolumeSpeed - VolumeThreshold);
            soundEffectManager.SetSoundVolume("HighSpeedSpin_Sound", volume);
        }
        else
        {
            if (!soundPlayed) return;
            //Plugin.Logger.LogInfo($"Stopping sound at speed: {averageSpinSpeed}");
            soundPlayed = false;
            soundEffectManager.StopSound("HighSpeedSpin_Sound");
        }
    }
}