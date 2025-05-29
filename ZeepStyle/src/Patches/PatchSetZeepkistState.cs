using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
public class PatchSetZeepkistState
{
    public static byte currentState;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(New_ControlCar __instance)
    {
        currentState = __instance.currentZeepkistState;
        // Plugin.Logger.LogInfo($"Zeepkist state changed to {currentState} (from SetZeepkistState)");
    }
}