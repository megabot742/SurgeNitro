using UnityEngine;

public enum AfterFireType { Particle, Audio } // Enum cho phân loại, dễ extend (add Light, etc.)

[DisallowMultipleComponent]
public class AfterFireEffect : MonoBehaviour
{
    [SerializeField] private AfterFireType type;
    [SerializeField, ReadOnly] private ParticleSystem particle; // Chỉ show nếu type = Particle
    [SerializeField, ReadOnly] private AudioSource sound; // Chỉ show nếu type = Audio

    public void Initialize()
    {
        // Validate dựa trên enum (quản lý lỗi)
        if (type == AfterFireType.Particle && particle == null)
            particle = GetComponent<ParticleSystem>();
        else if (type == AfterFireType.Audio && sound == null)
            sound = GetComponent<AudioSource>();

        if (type == AfterFireType.Audio && sound != null)
        {
            sound.playOnAwake = false;
            sound.loop = true; // Như code gốc
        }
    }

    public void PlayEffect()
    {
        switch (type)
        {
            case AfterFireType.Particle:
                if (particle != null && !particle.isPlaying) particle.Play();
                break;
            case AfterFireType.Audio:
                if (sound != null && !sound.isPlaying) sound.Play();
                break;
            // Add case mới cho nâng cấp, ví dụ: case Light: light.enabled = true;
        }
    }

    public void StopEffect()
    {
        switch (type)
        {
            case AfterFireType.Particle:
                if (particle != null && particle.isPlaying) particle.Stop();
                break;
            case AfterFireType.Audio:
                if (sound != null && sound.isPlaying) sound.Stop();
                break;
        }
    }
}
