using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class PauseMenuSettings : MonoBehaviour
{

    public AudioMixer aMain;
    public AudioMixer aSFX;

    public void SetVolumeMain(float volume)
    {
        aMain.SetFloat("MainVolume", volume);

    }


    public void SetVolumeSFX(float volume)
    {
        aSFX.SetFloat("MainVolume", volume);

    }


    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
