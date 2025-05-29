using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ZeepStyle.PointsManager;
using ZeepStyle.PointsUIManager;
using ZeepStyle.SoundEffectsManager;
using ZeepStyle.TrickDisplayManager;
using ZeepStyle.TrickManager;
using ZeepStyle.Tricks;

namespace ZeepStyle;

[BepInPlugin("andme123.zeepstyle", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource logger;
    private Harmony harmony;

    public static Plugin Instance { get; private set; }

    private void Awake()
    {
        // Plugin startup logic
        Instance = this; // Assign the static instance
        logger = Logger;
        harmony = new Harmony("andme123.zeepstyle");
        harmony.PatchAll();
        logger.LogInfo("Plugin andme123.zeepstyle is loaded!");
        ModConfig.Initialize(Config);
        gameObject.AddComponent<StyleTrickManager>();
        gameObject.AddComponent<StyleYaw>();
        gameObject.AddComponent<StylePitch>();
        gameObject.AddComponent<StyleRoll>();
        //gameObject.AddComponent<Style_GizmoVisualization>();
        gameObject.AddComponent<StyleTrickDisplay>();
        gameObject.AddComponent<StyleTrickPointsManager>();
        gameObject.AddComponent<StylePointsUIManager>();
        gameObject.AddComponent<StyleSoundEffectManager>();
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }
}