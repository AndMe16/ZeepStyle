using HarmonyLib;
using System.Reflection;
using ZeepStyle.src;


namespace ZeepStyle.src.Patches
{
    [HarmonyPatch]
    class Patch_UIConfigurator_GetPlaceholderText
    {
        static MethodBase TargetMethod()
        {
            var assembly = typeof(ZeepSDK.UI.UIApi).Assembly;

            var type = assembly.GetType("ZeepSDK.UI.UIConfigurator", false); // Replace with actual full type name
            if (type == null)
            {
                Plugin.Logger.LogError("UIConfigurator not found.");
                return null;
            }
            return type.GetMethod("GetPlaceholderText", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static bool Prefix(UnityEngine.RectTransform rect, ref string __result)
        {
            string rectName = rect.name;

            switch (rectName)
            {
                case "Style_PointsPBsText":
                    Plugin.Logger.LogInfo("GetPlaceholderText: Style_PointsPBsText");
                    __result = "<#daed4a><b>Stylepoints PBs (Placeholder Text)</b></color>\n" +
                    $"All Time: 0\n" +
                    $"Current Session: 0\n" +
                    $"Current Run: 0";

                    return false;

                case "Style_TricksDisplay":
                    //Plugin.Logger.LogInfo("GetPlaceholderText: Style_TricksDisplay");
                    __result = """
                        <color=#FFFFFF9B><size=10>180 Spin (+50)</size></color>
                        <color=#FFFFFFB4><size=13>180 Spin (+50)</size></color>
                        <color=#FFFFFFCD><size=17>180 Spin (+50)</size></color>
                        <color=#FFFFFFE6><size=21>180 Spin (+50)</size></color>
                        <color=#FFFFFFFF><size=25>180 Spin (+50)</size></color>
                        <color=#f7e520><b><size=25>+250</b>
                        """;
                    return false;
            }

            return true; // Let original run
        }
    }
}
