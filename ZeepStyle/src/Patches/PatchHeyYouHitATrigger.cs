using System;
using System.Linq;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(ReadyToReset), "HeyYouHitATrigger")]
    public class Patch_HeyYouHitATrigger
    {
        // Event to notify when entering the method and checking the condition
        public static event Action<bool> OnHeyYouHitATrigger;

        // Harmony Postfix: Executes after the original method
        [HarmonyPostfix]
        static void Postfix(ReadyToReset __instance, bool isFinish)
        {

            bool actuallyFinishedWithAllCPs = (isFinish && __instance.actuallyFinished) && (__instance.master.currentLevelMode.HasThisPlayerFinishedAccountingForRacepoints(PlayerManager.Instance.currentMaster.playerResults.First()));

            if (isFinish)
            {
                OnHeyYouHitATrigger?.Invoke(actuallyFinishedWithAllCPs);
            }
        }
    }
}