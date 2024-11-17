using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.TrickDisplayManager;
using ZeepStyle.src.TrickManager;

namespace ZeepStyle.src.Tricks
{
    public class Style_Yaw : MonoBehaviour
    {
        // Spin (Yaw)
        private Vector3 initialRight; // Reference X-axis direction
        private Vector3 initialForward; // Z-axis (forward) direction at takeoff
        private Vector3 initialUp; // Y-axis (up) direction at takeoff
        private float previousYaw; // To track the Y-axis (yaw) rotation
        private float accumulatedYaw; // To accumulate yaw rotation
        private readonly float spinThreshold = 80.0f; // Detect each 90º spin
        private readonly float spinAlignmentThreshold = 0.3f; // Threshold for Y-axis alignment (dot product close to 1 = upright)
        private int spinCount = 0;
        private float lastYawDelta; // To track the direction of the previous yaw delta
        public Queue<float> spinSpeedBuffer = new Queue<float>();
        private const int bufferSize = 10; // Number of frames to average
        private bool soundPlayed = false; // To ensure the sound is only triggered once
        private const float playThreshold = 600f; // Speed at which the sound starts playing
        private const float volumeThreshold = 600f; // Speed at which volume scaling starts
        private const float maxVolumeSpeed = 800f; // Speed at which volume reaches maximum

        Style_TrickDisplay trickDisplay;
        Style_TrickPointsManager trickPointsManager;
        Style_SoundEffectManager soundEffectManager;

        void Start()
        {
            trickDisplay = FindObjectOfType<Style_TrickDisplay>();
            trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();
            soundEffectManager = FindObjectOfType<Style_SoundEffectManager>();
        }

        public void ClearVars()
        {
            accumulatedYaw = 0;
            spinCount = 0;
            lastYawDelta = 0;
        }


        public void OnLeaveGround(Vector3 initialUp_, Vector3 initialForward_, Vector3 initialRight_)
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

            // Project current forward direction onto the initial X-Z plane
            Vector3 forwardInXZPlane = Vector3.ProjectOnPlane(currentForward_, Vector3.Cross(initialRight, initialForward));

            // Compute the angle between the projected forward direction and the initial forward direction
            float currentYaw = Vector3.SignedAngle(initialForward, forwardInXZPlane, initialUp);

            if (currentYaw < 0)
            {
                currentYaw = 360 + currentYaw;
            }

            int alignmentState = CheckSpinAlignment(currentUp_);

            float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);

            // Calculate spin speed (degrees per second)
            float spinSpeed = Mathf.Abs(yawDelta / Time.deltaTime);

            // Add spin speed to the buffer
            spinSpeedBuffer.Enqueue(spinSpeed);

            // Remove the oldest speed if buffer exceeds the size
            if (spinSpeedBuffer.Count > bufferSize)
            {
                spinSpeedBuffer.Dequeue();
            }

            // Calculate the moving average speed
            float averageSpinSpeed = spinSpeedBuffer.Average();

            // Check if average spin speed exceeds the play threshold
            if (averageSpinSpeed > playThreshold)
            {
                if (!soundPlayed)
                {
                    soundEffectManager.PlaySound("HighSpeedSpin_Sound");
                    soundPlayed = true; // Mark sound as played
                }

                // Adjust volume based on speed
                if (averageSpinSpeed > volumeThreshold)
                {
                    float normalizedSpeed = Mathf.Clamp(averageSpinSpeed, volumeThreshold, maxVolumeSpeed);
                    float volume = (normalizedSpeed - volumeThreshold) / (maxVolumeSpeed - volumeThreshold);
                    soundEffectManager.SetSoundVolume("HighSpeedSpin_Sound", volume);
                }
            }
            else
            {
                if (soundPlayed)
                {
                    // Reset sound trigger if speed falls below the play threshold
                    soundPlayed = false;
                    soundEffectManager.StopSound("HighSpeedSpin_Sound"); // Mute sound below threshold
                }
                
            }

            if (alignmentState == 0 || alignmentState == 1)
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
                                        // Display Trick Names
                    if ((spinCount % 2) == 0 && (spinCount != 0))
                    {
                        string trickName;
                        bool isInverse;
                        bool isPositiveDelta;
                        if (alignmentState == 0)
                        {
                            isInverse = false;
                        }
                        else
                        {
                            isInverse = true;
                        }

                        if (yawDelta > 0)
                        {
                            isPositiveDelta = true;
                        }
                        else
                        {
                            isPositiveDelta = false;
                        }

                        trickName = "Spin";
                        string rotations_str = $"{spinCount * 90}";
                        Trick trick = new()
                        {
                            trickName = trickName,
                            rotation = rotations_str,
                            isInverse = isInverse,
                            isPositiveDelta = isPositiveDelta
                        };
                        int points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_1_Sound");
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
            if (alignment < 0)
            {
                return 1; // Spining upside down
            }
            return 0; // Spining normally
        }
    }
}