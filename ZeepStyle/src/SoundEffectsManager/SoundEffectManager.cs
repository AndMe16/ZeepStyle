using UnityEngine;

public class Style_SoundEffectManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlaySound()
    {
        AudioClip clip = Resources.Load<AudioClip>("CD_00028");
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Audio clip not found in Resources!");
        }
    }
}
