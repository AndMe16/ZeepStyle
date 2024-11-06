using BepInEx.Configuration;
using UnityEngine;

namespace ZeepStyle.src
{
    public class ModConfig : MonoBehaviour
    {
        public static ConfigEntry<bool> displayPBs;
        public static ConfigEntry<KeyCode> displayPBsBind;

        // Constructor that takes a ConfigFile instance from the main class
        public static void Initialize(ConfigFile config)
        {
            displayPBs = config.Bind("UI", "Display PBs", false,
                                            "Display the points PBs");
            displayPBsBind = config.Bind("UI", "Display PBs key", KeyCode.M, "Key to display the points PBs");
        }
    }
}

