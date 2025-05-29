using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZeepStyle.UIHelpers;

public static class StyleUIHelpers
{
    public static GameObject CloneMainCanvas(string newName)
    {
        var canvasTransform = PlayerManager.Instance.gameObject.transform.Find("Canvas");
        if (!canvasTransform)
        {
            Plugin.logger.LogError("Main Canvas not found!");
            return null;
        }

        var originalCanvas = canvasTransform.gameObject;
        var clonedCanvas = Object.Instantiate(originalCanvas);
        clonedCanvas.name = newName;

        foreach (Transform child in clonedCanvas.transform) Object.Destroy(child.gameObject);

        if (!clonedCanvas.GetComponent<GraphicRaycaster>()) clonedCanvas.AddComponent<GraphicRaycaster>();

        if (clonedCanvas.TryGetComponent(out Canvas canvas))
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1; // Higher values render above others
        }

        SceneManager.MoveGameObjectToScene(clonedCanvas, SceneManager.GetActiveScene());

        return clonedCanvas;
    }

    public static (TextMeshProUGUI, RectTransform) CreateTextElement(
        Canvas canvas,
        string name,
        string textContent,
        Vector2 position,
        Vector2 size,
        float fontSize = 15,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        bool autosize = true)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(canvas.transform);

        var rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        var textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = textContent;
        textMesh.fontSize = fontSize;
        textMesh.alignment = alignment;
        textMesh.enableAutoSizing = autosize;
        textMesh.fontSizeMin = 1;

        var font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
            .FirstOrDefault(f => f.name == "Code New Roman b SDF");
        if (font)
        {
            textMesh.font = font;
            textMesh.fontMaterial = new Material(textMesh.fontMaterial);
        }
        else
        {
            Plugin.logger.LogError("Font not found in loaded resources!");
        }

        textMesh.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.05f);
        textMesh.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

        textMesh.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.7f);
        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.3f);
        textMesh.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);

        return (textMesh, rectTransform);
    }
}