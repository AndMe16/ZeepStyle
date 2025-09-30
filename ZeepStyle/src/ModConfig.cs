using BepInEx.Configuration;
using UnityEngine;

namespace ZeepStyle;

public class ModConfig : MonoBehaviour
{
    public static ConfigEntry<bool> DisplayPBs;
    public static ConfigEntry<KeyCode> DisplayPBsBind;
    public static ConfigEntry<int> TricksSfxVolume;
    public static ConfigEntry<bool> TricksDetectionOn;

    // Constructor that takes a ConfigFile instance from the main class
    public static void Initialize(ConfigFile config)
    {
        DisplayPBs = config.Bind("3. UI", "3.1. Display PBs", false,
            "Display the points PBs");
        DisplayPBsBind = config.Bind("3. UI", "3.2. Display PBs key", KeyCode.L, "Key to display the points PBs");

        TricksSfxVolume = config.Bind("2. Audio", "2.1. SFX Volume", 100,
            new ConfigDescription("SFX volume [0-100]", new AcceptableValueRange<int>(0, 100)));

        TricksDetectionOn =
            config.Bind("1. General", "1.1 Detect Tricks", true, "Enable/Disable the detection of tricks");
    }
}