using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ZeepSDK.UI;

namespace ZeepStyle.Patches;

[HarmonyPatch]
internal class PatchUIConfiguratorGetPlaceholderText
{
    // ReSharper disable once UnusedMember.Local
    private static MethodBase TargetMethod()
    {
        var assembly = typeof(UIApi).Assembly;

        var type = assembly.GetType("ZeepSDK.UI.UIConfigurator", false);
        if (type != null) return type.GetMethod("GetPlaceholderText", BindingFlags.NonPublic | BindingFlags.Instance);
        Plugin.logger.LogError("UIConfigurator not found.");
        return null;

    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    private static bool Prefix(RectTransform rect, ref string __result)
    {
        var rectName = rect.name;

        switch (rectName)
        {
            case "Style_PointsPBsText":
                Plugin.logger.LogInfo("GetPlaceholderText: Style_PointsPBsText");
                __result = "<#daed4a><b>Stylepoints PBs (Placeholder Text)</b></color>\n" +
                           "All Time: 0\n" +
                           "Current Session: 0\n" +
                           "Current Run: 0";

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

        return true; // Let the original run
    }
}