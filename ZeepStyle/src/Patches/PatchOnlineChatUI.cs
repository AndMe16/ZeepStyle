using System;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(OnlineChatUI), nameof(OnlineChatUI.Awake))]
    public class PatchOnlineChatUI
    {
        public static event Action<OnlineChatUI> Awake;

        private static void Postfix(OnlineChatUI __instance)
        {
            Awake?.Invoke(__instance);
        }
    }
}