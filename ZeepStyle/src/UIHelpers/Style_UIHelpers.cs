using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeepSDK.External.Cysharp.Threading.Tasks.Triggers;

namespace ZeepStyle.src.UIHelpers
{
    public static class Style_UIHelpers
    {
        public static GameObject CloneMainCanvas(string newName)
        {
            Transform canvasTransform = PlayerManager.Instance.gameObject.transform.Find("Canvas");
            if (canvasTransform == null)
            {
                Plugin.Logger.LogError("Main Canvas not found!");
                return null;
            }
            GameObject originalCanvas = canvasTransform.gameObject;
            GameObject clonedCanvas = Object.Instantiate(originalCanvas);
            clonedCanvas.name = newName;

            foreach (Transform child in clonedCanvas.transform)
            {
                Object.Destroy(child.gameObject);
            }

            if (clonedCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                clonedCanvas.AddComponent<GraphicRaycaster>();
            }

            if (clonedCanvas.TryGetComponent<Canvas>(out Canvas canvas))
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = -1;  // Higher values render above others
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
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(canvas.transform);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
            textMesh.text = textContent;
            textMesh.fontSize = fontSize;
            textMesh.alignment = alignment;
            textMesh.enableAutoSizing = autosize;
            textMesh.fontSizeMin = 1;

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
            textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.05f);
            textMesh.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            textMesh.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.7f);
            textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.3f);
            textMesh.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);

            return (textMesh, rectTransform);
        }
    }
}
