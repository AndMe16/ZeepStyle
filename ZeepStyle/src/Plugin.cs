using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ZeepStyle;

[BepInPlugin("andme123.zeepstyle", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        harmony = new Harmony("andme123.zeepstyle");
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {"andme123.zeepstyle"} is loaded!");
        gameObject.AddComponent<Style_TrickManager>();
        gameObject.AddComponent<Style_Yaw>();
        gameObject.AddComponent<Style_Pitch>();
        gameObject.AddComponent<Style_Roll>();
        //gameObject.AddComponent<Style_GizmoVisualization>();
        gameObject.AddComponent<Style_TrickDisplay>();
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }
}
