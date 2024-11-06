using System;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(PauseHandler), "Pause")]
    public class PatchPauseHandler_Pause
    {
        public static event Action<PauseHandler> OnPause;

        [HarmonyPostfix]
        static void Postfix(PauseHandler __instance)
        {
            OnPause?.Invoke(__instance);
            //Plugin.Logger.LogInfo($"PatchPauseHandler: IsPaused = {__instance.IsPaused}");
        }
    }

    [HarmonyPatch(typeof(PauseHandler), "Unpause")]
    public class PatchPauseHandler_Unpause
    {
        public static event Action<PauseHandler> OnUnpause;

        [HarmonyPostfix]
        static void Postfix(PauseHandler __instance)
        {
            OnUnpause?.Invoke(__instance);
            //Plugin.Logger.LogInfo($"PatchPauseHandler: IsPaused = {__instance.IsPaused}");
        }
    }
}
