using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(SetupGame), "LoadOfflineLevel")]
public class PatchLoadOfflineLevel
{
    public static bool IsTestLevel;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(SetupGame __instance)
    {
        IsTestLevel = __instance.GlobalLevel.IsTestLevel;
        Plugin.logger.LogInfo($"PatchLoadOfflineLevel: Is test level: {IsTestLevel}");
    }
}