using TMPro;
using UnityEngine;
using ZeepSDK.UI;
using ZeepStyle.Patches;
using ZeepStyle.PointsManager;
using ZeepStyle.TrickManager;
using ZeepStyle.UIHelpers;

namespace ZeepStyle.PointsUIManager;

public class StylePointsUIManager : MonoBehaviour
{
    public TextMeshProUGUI pointsInfoText;
    private Canvas canvas;

    private GameObject canvasObject;

    private bool isPaused;
    private StyleTrickManager trickManager;

    // Trick Points
    private StyleTrickPointsManager trickPointsManager;

    private RectTransform uiRectTransform;

    private void Awake()
    {
        //PatchOnlineChatUI_OnOpen.OnOpenChat += PatchOnlineChatUI_OnOpen_OnClose;
        PatchPauseHandlerPause.OnPause += PatchPauseHandler_OnPause_OnUnpause;
        PatchPauseHandlerUnpause.OnUnpause += PatchPauseHandler_OnPause_OnUnpause;
    }

    private void Start()
    {
        trickPointsManager = FindObjectOfType<StyleTrickPointsManager>();
        trickManager = FindObjectOfType<StyleTrickManager>();
    }

    private void Update()
    {
        if (!trickManager.isPlayerSpawned || isPaused) return;
        switch (ModConfig.DisplayPBs.Value)
        {
            case true when !canvasObject:
                CreateUI();
                break;
            case false when canvasObject:
                DestroyComponent();
                break;
        }
        if (Input.GetKeyDown(ModConfig.DisplayPBsBind.Value) && !trickManager.isInPhotomode) TogglePointsUI();
    }

    private void OnDestroy()
    {
        PatchPauseHandlerPause.OnPause -= PatchPauseHandler_OnPause_OnUnpause;
        PatchPauseHandlerUnpause.OnUnpause -= PatchPauseHandler_OnPause_OnUnpause;
    }

    private void PatchPauseHandler_OnPause_OnUnpause(PauseHandler obj)
    {
        isPaused = obj.IsPaused;

        if (isPaused)
            HideText();
        else
            ShowText();
    }

    public void TogglePointsUI()
    {
        if (ModConfig.DisplayPBs.Value)
        {
            ModConfig.DisplayPBs.Value = false;
            DestroyComponent();
        }
        else
        {
            ModConfig.DisplayPBs.Value = true;
            CreateUI();
        }
    }

    public void HideText()
    {
        if (pointsInfoText) pointsInfoText.text = ""; // Clear the text to hide it
    }

    public void ShowText()
    {
        if (pointsInfoText) UpdatePointsInfoText();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void CreateUI()
    {
        if (!ModConfig.DisplayPBs.Value || !trickManager.isPlayerSpawned) return;
        // Clone the existing main Canvas
        canvasObject = StyleUIHelpers.CloneMainCanvas("Style_PBPointsCanvas");

        if (!canvasObject) return;
        canvas = canvasObject.GetComponent<Canvas>();

        (pointsInfoText, uiRectTransform) = StyleUIHelpers.CreateTextElement(
            canvas,
            "Style_PointsPBsText",
            "",
            new Vector2(0, -40),
            new Vector2(400, 75)
        );
        UpdatePointsInfoText();

        // ---------- Register with UI Configurator ----------
        if (uiRectTransform)
            UIApi.AddToConfigurator(uiRectTransform);
    }

    public void UpdatePointsInfoText()
    {
        if (!pointsInfoText || !trickPointsManager)
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
        if (uiRectTransform)
        {
            UIApi.RemoveFromConfigurator(uiRectTransform);
            uiRectTransform = null;
        }

        // Destroy only your own UI elements, not the canvas
        if (pointsInfoText)
        {
            Destroy(pointsInfoText.gameObject);
            pointsInfoText = null;
        }

        if (canvasObject)
        {
            Destroy(canvasObject);
            canvasObject = null;
        }

        canvas = null;
    }
}