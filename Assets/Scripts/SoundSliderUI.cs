using UnityEngine;
using UnityEngine.UI;

public class SoundSliderUI : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    private void OnEnable()
    {
        masterSlider.value = SoundManager.I.masterVolume;
        musicSlider.value = SoundManager.I.musicVolume;
        sfxSlider.value = SoundManager.I.sfxVolume;

        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.I.masterVolume = value;
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.I.musicVolume = value;
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.I.sfxVolume = value;
    }
}
