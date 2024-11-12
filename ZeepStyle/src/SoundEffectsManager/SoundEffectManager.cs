using System;
using System.IO;
using FMOD;
using FMODUnity;
using UnityEngine;
using ZeepStyle.src;
using ZeepStyle.src.Patches;

public class Style_SoundEffectManager : MonoBehaviour
{
    private Sound sound;
    private ChannelGroup customChannelGroup;
    private Channel channel;
    private float globalVolume;
    private float baseVolume = 0.5f;

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
        FMOD.RESULT result = RuntimeManager.CoreSystem.createChannelGroup("CustomSounds", out customChannelGroup);
        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("FMOD failed to create custom channel group: " + result);
        }
    }

    private void LoadSounds()
    {
        // Build the file path relative to the mod's directory
        string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string filePath = Path.Combine(modDirectory, "Resources/SpecialTrickSound.ogg");

        // Create a sound instance from a file path
        FMOD.RESULT result = RuntimeManager.CoreSystem.createStream(filePath, FMOD.MODE.CREATESTREAM, out sound);

        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("FMOD failed to load audio file: " + result);
            return;
        }
    }

    public void PlaySound()
    {
        // Play the sound in the custom channel group
        FMOD.RESULT result = RuntimeManager.CoreSystem.playSound(sound, customChannelGroup, false, out channel);
        if (result != FMOD.RESULT.OK)
        {
            Plugin.Logger.LogError("FMOD failed to play sound: " + result);
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

    //public void StopAllSounds()
    //{
    //    // Stop all sounds in the custom group
    //    customChannelGroup.stop();
    //}

    public void Release()
    {
        // Release the sound and the custom channel group when no longer needed
        sound.release();
        customChannelGroup.release();
    }

    private void OnDestroy()
    {
        Release();
        PatchSetVolumeFromSettings.OnSettVolumeFromSettings -= SetGroupVolume;
    }
}

