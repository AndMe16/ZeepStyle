using System;
using HarmonyLib;

namespace ZeepStyle.Patches;

[HarmonyPatch(typeof(FMOD_Manager), "SetVolumeFromSettings")]
public class PatchSetVolumeFromSettings
{
    public static event Action<float> OnSettVolumeFromSettings;

    [HarmonyPostfix]
    // ReSharper disable once UnusedMember.Local
    private static void Postfix(GameSettingsScriptableObject tempSettings)
    {
        OnSettVolumeFromSettings?.Invoke(tempSettings.audio_master);
    }
}