using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using UnityEngine;
using ZeepStyle.Patches;
using Random = UnityEngine.Random;

namespace ZeepStyle.SoundEffectsManager;

public class StyleSoundEffectManager : MonoBehaviour
{
    private static readonly CHANNELCONTROL_CALLBACK EndCallback = OnSoundEnd;
    private const float BaseVolume = 0.5f;

    private readonly List<string> simpleTricks = ["SimpleTrick_1_Sound", "SimpleTrick_2_Sound", "SimpleTrick_3_Sound"];
    private readonly Dictionary<string, List<Channel>> soundChannels = new();
    private readonly Dictionary<string, Sound> sounds = [];
    private const float SpecialTrick = 0.1f;
    private ChannelGroup customChannelGroup;
    private float globalVolume;

    private void Start()
    {
        // Create a custom channel group for all sounds managed by this script
        Create_CustomChannel();
        LoadSounds();
        PatchSetVolumeFromSettings.OnSettVolumeFromSettings += SetVolumeWithGlobal;
        ModConfig.tricksSfxVolume.SettingChanged += OnVolumeChanged;
        PatchLoadWaarden.OnLoadWaarden += SetGlobalVolume;
    }

    private void OnDestroy()
    {
        Release();
        PatchSetVolumeFromSettings.OnSettVolumeFromSettings -= SetGroupVolume;
    }


    private void SetGlobalVolume(GameSettingsScriptableObject @object)
    {
        SetVolumeWithGlobal(@object.audio_master);
        Plugin.logger.LogInfo($"Loading audio_master: {globalVolume}");
    }

    private void SetVolumeWithGlobal(float obj)
    {
        globalVolume = obj;
        SetGroupVolume((float)((float)ModConfig.tricksSfxVolume.Value / 100 * globalVolume * 0.01 * BaseVolume));
    }

    private void OnVolumeChanged(object sender, EventArgs e)
    {
        SetGroupVolume((float)((float)ModConfig.tricksSfxVolume.Value / 100 * globalVolume * 0.01 * BaseVolume));
    }

    private void Create_CustomChannel()
    {
        var result = RuntimeManager.CoreSystem.createChannelGroup("StylePointsCustomSounds", out customChannelGroup);
        if (result != RESULT.OK) Plugin.logger.LogError("FMOD failed to create custom channel group: " + result);
    }

    private void LoadSounds()
    {
        // Define the directory where your audio files are stored
        var modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var resourcesFolder = Path.Combine(modDirectory, "Resources");

        if (!Directory.Exists(resourcesFolder))
        {
            Plugin.logger.LogError("Resources folder not found.");
            return;
        }

        // Load all audio files in the folder
        var audioFiles = Directory.GetFiles(resourcesFolder, "*.ogg");

        foreach (var filePath in audioFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!File.Exists(filePath)) continue;
            MODE fmodMode;
            if (fileName == "HighSpeedSpin_Sound")
                fmodMode = MODE.CREATESTREAM | MODE.LOOP_NORMAL;
            else
                fmodMode = MODE.CREATESTREAM;
            // Create a sound instance for each file
            var result = RuntimeManager.CoreSystem.createStream(filePath, fmodMode, out var sound);

            if (result == RESULT.OK)
                sounds[fileName] = sound;
            // Plugin.Logger.LogInfo($"Loaded sound: {fileName}");
            else
                Plugin.logger.LogError($"FMOD failed to load audio file {fileName}: " + result);
        }
    }

    public void PlaySound(string soundName)
    {
        if (simpleTricks.Contains(soundName))
            if (Random.value <= SpecialTrick)
                soundName = "SpecialTrick_Sound";

        if (sounds.TryGetValue(soundName, out var sound))
        {
            // Play the sound in the custom channel group
            var result = RuntimeManager.CoreSystem.playSound(sound, customChannelGroup, false, out var channel);
            if (result != RESULT.OK)
                Plugin.logger.LogError($"FMOD failed to play sound {soundName}: " + result);
            else
            {
                // Register channel
                if (!soundChannels.ContainsKey(soundName))
                    soundChannels[soundName] = [];

                soundChannels[soundName].Add(channel);

                // Setup callback to clean it up automatically when it ends
                void Cleanup(Channel endedChannel)
                {
                    if (!soundChannels.TryGetValue(soundName, out var list)) return;
                    list.RemoveAll(ch => ch.hasHandle() && ch.handle == endedChannel.handle);
                    if (list.Count == 0) soundChannels.Remove(soundName);
                    // var channelCount = GetTotalChannelCount();
                    // Plugin.Logger.LogInfo($"Sound '{soundName}' ended. Remaining channels: {channelCount}");
                }

                // Store cleanup action in user data
                var handle = GCHandle.Alloc((Action<Channel>)Cleanup);
                channel.setUserData((IntPtr)handle);

                // Set the callback
                channel.setCallback(EndCallback);
            }
        }
        else
        {
            Plugin.logger.LogError($"Sound '{soundName}' not found.");
        }
    }

    public void SetGroupVolume(float volume)
    {
        var result = customChannelGroup.setVolume(volume);
        if (result != RESULT.OK) Plugin.logger.LogError("Failed to set volume for custom channel group: " + result);
        //else
        //{
        //    Plugin.Logger.LogInfo($"Set volume for custom channel group: {volume}");
        //}
    }

    public void SetSoundVolume(string soundName, float volume)
    {
        if (!soundChannels.TryGetValue(soundName, out var channels)) return;
        List<Channel> stillValid = [];

        foreach (var channel in channels)
        {
            var check = channel.isPlaying(out var isPlaying);
            if (check != RESULT.OK || !isPlaying) continue;
            var result = channel.setVolume(volume);
            if (result != RESULT.OK)
                Plugin.logger.LogError($"Failed to set volume for sound: {soundName}, result: {result}");

            stillValid.Add(channel); // keep valid ones
        }

        // Replace with pruned list
        soundChannels[soundName] = stillValid;
        // Plugin.Logger.LogWarning("Sound not found: " + soundName);
    }


    public void StopSound(string soundName)
    {
        if (!soundChannels.TryGetValue(soundName, out var channels)) return;
        List<Channel> toRemove = [];

        foreach (var channel in channels)
        {
            var resultIsPlaying = channel.isPlaying(out var isPlaying);
            if (resultIsPlaying == RESULT.OK && isPlaying) toRemove.Add(channel);
        }

        foreach (var resultStop in toRemove.Select(channel => channel.stop()).Where(resultStop => resultStop != RESULT.OK))
        {
            Plugin.logger.LogError($"Failed to stop sound: {soundName}, result: {resultStop}");
        }
    }

    private static RESULT OnSoundEnd(
        IntPtr channelControl,
        CHANNELCONTROL_TYPE controlType,
        CHANNELCONTROL_CALLBACK_TYPE callbackType,
        IntPtr commandData1,
        IntPtr commandData2)
    {
        if (callbackType != CHANNELCONTROL_CALLBACK_TYPE.END ||
            controlType != CHANNELCONTROL_TYPE.CHANNEL) return RESULT.OK;
        var channel = new Channel(channelControl);

        channel.getUserData(out var userData);
        if (userData == IntPtr.Zero) return RESULT.OK;
        var handle = (GCHandle)userData;
        if (handle.Target is not Action<Channel> cleanup) return RESULT.OK;
        cleanup(channel);
        handle.Free();

        return RESULT.OK;
    }

    /*private int GetTotalChannelCount()
    {
        var result = customChannelGroup.getNumChannels(out var numChannels);
        if (result == RESULT.OK) return numChannels;
        Plugin.logger.LogError("Failed to get number of channels in group: " + result);
        return 0;

    }*/


    public void Release()
    {
        // Release each sound in the dictionary
        foreach (var sound in sounds.Values) sound.release();

        // Clear the dictionary and release the channel group
        sounds.Clear();
        customChannelGroup.release();
    }
}