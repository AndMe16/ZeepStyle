using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using ZeepSDK.UI;
using ZeepStyle.TrickManager;
using ZeepStyle.UIHelpers;

namespace ZeepStyle.TrickDisplayManager;

public class StyleTrickDisplay : MonoBehaviour
{
    private const int MaxDisplayTricks = 5;

    // List to store the tricks text
    public List<string> displayTextList = [];

    private const int BaseTextSize = 25;

    private Canvas canvas;

    private GameObject canvasObject;

    private StyleTrickManager trickManager;
    private TextMeshProUGUI trickText;

    private RectTransform uiRectTransform;

    private void Start()
    {
        trickManager = FindObjectOfType<StyleTrickManager>();
    }

    public void CreateDisplay()
    {
        // Clone the existing main Canvas
        canvasObject = StyleUIHelpers.CloneMainCanvas("Style_TricksDisplayCanvas");

        if (!canvasObject) return;

        canvas = canvasObject.GetComponent<Canvas>();

        (trickText, uiRectTransform) = StyleUIHelpers.CreateTextElement(
            canvas,
            "Style_TricksDisplay",
            "",
            new Vector2(0, -150),
            new Vector2(400, 150),
            BaseTextSize,
            TextAlignmentOptions.Bottom,
            false
        );

        // ---------- Register with UI Configurator ----------
        if (uiRectTransform)
            UIApi.AddToConfigurator(uiRectTransform);
    }

    // Method to update the displayed trick name
    public void DisplayTrick(Trick trick, int points)
    {
        //Plugin.Logger.LogInfo($"Displaying tricks {trick.trickName}, {trick.rotation}, {trick.isInverse}");

        string displayText;
        if (trick.TrickName is "Frontflip" or "Backflip" or "Sideflip")
            displayText = trick.TrickName + $" x{trick.Rotation}";
        else
            displayText = trick.Rotation + " " + trick.TrickName;

        if (trick.IsInverse) displayText = "Inverse" + " " + displayText;

        displayText += $" (+{points})";

        // Check if the tricksList is not empty before accessing the last element
        if (trickManager.TricksList.Count == 0 || trickManager.TricksList[^1].TrickName != trick.TrickName ||
            trickManager.TricksList[^1].IsInverse != trick.IsInverse ||
            trickManager.TricksList[^1].IsPositiveDelta != trick.IsPositiveDelta)
        {
            // Add only if the previous trick was different or the list is empty
            trickManager.TricksList.Add(trick);
            displayTextList.Add(displayText);
        }
        else
        {
            // Modify the last trick if it's the same
            trickManager.TricksList[^1] = trick;
            if (displayTextList.Count > 0)
                displayTextList[^1] = displayText;
            else
                displayTextList.Add(displayText);
        }

        // Limit the displayTextList to the last maxDisplayTricks items
        if (displayTextList.Count > MaxDisplayTricks)
            displayTextList.RemoveRange(0, displayTextList.Count - MaxDisplayTricks);

        UpdateTrickDisplay();
        if (trickManager.HideTextOnAirCoroutine != null)
            //Plugin.Logger.LogInfo($"DisplayTrick: Stoping trickManager.hideTextOnAirCoroutine {trickManager.hideTextOnAirCoroutine.ToString()}");
            StopCoroutine(trickManager.HideTextOnAirCoroutine);
        //Plugin.Logger.LogInfo("DisplayTrick: Starting Coroutine HideTextAfterSeconds(4)");
        trickManager.HideTextOnAirCoroutine = StartCoroutine(HideTextAfterSeconds(4));
        //Plugin.Logger.LogInfo($"DisplayTrick: Coroutine HideTextAfterSeconds(4) started: {trickManager.hideTextOnAirCoroutine.ToString()}");
    }

    // Method to update the trick display
    private void UpdateTrickDisplay()
    {
        // Start building the formatted text
        StringBuilder formattedText = new();

        // Loop through the displayTextList to format each line
        for (var i = 0; i < displayTextList.Count; i++)
        {
            var line = displayTextList[i];

            var iInv = displayTextList.Count - 1 - i;

            var alpha = 255 - iInv * 25; // Adjust the decrement based on your needs
            alpha = alpha < 25 ? 25 : alpha;
            alpha = Mathf.Clamp(alpha, 0, 255); // Ensure alpha is within valid range

            // Convert alpha to hexadecimal format
            var alphaHex = alpha.ToString("X2");

            // Apply the alpha value to a base color (e.g., white = FFFFFF, with varying alpha)
            var colorWithAlpha = $"#FFFFFF{alphaHex}"; // White color with varying transparency

            // Modify size based on the index
            var size = BaseTextSize - iInv * 4; // Decrease size for each subsequent line
            size = size < 10 ? 10 : size;
            // Apply TextMeshPro rich text tags
            formattedText.AppendLine($"<color={colorWithAlpha}><size={size}>{line}</size></color>");
        }

        //Plugin.Logger.LogInfo($"Displaying tricks: {formattedText.ToString()}");

        // Update the TextMeshPro text with the formatted text
        if (trickText) trickText.text = formattedText.ToString();
    }

    public void LandingDisplay(int totalPoints)
    {
        UpdateTrickDisplay();
        if (trickText) trickText.text += $"<color=#f7e520><b><size={BaseTextSize}>+{totalPoints}</b>";
    }

    //Method to hide text after a delay
    public IEnumerator HideTextAfterSeconds(float seconds)
    {
        //Plugin.Logger.LogInfo($"Hiding text after {seconds}");
        yield return new WaitForSeconds(seconds);
        if (trickText) trickText.text = "";
        displayTextList.Clear();
    }

    public void DestroyComponent()
    {
        // Unregister from UI Configurator
        if (uiRectTransform)
        {
            UIApi.RemoveFromConfigurator(uiRectTransform);
            uiRectTransform = null;
        }

        // Destroy only your own UI elements, not the canvas
        if (trickText)
        {
            Destroy(trickText.gameObject);
            trickText = null;
        }

        if (canvasObject)
        {
            Destroy(canvasObject);
            canvasObject = null;
        }

        canvas = null;
    }

    public void HideText()
    {
        if (trickText) trickText.enabled = false; // Disable the text to hide it
    }

    public void ShowText()
    {
        if (trickText) trickText.enabled = true; // Enable the text to show it again
    }

    public void ResetText()
    {
        if (trickText) trickText.text = "";
        trickManager.TricksList.Clear(); // Clear the list of tricks
        displayTextList.Clear();
    }

    public void StopHideTextOnAirCoroutine()
    {
        //Plugin.Logger.LogInfo($"OnLand: Stoping hideTextOnAirCoroutine {trickManager.hideTextOnAirCoroutine.ToString()}");
        StopCoroutine(trickManager.HideTextOnAirCoroutine);
    }
}