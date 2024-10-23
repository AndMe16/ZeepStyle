using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using ZeepStyle;

[HarmonyPatch(typeof(ReadyToReset), "HeyYouHitATrigger")]
public class Patch_HeyYouHitATrigger
{
    // Event to notify when entering the method and checking the condition
    public static event Action<bool> OnHeyYouHitATrigger;

    // Harmony Postfix: Executes after the original method
    static void Postfix(ReadyToReset __instance, bool isFinish)
    {
        if ((isFinish && __instance.actuallyFinished) && (__instance.master.currentLevelMode.HasThisPlayerFinishedAccountingForRacepoints(PlayerManager.Instance.currentMaster.playerResults.First())))
        {
            //Plugin.Logger.LogInfo("Actually finished the race!");
            OnHeyYouHitATrigger?.Invoke(__instance.actuallyFinished);
        }
    }
}
