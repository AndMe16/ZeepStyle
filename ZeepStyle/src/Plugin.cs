using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ZeepStyle.src.Patches;
using ZeepStyle.src.PointsManager;
using ZeepStyle.src.PointsUIManager;
using ZeepStyle.src.TrickDisplayManager;
using ZeepStyle.src.TrickManager;
using ZeepStyle.src.Tricks;

namespace ZeepStyle.src
{
    [BepInPlugin("andme123.zeepstyle", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ZeepSDK")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;
        internal static new ManualLogSource Logger;

        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            // Plugin startup logic
            Instance = this; // Assign the static instance
            Logger = base.Logger;
            harmony = new Harmony("andme123.zeepstyle");
            harmony.PatchAll();
            foreach (MethodBase method in harmony.GetPatchedMethods())
            {
                Plugin.Logger.LogInfo(method.Name);
            }
            Logger.LogInfo($"Plugin {"andme123.zeepstyle"} is loaded!");
            ModConfig.Initialize(Config);
            gameObject.AddComponent<Style_TrickManager>();
            gameObject.AddComponent<Style_Yaw>();
            gameObject.AddComponent<Style_Pitch>();
            gameObject.AddComponent<Style_Roll>();
            //gameObject.AddComponent<Style_GizmoVisualization>();
            gameObject.AddComponent<Style_TrickDisplay>();
            gameObject.AddComponent<Style_TrickPointsManager>();
            gameObject.AddComponent<Style_PointsUIManager>();
            gameObject.AddComponent<Style_SoundEffectManager>();
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            harmony = null;
        }
    }
}
