using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(SetupGame), "LoadOfflineLevel")]
    public class PatchLoadOfflineLevel
    {
        public static bool isTestLevel;
        [HarmonyPostfix]
        static void Postfix(SetupGame __instance)
        {
            isTestLevel = __instance.GlobalLevel.IsTestLevel;
            Plugin.Logger.LogInfo($"PatchLoadOfflineLevel: Is test level: {isTestLevel}");
        }
    }
}
