using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using Yarn.Unity;

public class AudioManager : MonoBehaviour
{
    public List<AudioData> audioDatas;

    [SerializeField] private AudioMixer AudioMixer;


    public Dictionary<string, GameObject> audioGameObjects = new Dictionary<string, GameObject>();

    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        foreach (AudioData audioData in audioDatas)
        {   
            GameObject audioGameObject = new GameObject(audioData.audioName);
            audioGameObject.transform.SetParent(transform);

            AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
            audioSource.clip = audioData.audioClip;
            audioSource.name = audioData.audioName;
            audioSource.loop = audioData.isLoop;
            audioSource.volume = audioData.volume;
            audioSource.pitch = audioData.pitch;
            audioSource.playOnAwake = audioData.playOnAwake;
            switch (audioData.type)
            {
                case AudioType.BGM:
                    audioSource.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("BGM")[0];
                    break;
                case AudioType.SFX:
                    audioSource.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("FSX")[0];
                    break;
                default:
                    break;
            }

            if (audioData.playOnStart)
            {
                audioSource.Play();
            }

            audioGameObjects.Add(audioData.audioName, audioGameObject);
        }
    }

    [YarnCommand("PlayAudio")]
    public void PlayAudio(string audioName)
    {
        if (audioGameObjects.ContainsKey(audioName))
        {
            AudioSource audioSource = audioGameObjects[audioName].GetComponent<AudioSource>();
            audioSource.Play();
        } else
        {
            Debug.LogError($"AudioManager: PlayAudio: audioName {audioName} not found");
        }
    }

    [YarnCommand("StopAudio")]
    public void StopAudio(string audioName)
    {
        if (audioGameObjects.ContainsKey(audioName))
        {
            AudioSource audioSource = audioGameObjects[audioName].GetComponent<AudioSource>();
            audioSource.Stop();
        } else
        {
            Debug.LogError($"AudioManager: StopAudio: audioName {audioName} not found");
        }
    }

    public void SetVolume(string volumeName, float volume)
    {
        Debug.Log($"SetVolume: {volumeName} volume: {volume}");
        AudioMixer.SetFloat(volumeName, Mathf.Log10(volume) * 20);
    }

    public float GetVolume(string volumeName)
    {
        AudioMixer.GetFloat(volumeName, out float volume);
        return Mathf.Pow(10, volume / 20);
    }
}

[System.Serializable]
public class AudioData
{
    public string audioName;
    public bool isLoop = false;
    public float volume = 1;
    public float pitch = 1;
    public bool playOnAwake = false;
    public AudioType type = AudioType.SFX;
    public bool playOnStart = false;

    public AudioClip audioClip;
}

public enum AudioType
{
    BGM,
    SFX
}
