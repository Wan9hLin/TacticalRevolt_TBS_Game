using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;  // 单例实例

    public Sound[] sounds;                 // 音效数组

    private void Awake()
    {
        // 单例模式：确保只有一个AudioManager存在
        if (Instance == null)
        {
            Instance = this;
          //  DontDestroyOnLoad(gameObject); // 在场景切换时不销毁
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化每个Sound的AudioSource
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixerGroup; // 设置 Mixer Group
        }
    }

    // 播放指定名称的音效
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    // 停止指定名称的音效
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    // 播放指定名称的音效并返回AudioSource
    public AudioSource PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        s.source.Play();
        return s.source;
    }

    // 通过事件播放音效（可选）
    // 您可以扩展此类，通过事件驱动的方式播放音效
}
