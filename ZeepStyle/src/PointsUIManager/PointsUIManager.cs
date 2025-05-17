using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeepStyle.src.Patches;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.TrickManager;
using ZeepSDK.UI;

namespace ZeepStyle.src.PointsUIManager
{
    public class Style_PointsUIManager : MonoBehaviour
    {
        private Canvas canvas;
        public TextMeshProUGUI pointsInfoText;

        private readonly List<RectTransform> _uiRectTransforms = new();

        GameObject canvasObject;
        GameObject textObject;

        // Trick Points
        Style_TrickPointsManager trickPointsManager;
        Style_TrickManager trickManager;

        bool isPaused = false;
        private RectTransform _uiRectTransform;

        void Awake()
        {
            //PatchOnlineChatUI_OnOpen.OnOpenChat += PatchOnlineChatUI_OnOpen_OnClose;
            PatchPauseHandler_Pause.OnPause += PatchPauseHandler_OnPause_OnUnpause;
            PatchPauseHandler_Unpause.OnUnpause += PatchPauseHandler_OnPause_OnUnpause;
        }

        void Start()
        {
            trickPointsManager = FindObjectOfType<Style_TrickPointsManager>();
            trickManager = FindObjectOfType<Style_TrickManager>();
        }

        private void PatchPauseHandler_OnPause_OnUnpause(PauseHandler obj)
        {
            isPaused = obj.IsPaused;

            if (isPaused)
                HideText();
            else
                ShowText();
        }

        void Update()
        {
            if (trickManager.isPlayerSpawned && !isPaused)
            {
                if (ModConfig.displayPBs.Value && canvasObject == null)
                {
                    CreateUI();
                }
                else if (!ModConfig.displayPBs.Value && canvasObject != null)
                {
                    DestroyComponent();
                }
                if (Input.GetKeyDown(ModConfig.displayPBsBind.Value) && !trickManager.isInPhotomode)
                {
                    TogglePointsUI();
                }
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

        public void HideText()
        {
            if (pointsInfoText != null)
            {
                pointsInfoText.enabled = false;  // Disable the text to hide it
            }
        }

        public void ShowText()
        {
            if (pointsInfoText != null)
            {
                pointsInfoText.enabled = true;  // Enable the text to show it again
            }
        }

        public void CreateUI()
        {
            if (ModConfig.displayPBs.Value && trickManager.isPlayerSpawned)
            {
                // Use the existing main Canvas
                Transform canvasTransform = PlayerManager.Instance.gameObject.transform.Find("Canvas");
                if (canvasTransform == null)
                {
                    Plugin.Logger.LogError("Main Canvas not found!");
                    return;
                }
                canvasObject = canvasTransform.gameObject;
                canvas = canvasObject.GetComponent<Canvas>();

                // Create a CanvasScaler and GraphicRaycaster only if not already present
                if (canvasObject.GetComponent<CanvasScaler>() == null)
                {
                    CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(1920, 1080);
                }
                if (canvasObject.GetComponent<GraphicRaycaster>() == null)
                {
                    canvasObject.AddComponent<GraphicRaycaster>();
                }

                // Create a single TextMeshProUGUI for all lines
                pointsInfoText = CreateTextElement(
                    "Style_PointsPBsText",
                    "", // Start with empty, will be set by UpdatePointsInfoText
                    new Vector2(0, -40)
                );
                UpdatePointsInfoText();

                // ---------- Register with UI Configurator ----------
                if (_uiRectTransform != null)
                    UIApi.AddToConfigurator(_uiRectTransform);

            }
        }

        // Helper method to create TextMeshProUGUI elements
        private TextMeshProUGUI CreateTextElement(string name, string textContent, Vector2 position)
        {
            textObject = new GameObject(name);
            textObject.transform.SetParent(canvas.transform);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = position;

            rectTransform.sizeDelta = new Vector2(400, 75); // Set size of the text box

            // Keep the rect so the player can move/scale it
            _uiRectTransform = rectTransform;

            TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
            textMesh.text = textContent;
            //textMesh.fontSize = 15;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableAutoSizing = true;

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

        public void UpdatePointsInfoText()
        {
            if (pointsInfoText == null || trickPointsManager == null)
                return;

            pointsInfoText.text =
                "<#daed4a><b>Stylepoints PBs</b></color>\n" +
                $"All Time: {trickPointsManager.bestPbAllTime}\n" +
                $"Current Session: {trickPointsManager.bestPbCurrentSession}\n" +
                $"Current Run: {trickPointsManager.totalRunPoints}";
        }

        public void DestroyComponent()
        {
            // Unregister from UI Configurator
            if(_uiRectTransform != null)
{
                UIApi.RemoveFromConfigurator(_uiRectTransform);
                _uiRectTransform = null;
            }


            // Destroy only your own UI elements, not the canvas
            if (pointsInfoText != null)
            {
                GameObject.Destroy(pointsInfoText.gameObject);
                pointsInfoText = null;
            }
            textObject = null;

            canvas = null;
            canvasObject = null;
        }

        private void OnDestroy()
        {
            PatchPauseHandler_Pause.OnPause -= PatchPauseHandler_OnPause_OnUnpause;
            PatchPauseHandler_Unpause.OnUnpause -= PatchPauseHandler_OnPause_OnUnpause;
        }
    }
}
