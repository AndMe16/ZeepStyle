using System;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(PauseHandler), nameof(PauseHandler.Awake))]
    public class PatchPauseHandler
    {
        public static event Action<PauseHandler> Awake;

        private static void Postfix(PauseHandler __instance)
        {
            Awake?.Invoke(__instance);
        }
    }
}
