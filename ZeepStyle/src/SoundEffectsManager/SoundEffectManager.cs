using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using ZeepStyle.src;

public class Style_SoundEffectManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;  // Prevent autoplay on load
    }

    public void PlaySound()
    {
        // Build the file path relative to the mod's directory
        string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string filePath = Path.Combine(modDirectory, "Resources/CD_29.ogg");

        StartCoroutine(LoadAndPlayAudio(filePath));
    }

    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        // Use "file:///" prefix for local files
        if (!File.Exists(filePath))
        {
            Plugin.Logger.LogError($"The file path {filePath} does not exists!");
            yield break;
        }
        string path = "file://localhost/" + filePath;
        Uri uri = new(path);
        Plugin.Logger.LogInfo("Loading audio from path: " + uri);

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS);

        yield return www.SendWebRequest();

        while (!www.isDone){
            Plugin.Logger.LogInfo("Waiting....");
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Plugin.Logger.LogError("Failed to load audio: " + www.error);
        }

        else if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip == null)
            {
                Plugin.Logger.LogError("AudioClip is null for some dumb reason!");
            }
            else if (clip.length == 0)
            {
                Plugin.Logger.LogError("AudioClip length is 0 for some dumb reason!");
                Plugin.Logger.LogInfo($"name:{clip.name} freq:{clip.frequency} samp:{clip.samples} chann:{clip.channels} len:{clip.length}");
            }
            else
            {
                audioSource.clip = clip;
                audioSource.volume = 1.0f; // Ensure this is set between 0.0f and 1.0f
                audioSource.spatialBlend = 0.0f;
                audioSource.loop = false;       // Set to loop if needed
                if (audioSource.clip != null && audioSource.clip.length > 0)
                {
                    audioSource.Play();
                    Plugin.Logger.LogInfo("Playing special trick sound");
                }
                else
                {
                    Debug.LogError("AudioClip not assigned to AudioSource!");
                }
            }

        }
    }

}

