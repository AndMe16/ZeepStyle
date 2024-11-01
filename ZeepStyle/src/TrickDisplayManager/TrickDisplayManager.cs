using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeepStyle;

public class Style_TrickDisplay : MonoBehaviour
{
    private TextMeshProUGUI trickText;
    private GameObject canvasObject;
    private GameObject textObject;

    Style_TrickManager trickManager;
    Style_TrickPointsManager trickPointsManager;

    private readonly int baseTextSize = 30;

    // List to store the tricks text
    public List<string> displayTextList = new List<string>();

    void Start()
    {
        trickManager = FindObjectOfType<Style_TrickManager>();
        trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();
    }

    public void CreateDisplay()
    {
        // Create a Canvas to hold the TextMeshPro element
        canvasObject = new GameObject("Style_TrickCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;  // Higher values render above others


        // Optionally, add a CanvasScaler to handle different resolutions
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        // Create a TextMeshProUGUI object
        textObject = new GameObject("TrickText");
        textObject.transform.SetParent(canvasObject.transform);

        // Add TextMeshProUGUI component to the text object
        trickText = textObject.AddComponent<TextMeshProUGUI>();

        // Set text properties
        trickText.fontSize = baseTextSize;
        trickText.alignment = TextAlignmentOptions.Center;
        trickText.color = Color.white;

        // Try to assign one of the available fonts
        TMP_FontAsset font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Code New Roman b SDF");

        if (font != null)
        {
            trickText.font = font;
            trickText.fontMaterial = new Material(trickText.fontMaterial);
        }
        else
        {
            Plugin.Logger.LogError("Font not found in loaded resources!");
        }

        trickText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        trickText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.05f); // Set outline width
        trickText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black); // Set outline color

        trickText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        trickText.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.7f);
        trickText.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.3f);
        trickText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black); // Shadow color

        // Set the position and size of the text object
        RectTransform textRectTransform = trickText.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(600, 200);
        textRectTransform.anchoredPosition = new Vector2(0, 300); // Position near the middle-top of the screen
    }

    // Method to update the displayed trick name
    public void DisplayTrick(Trick trick, int points)
    {
        //Plugin.Logger.LogInfo($"Displaying tricks {trick.trickName}, {trick.rotation}, {trick.isInverse}");
        Trick _trick = trick;

        string displayText;
        if ((_trick.trickName == "Frontflip") || (_trick.trickName == "Backflip")) {
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

        displayText = displayText + $" (+{points})";

        // Check if the tricksList is not empty before accessing the last element
        if (trickManager.tricksList.Count == 0 || (trickManager.tricksList[^1].trickName != _trick.trickName) || (trickManager.tricksList[^1].isInverse != _trick.isInverse) || (trickManager.tricksList[^1].isPositiveDelta != _trick.isPositiveDelta))
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
                displayTextList[^1] = (displayText);
            }
            else
            {
                displayTextList.Add(displayText);
            }
            
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
        StringBuilder formattedText = new StringBuilder();

        // Loop through the displayTextList to format each line
        for (int i = 0; i < displayTextList.Count; i++)
        {
            string line = displayTextList[i];

            int i_inv = (displayTextList.Count - 1) - i;

            int alpha = 255 - (i_inv * 25);  // Adjust the decrement based on your needs
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
        trickText.text = formattedText.ToString();
    }

    public void LandingDisplay(int totalPoints)
    {
        UpdateTrickDisplay();
        trickText.text = trickText.text + $"<color=#f7e520><b>+{totalPoints}</b>";
    }

    //Method to hide text after a delay
    public IEnumerator HideTextAfterSeconds(float seconds)
    {
        //Plugin.Logger.LogInfo($"Hidding text after {seconds}");
        yield return new WaitForSeconds(seconds);
        trickText.text = "";
        displayTextList.Clear();
    }

    public void DestroyComponent()
    {
        Destroy(canvasObject);
        Destroy(textObject);
    }

    public void HideText()
    {
        trickText.enabled = false;  // Disable the text to hide it
    }

    public void ShowText()
    {
        trickText.enabled = true;  // Enable the text to show it again
    }

    public void ResetText()
    {
        trickText.text = "";
        trickManager.tricksList.Clear();   // Clear the list of tricks
        displayTextList.Clear();
    }

    public void StopHideTextOnAirCoroutine()
    {
        //Plugin.Logger.LogInfo($"OnLand: Stoping hideTextOnAirCoroutine {trickManager.hideTextOnAirCoroutine.ToString()}");
        StopCoroutine(trickManager.hideTextOnAirCoroutine);
    }

}
