using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public float volume = 1f;

    private bool isPaused = false;
    private float previousVolume;




    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.volume = volume;
            audioSource.Play();
            isPaused = false;
        }
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            previousVolume = audioSource.volume;
            audioSource.volume = 0f;
            audioSource.Pause();
            isPaused = true;
        }
    }

    public void Unpause()
    {
        if (isPaused)
        {
            audioSource.volume = previousVolume;
            audioSource.UnPause();
            isPaused = false;
        }
    }
}
