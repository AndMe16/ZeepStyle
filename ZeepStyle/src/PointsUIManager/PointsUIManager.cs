using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeepStyle;
using ZeepStyle.src;

public class Style_PointsUIManager : MonoBehaviour
{
    private Canvas canvas;
    public TextMeshProUGUI bestPbAllTimeText;
    public TextMeshProUGUI bestPbCurrentSessionText;
    public TextMeshProUGUI currentRunPointsText;

    GameObject canvasObject;
    GameObject textObject;

    // Trick Points
    Style_TrickPointsManager trickPointsManager;
    Style_TrickManager trickManager;

    void Start()
    {
        trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();
        trickManager = FindObjectOfType<Style_TrickManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(ModConfig.displayPBsBind.Value))
        {
            TogglePointsUI();
        }
    }

    public void TogglePointsUI()
    {
        if (ModConfig.displayPBs.Value)
        {
            ModConfig.displayPBs.Value = false;
            DestroyComponent();
        }
        else
        {
            ModConfig.displayPBs.Value = true;
            CreateUI();
        } 
    }

    public void CreateUI()
    {
        if (ModConfig.displayPBs.Value && trickManager.isPlayerSpawned)
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
            bestPbAllTimeText = CreateTextElement($"Best PB (All Sessions): {trickPointsManager.bestPbAllTime}", new Vector2(0, 500));

            // Create Best PB Current Session Text
            bestPbCurrentSessionText = CreateTextElement($"Best PB (Current Session): {trickPointsManager.bestPbCurrentSession}", new Vector2(0, 480));

            // Create Current Run Points Text
            currentRunPointsText = CreateTextElement($"Current Run Points: {trickPointsManager.totalRunPoints}", new Vector2(0, 460));
        }
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
        textMesh.fontSize = 15;
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


    public void DestroyComponent()
    {
        Destroy(canvasObject);
        Destroy(textObject);
    }

}


