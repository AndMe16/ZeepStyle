using UnityEngine;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.TrickDisplayManager;
using ZeepStyle.src.TrickManager;

namespace ZeepStyle.src.Tricks
{
    public class Style_Roll : MonoBehaviour
    {
        // roll (roll)
        private Vector3 initialRight; // Reference X-axis direction
        private Vector3 initialForward; // Z-axis (forward) direction at takeoff
        private Vector3 initialUp; // Y-axis (up) direction at takeoff
        private Vector3 referencePlaneNormal; // Normal of the plane defined by initialRight and initialUp
        private float accumulatedRoll = 0; // Accumulated roll angle
        private float previousRoll = 0;
        private readonly float rollThreshold = 80.0f; // Detect each 90º roll
        private readonly float rollAlignmentThreshold = 0.5f; // Threshold for X-axis alignment (dot product close to 1 = straight)
        private int rollCount = 0;
        private float lastRollDelta; // To track the direction of the previous roll delta

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
            accumulatedRoll = 0;
            rollCount = 0;
            lastRollDelta = 0;
        }

        public void OnLeaveGround(UnityEngine.Vector3 initialUp_, UnityEngine.Vector3 initialForward_, UnityEngine.Vector3 initialRight_)
        {
            initialUp = initialUp_;
            initialForward = initialForward_;
            initialRight = initialRight_;
            referencePlaneNormal = Vector3.Cross(initialRight, initialUp).normalized; // Normal of the plane defined by initialRight and initialUp

            previousRoll = 0;
            accumulatedRoll = 0;
            rollCount = 0;
            lastRollDelta = 0;
        }

        public void DetectRollTrick(Vector3 currentUp_, Vector3 currentForward_)
        {
            // Get the current up direction (Y-axis)
            Vector3 currentUp = currentUp_;

            // Project current up direction onto the initial X-Y plane
            Vector3 upInXYPlane = Vector3.ProjectOnPlane(currentUp, referencePlaneNormal).normalized;

            // Compute the angle between the projected up direction and the initial up direction
            float currentRoll = Vector3.SignedAngle(initialUp, upInXYPlane, initialForward);

            if (currentRoll < 0)
            {
                currentRoll = 360 + currentRoll;
            }

            int alignmentState = CheckrollAlignment(currentForward_);

            float rollDelta = Mathf.DeltaAngle(previousRoll, currentRoll);

            if (alignmentState == 0 || alignmentState == 1)
            {
                // Check if the roll direction has changed
                if (Mathf.Sign(rollDelta) != Mathf.Sign(lastRollDelta) && Mathf.Abs(lastRollDelta) > 0)
                {
                    // Direction changed, reset roll counter
                    //Plugin.Logger.LogInfo("Roll direction changed! Resetting roll counter.");
                    accumulatedRoll = 0;
                    rollCount = 0;
                }

                // Accumulate the roll rotation
                accumulatedRoll += rollDelta;

                // Check if we have completed a 90º increment of roll
                if (Mathf.Abs(accumulatedRoll) >= rollThreshold)
                {
                    rollCount++;
                    accumulatedRoll = 0; // Reset accumulated roll for the next 90º increment

                    if (((rollCount % 2) == 0) && (rollCount != 0))
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
                        if (rollDelta > 0)
                        {
                            isPositiveDelta = true;
                        }
                        else
                        {
                            isPositiveDelta = false;
                        }
                        trickName = "Roll";
                        string rotations_str = $"{rollCount * 90}";
                        Trick trick = new()
                        {
                            trickName = trickName,
                            rotation = rotations_str,
                            isInverse = isInverse,
                            isPositiveDelta = isPositiveDelta
                        };
                        int points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_2_Sound");
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
        }

        private int CheckrollAlignment(Vector3 currentForward_)
        {
            // Check if the player is sufficiently tilted relative to the initial reference

            // Compute the dot product between the current and reference X-axis directions
            float alignment = Vector3.Dot(currentForward_, initialForward);

            if (Mathf.Abs(alignment) < rollAlignmentThreshold)
            {
                return 2; // Skip roll detection if the player is not straight
            }
            if (alignment < 0)
            {
                return 1; // rolling backwards
            }
            return 0; // rolling normally
        }
    }
}