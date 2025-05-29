using System;
using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(GameMaster), "RestartLevel")]
public class PatchRestartLevel
{
    public static event Action<GameMaster> OnRestart;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(GameMaster __instance)
    {
        OnRestart?.Invoke(__instance);
        //Plugin.Logger.LogInfo($"PatchRestartLevel: Restarting level");
    }
}