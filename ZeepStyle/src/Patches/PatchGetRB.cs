using HarmonyLib;
using UnityEngine;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(New_ControlCar), "GetRB")]
public class PatchGetRb
{
    public static Rigidbody rb;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static void Postfix(ref Rigidbody __result)
    {
        rb = __result;
        //Plugin.Logger.LogInfo("Getting RB");
    }
}