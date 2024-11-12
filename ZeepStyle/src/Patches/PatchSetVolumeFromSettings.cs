using System;
using HarmonyLib;

namespace ZeepStyle.src.Patches
{
    [HarmonyPatch(typeof(FMOD_Manager), "SetVolumeFromSettings")]
    public class PatchSetVolumeFromSettings
    {
        public static event Action<float> OnSettVolumeFromSettings;

        [HarmonyPostfix]
        static void Postfix(GameSettingsScriptableObject tempSettings)
        {
            OnSettVolumeFromSettings?.Invoke(tempSettings.audio_master);
        }
    }
}