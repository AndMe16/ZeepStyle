using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using UnityEngine;
using ZeepStyle.src;
using ZeepStyle.src.Patches;

public class Style_SoundEffectManager : MonoBehaviour
{
    private Dictionary<string, Sound> sounds = [];
    private ChannelGroup customChannelGroup;
    private Dictionary<string, List<FMOD.Channel>> soundChannels = new();
    private float globalVolume;
    private readonly float baseVolume = 0.5f;
    private readonly float specialTrick = 0.1f;

    private readonly List<string> simpleTricks = ["SimpleTrick_1_Sound", "SimpleTrick_2_Sound", "SimpleTrick_3_Sound"];

    private static readonly CHANNELCONTROL_CALLBACK endCallback = OnSoundEnd;

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

            if (File.Exists(filePath))
            {
                MODE fmod_mode;
                if (fileName == "HighSpeedSpin_Sound")
                {
                    fmod_mode = FMOD.MODE.CREATESTREAM | FMOD.MODE.LOOP_NORMAL;
                }
                else
                {
                    fmod_mode = FMOD.MODE.CREATESTREAM;
                }
                // Create a sound instance for each file
                FMOD.RESULT result = RuntimeManager.CoreSystem.createStream(filePath, fmod_mode, out Sound sound);

                if (result == FMOD.RESULT.OK)
                {
                    sounds[fileName] = sound;
                    // Plugin.Logger.LogInfo($"Loaded sound: {fileName}");
                }
                else
                {
                    Plugin.Logger.LogError($"FMOD failed to load audio file {fileName}: " + result);
                }
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
                if (result == FMOD.RESULT.OK)
                {
                    // Register channel
                    if (!soundChannels.ContainsKey(soundName))
                        soundChannels[soundName] = new();

                    soundChannels[soundName].Add(channel);

                    // Setup callback to clean it up automatically when it ends
                    Action<FMOD.Channel> cleanup = (endedChannel) =>
                    {
                        if (soundChannels.TryGetValue(soundName, out var list))
                        {
                            int channelCount = GetTotalChannelCount();
                            list.RemoveAll(ch => ch.hasHandle() && ch.handle == endedChannel.handle);
                            if (list.Count == 0)
                                soundChannels.Remove(soundName);
                            // Plugin.Logger.LogInfo($"Sound '{soundName}' ended. Remaining channels: {channelCount}");

                        }
                    };

                    // Store cleanup action in user data
                    GCHandle handle = GCHandle.Alloc(cleanup);
                    channel.setUserData((IntPtr)handle);

                    // Set the callback
                    channel.setCallback(endCallback);
                }
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
        //else
        //{
        //    Plugin.Logger.LogInfo($"Set volume for custom channel group: {volume}");
        //}
    }

    public void SetSoundVolume(string soundName, float volume)
    {
        if (soundChannels.TryGetValue(soundName, out List<FMOD.Channel> channels))
        {
            List<FMOD.Channel> stillValid = new();

            foreach (var channel in channels)
            {
                FMOD.RESULT check = channel.isPlaying(out bool isPlaying);
                if (check == FMOD.RESULT.OK && isPlaying)
                {
                    var result = channel.setVolume(volume);
                    if (result != FMOD.RESULT.OK)
                    {
                        Plugin.Logger.LogError($"Failed to set volume for sound: {soundName}, result: {result}");
                    }

                    stillValid.Add(channel); // keep valid ones
                }
            }

            // Replace with pruned list
            soundChannels[soundName] = stillValid;
        }
        else
        {
            // Plugin.Logger.LogWarning("Sound not found: " + soundName);
        }
    }


    public void StopSound(string soundName)
    {
        if (soundChannels.TryGetValue(soundName, out List<FMOD.Channel> channels))
        {
            List<FMOD.Channel> toRemove = [];

            foreach (var channel in channels)
            {
                FMOD.RESULT result_isPlaying = channel.isPlaying(out bool isPlaying);
                if (result_isPlaying == FMOD.RESULT.OK && isPlaying)
                {
                    toRemove.Add(channel);
                }
            }
            foreach (var channel in toRemove)
            {
                FMOD.RESULT result_stop = channel.stop();
                if (result_stop != FMOD.RESULT.OK)
                {
                    Plugin.Logger.LogError($"Failed to stop sound: {soundName}, result: {result_stop}");
                }
            }
        }
    }

    private static FMOD.RESULT OnSoundEnd(
    IntPtr channelControl,
    FMOD.CHANNELCONTROL_TYPE controlType,
    FMOD.CHANNELCONTROL_CALLBACK_TYPE callbackType,
    IntPtr commandData1,
    IntPtr commandData2)
    {
        if (callbackType == FMOD.CHANNELCONTROL_CALLBACK_TYPE.END &&
            controlType == FMOD.CHANNELCONTROL_TYPE.CHANNEL)
        {
            FMOD.Channel channel = new FMOD.Channel(channelControl);

            channel.getUserData(out IntPtr userData);
            if (userData != IntPtr.Zero)
            {
                GCHandle handle = (GCHandle)userData;
                if (handle.Target is Action<FMOD.Channel> cleanup)
                {
                    cleanup(channel);
                    handle.Free();
                }
            }
        }

        return FMOD.RESULT.OK;
    }

    public int GetTotalChannelCount()
    {
        FMOD.RESULT result = customChannelGroup.getNumChannels(out int numChannels);
        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("Failed to get number of channels in group: " + result);
            return 0;
        }

        return numChannels;
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

