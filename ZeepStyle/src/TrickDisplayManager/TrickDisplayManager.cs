using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
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

    // List to store the tricks
    public List<Tricks> tricksList = new List<Tricks>();

    // List to store the tricks text
    public List<string> displayTextList = new List<string>();

    void Start()
    {
        trickManager = FindObjectOfType<Style_TrickManager>();
    }

    public void CreateDisplay()
    {
        // Create a Canvas to hold the TextMeshPro element
        canvasObject = new GameObject("TrickCanvas");
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
        trickText.fontSize = 50;
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
        textRectTransform.anchoredPosition = new Vector2(0, 300); // Position near the bottom of the screen
    }

    // Method to update the displayed trick name
    public void DisplayTrick(string trickName, string rotation, bool isInverse, bool isPositiveDelta)
    {
        Plugin.Logger.LogInfo($"Displaying tricks {trickName}, {rotation}, {isInverse}");
        Tricks trickList = new();
        trickList.trickName = trickName;
        trickList.rotation = rotation;
        trickList.isInverse = isInverse;
        trickList.isPositiveDelta = isPositiveDelta;

        string displayText;
        if ((trickName == "Frontflip") || (trickName == "Backflip")) {
            displayText = trickName + rotation;
        }
        else
        {
            if (!isInverse)
            {
                displayText = rotation + " " + trickName;
            }
            else {
                displayText = "Inverse" + " " + rotation + " " + trickName;
            }
        }
        // Check if the tricksList is not empty before accessing the last element
        if (tricksList.Count == 0 || (tricksList[^1].trickName != trickName) || (tricksList[^1].isInverse != isInverse) || (tricksList[^1].isPositiveDelta != isPositiveDelta))
        {
            // Add only if the previous trick was different or the list is empty
            tricksList.Add(trickList);
            displayTextList.Add(displayText);
        }
        else
        {
            // Modify the last trick if it's the same
            tricksList[^1] = trickList;
            displayTextList[^1] = (displayText);
        }
        UpdateTrickDisplay();
        if (trickManager.hideTextOnAirCoroutine != null)
        {
            StopCoroutine(trickManager.hideTextOnAirCoroutine);
        }
        trickManager.hideTextOnAirCoroutine = StartCoroutine(HideTextAfterSeconds(4));
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
            int size = 50 - i_inv * 4;  // Decrease size for each subsequent line
            size = size < 10 ? 10 : size;
            // Apply TextMeshPro rich text tags
            formattedText.AppendLine($"<color={colorWithAlpha}><size={size}>{line}</size></color>");
        }

        Plugin.Logger.LogInfo($"Displaying tricks: {formattedText.ToString()}");

        // Update the TextMeshPro text with the formatted text
        trickText.text = formattedText.ToString();
    }


    //Method to hide text after a delay
    public IEnumerator HideTextAfterSeconds(float seconds)
    {
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
        tricksList.Clear();   // Clear the list of tricks
        displayTextList.Clear();
    }

}

public class Tricks
{
    public string trickName;
    public string rotation;
    public bool isInverse;
    public bool isPositiveDelta;
}