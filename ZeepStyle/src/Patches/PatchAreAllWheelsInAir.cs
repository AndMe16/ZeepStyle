using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(New_ControlCar), "AreAllWheelsInAir")]
    public class PatchAreAllWheelsInAir
    {
        public static bool IsInTheAir = false;
        [HarmonyPostfix]
        static void Postfix(ref bool __result)
        {
            IsInTheAir = __result;
        }
    }
}


