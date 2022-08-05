using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [SerializeField] private Sound[] sounds;
    
    [SerializeField] private AudioMixer mainMixer;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.audioMixerGroup;
            s.source.loop = (s.audioMixerGroup.name == "Music");
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        mainMixer.SetFloat("musicVol", PlayerPrefs.GetFloat("musicVol",20));
        mainMixer.SetFloat("soundVol", PlayerPrefs.GetFloat("soundVol",20));
    }
    
    public void PlaySound(string soundName, bool looping = false, GameObject onGameObject = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        
        if (s == null)
        {
            Debug.LogWarning("Sound called " + soundName + " not found.");
            return;
        }
        if (s.source.isPlaying && s.audioMixerGroup.name == "Music")
        {
            // looping sounds should not be restarted
            return;
        }

        if (PlayerPrefs.GetInt(s.audioMixerGroup.name == "Music"? "music" : "sound", 1) == 1)
        {
            if (onGameObject != null)
            {
                AudioSource source = onGameObject.AddComponent<AudioSource>();
                source.clip = s.clip;
                source.outputAudioMixerGroup = s.audioMixerGroup;
                source.loop = looping; //(s.audioMixerGroup.name == "Music");

                source.Play();
            }
            else
            {
                s.source.Play();
            }
        }
    }


    public void StopSound(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        
        if (s == null)
        {
            Debug.LogWarning("Sound called " + soundName + " not found.");
            return;
        }
        
        s.source.Stop();
    }
    
    public void StopAllMusic()
    {
        Sound[] s = Array.FindAll(sounds, sound => sound.audioMixerGroup.name == "Music");
        
        if (s.Length == 0)
        {
            Debug.LogWarning("No sounds with AudioMixerGroup Music found.");
            return;
        }

        foreach (Sound sound in s)
        {
            sound.source.Stop();
        }
    }
    
    //TODO some functions for vibration

    public void SetMusicVolume(float vol)
    {
        mainMixer.SetFloat("musicVol", vol);
        PlayerPrefs.SetFloat("musicVol", vol);
    }
    
    public void SetSoundVolume(float vol)
    {
        mainMixer.SetFloat("soundVol", vol);
        PlayerPrefs.SetFloat("soundVol", vol);
    }
}


[System.Serializable]
public class Sound
{
    public string name;
    public AudioMixerGroup audioMixerGroup;
    
    public AudioClip clip;
    
    [HideInInspector]
    public AudioSource source;
}