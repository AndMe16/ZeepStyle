using System;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(GameMaster), "RestartLevel")]
    public class PatchRestartLevel
    {
        public static event Action<GameMaster> OnRestart;
        [HarmonyPostfix]
        static void Postfix(GameMaster __instance)
        {
            OnRestart?.Invoke(__instance);
            Plugin.Logger.LogInfo($"PatchRestartLevel: Restarting level");
        }
    }
}
