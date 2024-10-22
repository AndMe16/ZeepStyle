using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeepStyle;

public class Style_PointsUIManager : MonoBehaviour
{
    private Canvas canvas;
    private TextMeshProUGUI bestPbAllTimeText;
    private TextMeshProUGUI bestPbCurrentSessionText;
    private TextMeshProUGUI currentRunPointsText;

    GameObject canvasObject;
    GameObject textObject;

    private int bestPbAllTime = 0;
    private int bestPbCurrentSession = 0;
    private int currentRunPoints = 0;

    public void CreateUI()
    {
        // Create a new Canvas
        canvasObject = new GameObject("PointsCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;  // Higher values render above others

        // Create a CanvasScaler and GraphicRaycaster
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();

        // Create Best PB All Time Text
        bestPbAllTimeText = CreateTextElement($"Best PB (All Sessions): {bestPbAllTime}", new Vector2(0, 500));

        // Create Best PB Current Session Text
        bestPbCurrentSessionText = CreateTextElement($"Best PB (Current Session): {bestPbCurrentSession}", new Vector2(0, 480));

        // Create Current Run Points Text
        currentRunPointsText = CreateTextElement($"Current Run Points: {currentRunPoints}", new Vector2(0, 460));
    }

    // Helper method to create TextMeshProUGUI elements
    private TextMeshProUGUI CreateTextElement(string textContent, Vector2 position)
    {
        textObject = new GameObject("TextElement");
        textObject.transform.SetParent(canvas.transform);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(600, 100);

        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = textContent;
        textMesh.fontSize = 20;
        textMesh.alignment = TextAlignmentOptions.Center;

        // Try to assign one of the available fonts
        TMP_FontAsset font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Code New Roman b SDF");

        if (font != null)
        {
            textMesh.font = font;
            textMesh.fontMaterial = new Material(textMesh.fontMaterial);
        }
        else
        {
            Plugin.Logger.LogError("Font not found in loaded resources!");
        }

        textMesh.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.05f); // Set outline width
        textMesh.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black); // Set outline color

        textMesh.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.7f);
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.3f);
        textMesh.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black); // Shadow color

        return textMesh;
    }

    // Call this method to update the current run points
    public void UpdateCurrentRunPoints(int points)
    {
        currentRunPoints = points;
        currentRunPointsText.text = $"Current Run Points: {currentRunPoints}";

        // Update current session PB if the current run is better
        if (currentRunPoints > bestPbCurrentSession)
        {
            bestPbCurrentSession = currentRunPoints;
            bestPbCurrentSessionText.text = $"Best PB (Current Session): {bestPbCurrentSession}";
        }

        // Update all-time PB if necessary
        if (currentRunPoints > bestPbAllTime)
        {
            bestPbAllTime = currentRunPoints;
            bestPbAllTimeText.text = $"Best PB (All Sessions): {bestPbAllTime}";
        }
    }

    public void DestroyComponent()
    {
        Destroy(canvasObject);
        Destroy(textObject);
    }

}


