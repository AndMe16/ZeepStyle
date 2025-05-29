using System;
using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(Instellingen), "LoadWaarden")]
public class PatchLoadWaarden
{
    public static event Action<GameSettingsScriptableObject> OnLoadWaarden;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(Instellingen __instance)
    {
        OnLoadWaarden?.Invoke(__instance.GlobalSettings);
        //Plugin.Logger.LogInfo("Getting RB");
    }
}