using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMODUnity;
using UnityEngine;
using ZeepStyle.src;
using ZeepStyle.src.Patches;

public class Style_SoundEffectManager : MonoBehaviour
{
    private Dictionary<string, Sound> sounds = [];
    private ChannelGroup customChannelGroup;
    private Dictionary<string, FMOD.Channel> soundChannels = [];
    private float globalVolume;
    private readonly float baseVolume = 0.5f;
    private readonly float specialTrick = 0.1f;

    private readonly List<string> simpleTricks = ["SimpleTrick_1_Sound", "SimpleTrick_2_Sound", "SimpleTrick_3_Sound"];

    void Start()
    {
        // Create a custom channel group for all sounds managed by this script
        Create_CustomChannel();
        LoadSounds();
        PatchSetVolumeFromSettings.OnSettVolumeFromSettings += SetVolumeWithGlobal;
        ModConfig.tricks_SFX_volume.SettingChanged += OnVolumeChanged;
        PatchLoadWaarden.OnLoadWaarden += SetGlobalVolume;
    }


    private void SetGlobalVolume(GameSettingsScriptableObject @object)
    {
        SetVolumeWithGlobal(@object.audio_master);
        Plugin.Logger.LogInfo($"Loading audio_master: {globalVolume}");
    }

    private void SetVolumeWithGlobal(float obj)
    {
        globalVolume = obj;
        SetGroupVolume((float)(((float)ModConfig.tricks_SFX_volume.Value / 100) * globalVolume * 0.01 * baseVolume));
    }

    private void OnVolumeChanged(object sender, EventArgs e)
    {
        SetGroupVolume((float)(((float)ModConfig.tricks_SFX_volume.Value / 100) * globalVolume * 0.01 * baseVolume));
    }

    private void Create_CustomChannel()
    {
        FMOD.RESULT result = RuntimeManager.CoreSystem.createChannelGroup("StylePointsCustomSounds", out customChannelGroup);
        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("FMOD failed to create custom channel group: " + result);
        }
    }

    private void LoadSounds()
    {
        // Define the directory where your audio files are stored
        string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string resourcesFolder = Path.Combine(modDirectory, "Resources");

        if (!Directory.Exists(resourcesFolder))
        {
            Plugin.Logger.LogError("Resources folder not found.");
            return;
        }

        // Load all audio files in the folder
        string[] audioFiles = Directory.GetFiles(resourcesFolder, "*.ogg");

        foreach (var filePath in audioFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Create a sound instance for each file
            FMOD.RESULT result = RuntimeManager.CoreSystem.createStream(filePath, FMOD.MODE.CREATESTREAM, out Sound sound);

            if (result == FMOD.RESULT.OK)
            {
                sounds[fileName] = sound;
                Plugin.Logger.LogInfo($"Loaded sound: {fileName}");
            }
            else
            {
                Plugin.Logger.LogError($"FMOD failed to load audio file {fileName}: " + result);
            }
        }
    }

    public void PlaySound(string soundName)
    {
        if (simpleTricks.Contains(soundName))
        {
            if (UnityEngine.Random.value <= specialTrick)
            {
                soundName = "SpecialTrick_Sound";
            }
        }
        if (sounds.TryGetValue(soundName, out Sound sound))
        {
            // Play the sound in the custom channel group
            FMOD.RESULT result = RuntimeManager.CoreSystem.playSound(sound, customChannelGroup, false, out FMOD.Channel channel);
            if (result != FMOD.RESULT.OK)
            {
                Plugin.Logger.LogError($"FMOD failed to play sound {soundName}: " + result);
            }
            else
            {
                soundChannels[soundName] = channel;
            }
        }
        else
        {
            Plugin.Logger.LogError($"Sound '{soundName}' not found.");
        }
    }

    public void SetGroupVolume(float volume)
    {
        FMOD.RESULT result = customChannelGroup.setVolume(volume);
        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("Failed to set volume for custom channel group: " + result);
        }
    }

    public void SetSoundVolume(string soundName, float volume)
    {
        if (soundChannels.TryGetValue(soundName, out FMOD.Channel channel))
        {
            FMOD.RESULT result = channel.setVolume(volume);
            if (result != FMOD.RESULT.OK)
            {
                Plugin.Logger.LogError("Failed to set volume for sound: " + soundName + ", result: " + result);
            }
        }
        else
        {
            Plugin.Logger.LogWarning("Sound not found: " + soundName);
        }
    }

    public void StopSound(string soundName)
    {
        if (soundChannels.TryGetValue(soundName, out FMOD.Channel channel))
        {
            FMOD.RESULT result = channel.isPlaying(out bool isPlaying);

            if (result != FMOD.RESULT.OK || isPlaying)
            {
                result = channel.stop();
                if (result != FMOD.RESULT.OK)
                {
                    Plugin.Logger.LogError($"Failed to stop {soundName}: {result}");
                }
                soundChannels.Remove(soundName);
            }
        }
        else
        {
            //Plugin.Logger.LogWarning("Sound not found: " + soundName);
        }
    }

    public void CleanupInactiveChannels()
    {
        List<string> toRemove = [];

        foreach (var entry in soundChannels)
        {
            FMOD.Channel channel = entry.Value;
            FMOD.RESULT result = channel.isPlaying(out bool isPlaying);

            if (result != FMOD.RESULT.OK || !isPlaying)
            {
                toRemove.Add(entry.Key);
            }
        }

        foreach (string soundName in toRemove)
        {
            soundChannels.Remove(soundName);
        }
    }

    public void Release()
    {
        // Release each sound in the dictionary
        foreach (var sound in sounds.Values)
        {
            sound.release();
        }

        // Clear the dictionary and release the channel group
        sounds.Clear();
        customChannelGroup.release();
    }

    private void OnDestroy()
    {
        Release();
        PatchSetVolumeFromSettings.OnSettVolumeFromSettings -= SetGroupVolume;
    }
}

