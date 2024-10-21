using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
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
        if ((trickName == "Frontflip")||(trickName == "Backflip")){
            displayText = trickName+rotation;
        }
        else
        {
            if (!isInverse)
            {
                displayText = rotation + " "+ trickName;
            }
            else{
                displayText = "Inverse" + " " + rotation + " " + trickName;
            }
        }
        // Check if the tricksList is not empty before accessing the last element
        if (tricksList.Count == 0 || (tricksList[^1].trickName != trickName) || (tricksList[^1].isInverse != isInverse) || (tricksList[^1].isPositiveDelta != isInverse))
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
        Plugin.Logger.LogInfo($"Displaying tricks: {string.Join("\n", displayTextList)}");
        trickText.text = string.Join("\n", displayTextList);  // Display tricks separated by new lines
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
