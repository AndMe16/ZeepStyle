using TMPro;
using UnityEngine;
using ZeepSDK.UI;
using ZeepStyle.src.Patches;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.TrickManager;
using ZeepStyle.src.UIHelpers;

namespace ZeepStyle.src.PointsUIManager
{
    public class Style_PointsUIManager : MonoBehaviour
    {
        private Canvas canvas;
        public TextMeshProUGUI pointsInfoText;

        private RectTransform _uiRectTransform;

        GameObject canvasObject;

        // Trick Points
        Style_TrickPointsManager trickPointsManager;
        Style_TrickManager trickManager;

        bool isPaused = false;

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
                pointsInfoText.text = "";  // Clear the text to hide it
            }
        }

        public void ShowText()
        {
            if (pointsInfoText != null)
            {
                UpdatePointsInfoText();
            }
        }

        public void CreateUI()
        {
            if (ModConfig.displayPBs.Value && trickManager.isPlayerSpawned)
            {
                // Clone the existing main Canvas
                canvasObject = Style_UIHelpers.CloneMainCanvas("Style_PBPointsCanvas");

                if (canvasObject == null)
                {
                    return;
                }
                canvas = canvasObject.GetComponent<Canvas>();

                (pointsInfoText, _uiRectTransform) = Style_UIHelpers.CreateTextElement(
                    canvas,
                    "Style_PointsPBsText",
                    "",
                    new Vector2(0, -40),
                    new Vector2(400, 75)
                );
                UpdatePointsInfoText();

                // ---------- Register with UI Configurator ----------
                if (_uiRectTransform != null)
                    UIApi.AddToConfigurator(_uiRectTransform);
            }
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
            if (_uiRectTransform != null)
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

            if (canvasObject != null)
            {
                GameObject.Destroy(canvasObject);
                canvasObject = null;
            }

            canvas = null;
        }

        private void OnDestroy()
        {
            PatchPauseHandler_Pause.OnPause -= PatchPauseHandler_OnPause_OnUnpause;
            PatchPauseHandler_Unpause.OnUnpause -= PatchPauseHandler_OnPause_OnUnpause;
        }
    }
}
