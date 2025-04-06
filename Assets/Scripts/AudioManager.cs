
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public VisualTreeAsset menuDoc;
    public VisualTreeAsset gamneDoc;

    [SerializeField]
    private List<AudioClip> musicSounds, sfxSounds;


    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetupAudio()
    {
        // Sound Effects
        if (MainMenuEvents.Instance != null)
        {
            MainMenuEvents.Instance.sfxSlider.value = 100 * sfxSource.volume;
            Debug.Log(sfxSource.volume);
            MainMenuEvents.Instance.sfxSlider.RegisterValueChangedCallback(evt => OnSfxSliderChanged(evt.newValue));
        }
    }


    // SFX -----------------------------------
    public void PlaySound(string soundName)
    {
        sfxSource.clip = sfxSounds.Find(s => s.name == soundName); 
        sfxSource.Play(); 
    }


    private void OnSfxSliderChanged(float value)
    {
        sfxSource.volume = value / 100f; 
        Debug.Log($"SFX Volume Changed: {sfxSource.volume}");
    }

    // Songs ---------------------------------
    public void PlayMusic(string songName, bool loop)
    {
        musicSource.clip = musicSounds.Find(s => s.name == songName);
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() { musicSource.Stop(); }


    private void OnMusicSliderChanged(float value)
    {
        sfxSource.volume = value / 100f;
        Debug.Log($"Music Volume Changed: {sfxSource.volume}");
    }

}
