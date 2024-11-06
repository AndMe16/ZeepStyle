using HarmonyLib;
using UnityEngine;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(New_ControlCar), "GetRB")]
    public class PatchGetRB
    {
        public static Rigidbody Rb;
        [HarmonyPostfix]
        static void Postfix(New_ControlCar __instance, ref Rigidbody __result)
        {
            Rb = __result;
            //Plugin.Logger.LogInfo("Getting RB");
        }
    }
}


