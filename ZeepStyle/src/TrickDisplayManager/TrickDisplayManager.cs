using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Style_TrickDisplay : MonoBehaviour
{
    private TextMeshProUGUI trickText;
    private GameObject canvasObject;
    private GameObject textObject;

    public void CreateDisplay()
    {
        // Create a Canvas to hold the TextMeshPro element
        canvasObject = new GameObject("TrickCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

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
        trickText.fontSize = 60;
        trickText.alignment = TextAlignmentOptions.Center;
        trickText.color = Color.white;

        // Load and assign the custom font
        TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("Bangers SDF"); // Change path to your font asset
        if (customFont != null)
        {
            trickText.font = customFont;
        }
        else
        {
            Debug.LogWarning("Custom font not found, using default font.");
        }

        // Set the position and size of the text object
        RectTransform textRectTransform = trickText.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(600, 100);
        textRectTransform.anchoredPosition = new Vector2(0, 100); // Position near the bottom of the screen
    }

    // Method to update the displayed trick name
    public void DisplayTrick(string trickName)
    {
        trickText.text = $"{trickName}";
        //StartCoroutine(HideTextAfterSeconds(5));
    }

    // Optional: Method to hide text after a delay
    public IEnumerator HideTextAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        trickText.text = "";
    }

    public void DestroyComponent()
    {
        Destroy(canvasObject);
        Destroy(textObject);
    }
}
