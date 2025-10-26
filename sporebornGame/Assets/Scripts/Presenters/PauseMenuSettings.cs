using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuSettings : MonoBehaviour
{

    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        float musicVolumeDb;

        if (mainMixer.GetFloat("MusicVolume", out musicVolumeDb))
        {   
            float linearVolume = Mathf.Pow(10, musicVolumeDb / 20);
            musicSlider.value = linearVolume;
        }

        float sfxVolumeDb;
        if (mainMixer.GetFloat("SFXVolume", out sfxVolumeDb) && sfxSlider != null)
        {
            float linearSfx = Mathf.Pow(10f, sfxVolumeDb / 20f);
            sfxSlider.value = linearSfx;
        }
    }

    public void SetVolumeMain(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetVolumeSFX(float volume)
    {
        float db = (volume <= 0f) ? -80f : Mathf.Log10(volume) * 20f;
        mainMixer.SetFloat("SFXVolume", db);
    }

    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}