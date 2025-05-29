using System;
using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(PauseHandler), "Pause")]
public class PatchPauseHandlerPause
{
    public static event Action<PauseHandler> OnPause;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(PauseHandler __instance)
    {
        OnPause?.Invoke(__instance);
        //Plugin.Logger.LogInfo($"PatchPauseHandler: IsPaused = {__instance.IsPaused}");
    }
}

[HarmonyPatch(typeof(PauseHandler), "Unpause")]
public class PatchPauseHandlerUnpause
{
    public static event Action<PauseHandler> OnUnpause;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(PauseHandler __instance)
    {
        OnUnpause?.Invoke(__instance);
        //Plugin.Logger.LogInfo($"PatchPauseHandler: IsPaused = {__instance.IsPaused}");
    }
}