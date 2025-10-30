using UnityEngine;
using UnityEngine.Audio;
[DisallowMultipleComponent]
public class SkidSound : WheelEffectBase
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioMixerGroup audioMixerGroup;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 0.5f;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        AddAudioSource();
    }

    protected override void Update()
    {
        base.Update();

        if (audioSource == null || Wheel == null)
        {
            return;
        }

        var volume = SlipAmount * maxVolume;

        if (volume > 0f)
        {
            audioSource.volume = volume;
        }
        else
        {
            audioSource.volume = 0f;
        }
    }

    private void AddAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0.5f;
        audioSource.Play();
    }
}
