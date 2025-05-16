using UnityEngine;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.TrickDisplayManager;
using ZeepStyle.src.TrickManager;

namespace ZeepStyle.src.Tricks
{
    public class Style_Pitch : MonoBehaviour
    {
        // Flip (Pitch)
        private Vector3 initialRight; // Reference X-axis direction
        private Vector3 initialForward; // Z-axis (forward) direction at takeoff
        private Vector3 initialUp; // Y-axis (up) direction at takeoff
        private Vector3 referencePlaneNormal; // Normal of the plane defined by initialForward and initialUp
        private float accumulatedPitch_flip = 0; // Accumulated pitch angle for normal flips
        private float accumulatedPitch_sideflip = 0; // Accumulated pitch angle for side flips
        private float previousPitch = 0;
        private readonly float flipThreshold = 80.0f; // Detect each 90º flip
        private readonly float sideflipThreshold = 80.0f; // Detect each 90º sideflip
        private readonly float flipAlignmentThreshold = 0.7f; // Threshold for X-axis alignment with right direction (dot product close to 1 = straight)
        private readonly float sideFlipAlignmentThreshold = 0.8f; // Threshold for X-axis alignment with up direction (dot product close to 1 = side)
        private int flipCount = 0;
        private int sideflipCount = 0;
        private float lastPitchDelta; // To track the direction of the previous pitch delta

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
            accumulatedPitch_flip = 0;
            accumulatedPitch_sideflip = 0;
            flipCount = 0;
            sideflipCount = 0;
            lastPitchDelta = 0;
        }

        public void OnLeaveGround(UnityEngine.Vector3 initialUp_, UnityEngine.Vector3 initialForward_, UnityEngine.Vector3 initialRight_)
        {
            initialUp = initialUp_;
            initialForward = initialForward_;
            initialRight = initialRight_;
            referencePlaneNormal = Vector3.Cross(initialForward, initialUp).normalized; // Normal of the plane defined by initialForward and initialUp

            previousPitch = 0;
            accumulatedPitch_flip = 0;
            accumulatedPitch_sideflip = 0;
            flipCount = 0;
            sideflipCount = 0;
            lastPitchDelta = 0;
        }

        public void DetectFlipTrick(Vector3 currentForward_, Vector3 currentRight_, Vector3 currentUp_)
        {
            // Get the current forward direction (Z-axis)

            // Project current forward direction onto the initial Z-Y plane
            Vector3 forwardInZYPlane = Vector3.ProjectOnPlane(currentForward_, referencePlaneNormal).normalized;

            // Compute the angle between the projected forward direction and the initial forward direction
            float currentPitch = Vector3.SignedAngle(initialForward, forwardInZYPlane, initialRight);

            if (currentPitch < 0)
            {
                currentPitch = 360 + currentPitch;
            }

            int flipAlignmentState = CheckFlipAlignment(currentRight_);
            int sideflipAlignmentState = CheckSideFlipAlignment(currentUp_);

            float pitchDelta = Mathf.DeltaAngle(previousPitch, currentPitch);

            if (flipAlignmentState == 0 || flipAlignmentState == 1)
            {
                // Check if the spin direction has changed
                if (Mathf.Sign(pitchDelta) != Mathf.Sign(lastPitchDelta) && Mathf.Abs(lastPitchDelta) > 0)
                {
                    // Direction changed, reset flip counter
                    //Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
                    accumulatedPitch_flip = 0;
                    flipCount = 0;
                }

                // Accumulate the pitch rotation
                accumulatedPitch_flip += pitchDelta;

                // Check if we have completed a 90º increment of flip
                if (Mathf.Abs(accumulatedPitch_flip) >= flipThreshold)
                {
                    flipCount++;
                    accumulatedPitch_flip = 0; // Reset accumulated pitch for the next 90º increment

                    if (((flipCount % 4) == 0) && (flipCount != 0))
                    {
                        string trickName;
                        bool isInverse;
                        bool isPositiveDelta;
                        if (flipAlignmentState == 0)
                        {
                            isInverse = false;
                        }
                        else
                        {
                            isInverse = true;
                        }
                        if (pitchDelta > 0)
                        {
                            isPositiveDelta = true;
                            trickName = "Frontflip";
                        }
                        else
                        {
                            isPositiveDelta = false;
                            trickName = $"Backflip";
                        }
                        string rotations_str = $"{flipCount / 4}";
                        Trick trick = new()
                        {
                            trickName = trickName,
                            rotation = rotations_str,
                            isInverse = isInverse,
                            isPositiveDelta = isPositiveDelta
                        };
                        int points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_3_Sound");
                    }
                }
            }
            else
            {
                accumulatedPitch_flip = 0;
                flipCount = 0;
            }

            if (sideflipAlignmentState == 0 || sideflipAlignmentState == 1)
            {
                // Check if the spin direction has changed
                if (Mathf.Sign(pitchDelta) != Mathf.Sign(lastPitchDelta) && Mathf.Abs(lastPitchDelta) > 0)
                {
                    // Direction changed, reset flip counter
                    //Plugin.Logger.LogInfo("Flip direction changed! Resetting flip counter.");
                    accumulatedPitch_sideflip = 0;
                    sideflipCount = 0;
                }
                // Accumulate the pitch rotation
                accumulatedPitch_sideflip += pitchDelta;

                // Check if we have completed a 90º increment of flip
                if (Mathf.Abs(accumulatedPitch_sideflip) >= sideflipThreshold)
                {
                    sideflipCount++;
                    accumulatedPitch_sideflip = 0; // Reset accumulated pitch for the next 90º increment

                    if (((sideflipCount % 4) == 0) && (sideflipCount != 0))
                    {
                        string trickName = "Sideflip";
                        bool isInverse;
                        bool isPositiveDelta = true;
                        if (pitchDelta > 0)
                        {
                            isInverse = false;
                        }
                        else
                        {
                            isInverse = true;
                        }
                        string rotations_str = $"{sideflipCount / 4}";
                        Trick trick = new()
                        {
                            trickName = trickName,
                            rotation = rotations_str,
                            isInverse = isInverse,
                            isPositiveDelta = isPositiveDelta
                        };
                        int points = trickPointsManager.CalculatePoints(trick);
                        trickDisplay.DisplayTrick(trick, points);
                        soundEffectManager.PlaySound("SimpleTrick_3_Sound");
                    }
                }
            }
            else
            {
                accumulatedPitch_sideflip = 0;
                sideflipCount = 0;
            }

            // Update the previous pitch and last pitch delta for the next frame
            previousPitch = currentPitch;
            lastPitchDelta = pitchDelta; // Store current pitch delta to detect direction change
        }

        private int CheckFlipAlignment(Vector3 currentRight_)
        {
            // Check if the player is sufficiently tilted relative to the initial reference
            Vector3 currentRight = currentRight_; // Current right direction of the rigidbody

            // Compute the dot product between the current right and reference X-axis directions
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

        private int CheckSideFlipAlignment(Vector3 currentUp_)
        {
            // Check if the player is sufficiently tilted relative to the initial reference
            Vector3 currentUp = currentUp_; // Current up direction of the rigidbody

            // Compute the dot product between the current up and reference X-axis directions
            float alignment = Vector3.Dot(currentUp, initialRight);

            if (Mathf.Abs(alignment) < sideFlipAlignmentThreshold)
            {
                return 2; // Skip sideflip detection if the player is not sideways
            }

            if (alignment < 0)
            {
                return 1; // Sidefliping 
            }
            return 0; // Sidefliping
        }
    }
}