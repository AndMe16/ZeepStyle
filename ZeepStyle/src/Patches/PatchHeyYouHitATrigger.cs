using System;
using System.Linq;
using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(ReadyToReset), "HeyYouHitATrigger")]
public class PatchHeyYouHitATrigger
{
    // Event to notify when entering the method and checking the condition
    public static event Action<bool> OnHeyYouHitATrigger;

    // Harmony Postfix: Executes after the original method
    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(ReadyToReset __instance, bool isFinish)
    {
        var actuallyFinishedWithAllCPs = isFinish && __instance.actuallyFinished &&
                                         __instance.master.currentLevelMode
                                             .HasThisPlayerFinishedAccountingForRacepoints(PlayerManager.Instance
                                                 .currentMaster.playerResults.First());

        if (isFinish) OnHeyYouHitATrigger?.Invoke(actuallyFinishedWithAllCPs);
    }
}