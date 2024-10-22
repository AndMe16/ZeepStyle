using System;
using HarmonyLib;
using UnityEngine;
using ZeepStyle;

[HarmonyPatch(typeof(GameMaster), "GetResults2")]
public class Patch_GetResults2
{
    // Event to notify when entering the method and checking the condition
    public static event Action<bool> OnGetResults2Entered;

    // Harmony Prefix: Executes before the original method
    static void Prefix(GameMaster __instance)
    {
        // Assuming __instance is the current instance of the class where GetResults2 is located

        // Check if playerResults and racepoints are set correctly
        if (__instance.playerResults != null && __instance.playerResults.Count > 0)
        {
            bool racePointsMatch = __instance.playerResults[0].racepoints == __instance.racePoints;

            // Trigger the event, passing whether the race points match
            OnGetResults2Entered?.Invoke(racePointsMatch);

            if (racePointsMatch)
            {
                Plugin.Logger.LogInfo("Race points match condition met!");
            }
            else
            {
                Plugin.Logger.LogInfo("Entered GetResults2 method, but race points do not match.");
            }
        }
        else
        {
            Plugin.Logger.LogInfo("playerResults is null or empty, could not check race points.");
        }
    }
}
