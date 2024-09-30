using HarmonyLib;
using UnityEngine;

namespace ZeepStyle
{
    [HarmonyPatch(typeof(New_ControlCar), "AreAllWheelsInAir")]
    public class PatchAreAllWheelsInAir
    {
        public static bool IsInTheAir = false;
        [HarmonyPostfix]
        static void Postfix(New_ControlCar __instance, ref bool __result)
        {
            IsInTheAir = __result;
        }
    }
}


