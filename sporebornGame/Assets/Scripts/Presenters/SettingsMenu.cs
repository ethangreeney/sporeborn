using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{

    public AudioMixer aM;
    public TMP_Dropdown qualityDropdown;

    void Start()
    {
        if (qualityDropdown != null)
        {
            int currentQualityIndex = QualitySettings.GetQualityLevel();

            qualityDropdown.value = currentQualityIndex;

            qualityDropdown.RefreshShownValue();
        }
    }
    public void SetVolume(float volume)
    {
        aM.SetFloat("MainVolume", volume);

    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
