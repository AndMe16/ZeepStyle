using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using ZeepSDK.UI;
using ZeepStyle.src.TrickManager;
using ZeepStyle.src.UIHelpers;

namespace ZeepStyle.src.TrickDisplayManager
{
    public class Style_TrickDisplay : MonoBehaviour
    {

        private Canvas canvas;
        private TextMeshProUGUI trickText;

        private RectTransform _uiRectTransform;

        private GameObject canvasObject;

        Style_TrickManager trickManager;

        private readonly int baseTextSize = 25;
        private const int maxDisplayTricks = 5;

        // List to store the tricks text
        public List<string> displayTextList = [];

        void Start()
        {
            trickManager = FindObjectOfType<Style_TrickManager>();
        }

        public void CreateDisplay()
        {
            // Clone the existing main Canvas
            canvasObject = Style_UIHelpers.CloneMainCanvas("Style_TricksDisplayCanvas");

            if (canvasObject == null)
            {
                return;
            }

            canvas = canvasObject.GetComponent<Canvas>();

            (trickText, _uiRectTransform) = Style_UIHelpers.CreateTextElement(
                canvas,
                "Style_TricksDisplay",
                "",
                new Vector2(0, -150),
                new Vector2(400, 150),
                baseTextSize,
                TextAlignmentOptions.Bottom,
                false
            );

            // ---------- Register with UI Configurator ----------
            if (_uiRectTransform != null)
                UIApi.AddToConfigurator(_uiRectTransform);
        }

        // Method to update the displayed trick name
        public void DisplayTrick(Trick trick, int points)
        {
            //Plugin.Logger.LogInfo($"Displaying tricks {trick.trickName}, {trick.rotation}, {trick.isInverse}");
            Trick _trick = trick;

            string displayText;
            if (_trick.trickName == "Frontflip" || _trick.trickName == "Backflip" || trick.trickName == "Sideflip")
            {
                displayText = _trick.trickName + $" x{_trick.rotation}";
            }
            else
            {
                displayText = _trick.rotation + " " + _trick.trickName;
            }

            if (_trick.isInverse)
            {
                displayText = "Inverse" + " " + displayText;
            }

            displayText += $" (+{points})";

            // Check if the tricksList is not empty before accessing the last element
            if (trickManager.tricksList.Count == 0 || trickManager.tricksList[^1].trickName != _trick.trickName || trickManager.tricksList[^1].isInverse != _trick.isInverse || trickManager.tricksList[^1].isPositiveDelta != _trick.isPositiveDelta)
            {
                // Add only if the previous trick was different or the list is empty
                trickManager.tricksList.Add(_trick);
                displayTextList.Add(displayText);
            }
            else
            {
                // Modify the last trick if it's the same
                trickManager.tricksList[^1] = _trick;
                if (displayTextList.Count > 0)
                {
                    displayTextList[^1] = displayText;
                }
                else
                {
                    displayTextList.Add(displayText);
                }
            }

            // Limit the displayTextList to the last maxDisplayTricks items
            if (displayTextList.Count > maxDisplayTricks)
            {
                displayTextList.RemoveRange(0, displayTextList.Count - maxDisplayTricks);
            }

            UpdateTrickDisplay();
            if (trickManager.hideTextOnAirCoroutine != null)
            {
                //Plugin.Logger.LogInfo($"DisplayTrick: Stoping trickManager.hideTextOnAirCoroutine {trickManager.hideTextOnAirCoroutine.ToString()}");
                StopCoroutine(trickManager.hideTextOnAirCoroutine);
            }
            //Plugin.Logger.LogInfo("DisplayTrick: Starting Coroutine HideTextAfterSeconds(4)");
            trickManager.hideTextOnAirCoroutine = StartCoroutine(HideTextAfterSeconds(4));
            //Plugin.Logger.LogInfo($"DisplayTrick: Coroutine HideTextAfterSeconds(4) started: {trickManager.hideTextOnAirCoroutine.ToString()}");
        }

        // Method to update the trick display
        private void UpdateTrickDisplay()
        {
            // Start building the formatted text
            StringBuilder formattedText = new();

            // Loop through the displayTextList to format each line
            for (int i = 0; i < displayTextList.Count; i++)
            {
                string line = displayTextList[i];

                int i_inv = displayTextList.Count - 1 - i;

                int alpha = 255 - i_inv * 25;  // Adjust the decrement based on your needs
                alpha = alpha < 25 ? 25 : alpha;
                alpha = Mathf.Clamp(alpha, 0, 255);  // Ensure alpha is within valid range

                // Convert alpha to hexadecimal format
                string alphaHex = alpha.ToString("X2");

                // Apply the alpha value to a base color (e.g., white = FFFFFF, with varying alpha)
                string colorWithAlpha = $"#FFFFFF{alphaHex}";  // White color with varying transparency

                // Modify size based on the index
                int size = baseTextSize - i_inv * 4;  // Decrease size for each subsequent line
                size = size < 10 ? 10 : size;
                // Apply TextMeshPro rich text tags
                formattedText.AppendLine($"<color={colorWithAlpha}><size={size}>{line}</size></color>");
            }

            //Plugin.Logger.LogInfo($"Displaying tricks: {formattedText.ToString()}");

            // Update the TextMeshPro text with the formatted text
            if (trickText != null)
            {
                trickText.text = formattedText.ToString();
            }
        }

        public void LandingDisplay(int totalPoints)
        {
            UpdateTrickDisplay();
            if (trickText != null)
            {
                trickText.text += $"<color=#f7e520><b><size={baseTextSize}>+{totalPoints}</b>";
            }
        }

        //Method to hide text after a delay
        public IEnumerator HideTextAfterSeconds(float seconds)
        {
            //Plugin.Logger.LogInfo($"Hidding text after {seconds}");
            yield return new WaitForSeconds(seconds);
            if (trickText != null)
            {
                trickText.text = "";
            }
            displayTextList.Clear();
        }

        public void DestroyComponent()
        {
            // Unregister from UI Configurator
            if (_uiRectTransform != null)
            {
                UIApi.RemoveFromConfigurator(_uiRectTransform);
                _uiRectTransform = null;
            }

            // Destroy only your own UI elements, not the canvas
            if (trickText != null)
            {
                GameObject.Destroy(trickText.gameObject);
                trickText = null;
            }

            if (canvasObject != null)
            {
                GameObject.Destroy(canvasObject);
                canvasObject = null;
            }

            canvas = null;
        }

        public void HideText()
        {
            if (trickText != null)
            {
                trickText.enabled = false;  // Disable the text to hide it
            }
        }

        public void ShowText()
        {
            if (trickText != null)
            {
                trickText.enabled = true;  // Enable the text to show it again
            }
        }

        public void ResetText()
        {
            if (trickText != null)
            {
                trickText.text = "";
            }
            trickManager.tricksList.Clear();   // Clear the list of tricks
            displayTextList.Clear();
        }

        public void StopHideTextOnAirCoroutine()
        {
            //Plugin.Logger.LogInfo($"OnLand: Stoping hideTextOnAirCoroutine {trickManager.hideTextOnAirCoroutine.ToString()}");
            StopCoroutine(trickManager.hideTextOnAirCoroutine);
        }

    }
}
