using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
    public class PatchSetZeepkistState
    {
        public static byte currentState;
        [HarmonyPostfix]
        static void Postfix(New_ControlCar __instance, byte newState)
        {
            currentState = __instance.currentZeepkistState;
            // Plugin.Logger.LogInfo($"Zeepkist state changed to {currentState} (from SetZeepkistState)");
        }
    }
}
