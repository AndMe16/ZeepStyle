using BepInEx.Configuration;
using UnityEngine;

namespace ZeepStyle.src
{
    public class ModConfig : MonoBehaviour
    {
        public static ConfigEntry<bool> displayPBs;
        public static ConfigEntry<KeyCode> displayPBsBind;
        public static ConfigEntry<int> tricks_SFX_volume;
        public static ConfigEntry<bool> tricksDetectionOn;

        // Constructor that takes a ConfigFile instance from the main class
        public static void Initialize(ConfigFile config)
        {
            displayPBs = config.Bind("3. UI", "3.1. Display PBs", false,
                                            "Display the points PBs");
            displayPBsBind = config.Bind("3. UI", "3.2. Display PBs key", KeyCode.L, "Key to display the points PBs");

            tricks_SFX_volume = config.Bind("2. Audio", "2.1. SFX Volume", 100, new ConfigDescription("SFX volume [0-100]", new AcceptableValueRange<int>(0, 100)));

            tricksDetectionOn = config.Bind("1. General", "1.1 Detect Tricks", true, "Enable/Disable the detection of tricks");
        }
    }
}

