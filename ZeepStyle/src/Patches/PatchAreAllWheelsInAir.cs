using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(New_ControlCar), "AreAllWheelsInAir")]
public class PatchAreAllWheelsInAir
{
    public static bool IsInTheAir;

    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Local
    private static void Postfix(ref bool __result)
    {
        IsInTheAir = __result;
    }
}