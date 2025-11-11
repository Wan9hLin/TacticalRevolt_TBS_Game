
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;                // 音效名称，用于调用
    public AudioClip clip;             // 音频剪辑

    [Range(0f, 1f)]
    public float volume = 0.7f;        // 音量

    [Range(0.1f, 3f)]
    public float pitch = 1f;           // 音调

    public bool loop = false;          // 是否循环播放

    public AudioMixerGroup mixerGroup; // 新增：所属 Mixer Group

    [HideInInspector]
    public AudioSource source;         // 音频源，将在运行时赋值
}
