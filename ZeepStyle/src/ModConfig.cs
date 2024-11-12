using BepInEx.Configuration;
using FMOD;
using UnityEngine;

namespace ZeepStyle.src
{
    public class ModConfig : MonoBehaviour
    {
        public static ConfigEntry<bool> displayPBs;
        public static ConfigEntry<KeyCode> displayPBsBind;
        public static ConfigEntry<int> tricks_SFX_volume;

        // Constructor that takes a ConfigFile instance from the main class
        public static void Initialize(ConfigFile config)
        {
            displayPBs = config.Bind("UI", "Display PBs", false,
                                            "Display the points PBs");
            displayPBsBind = config.Bind("UI", "Display PBs key", KeyCode.M, "Key to display the points PBs");

            tricks_SFX_volume = config.Bind("Audio","SFX Volume", 100, new ConfigDescription("SFX volume [0-100]", new AcceptableValueRange<int>(0, 100)));
        }
    }
}

