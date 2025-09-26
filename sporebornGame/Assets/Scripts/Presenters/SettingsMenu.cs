using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{

    public AudioMixer aM;
    public TMP_Dropdown qualityDropdown;

    Resolution[] resolutions;

    public TMP_Dropdown resolutionDropdown;

    void Start()
    {
        if (qualityDropdown != null)
        {
            int currentQualityIndex = QualitySettings.GetQualityLevel();

            qualityDropdown.value = currentQualityIndex;

            qualityDropdown.RefreshShownValue();
        }

        resolutions = Screen.resolutions
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First())
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ToArray();

        resolutionDropdown.ClearOptions();

        List<string> resolutionStrings = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionStrings.Add(option);

            if ((resolutions[i].width == Screen.currentResolution.width) && (resolutions[i].height == Screen.currentResolution.height))
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionStrings);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void SetVolume(float volume)
    {
        aM.SetFloat("MainVolume", volume);

    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
