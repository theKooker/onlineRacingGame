using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVol", 0);
        soundSlider.value = PlayerPrefs.GetFloat("soundVol", 0);
    }
    
    public void SetMusicVolume(float vol)
    {
        AudioManager.instance.SetMusicVolume(vol);
    }
    
    public void SetSoundVolume(float vol)
    {
        AudioManager.instance.SetSoundVolume(vol);
    }
}
