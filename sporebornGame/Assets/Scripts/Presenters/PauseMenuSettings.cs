using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuSettings : MonoBehaviour
{

    public AudioMixer mainMixer;
    public Slider musicSlider;

    private void OnEnable()
    {
        float musicVolumeDb;

         if (mainMixer.GetFloat("MusicVolume", out musicVolumeDb))
        {   
            float linearVolume = Mathf.Pow(10, musicVolumeDb / 20);
            musicSlider.value = linearVolume;
        }
         
    }

    public void SetVolumeMain(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);

    }


    public void SetVolumeSFX(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);

    }


    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
