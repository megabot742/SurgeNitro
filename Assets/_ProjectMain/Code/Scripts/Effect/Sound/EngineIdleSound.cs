using System.Collections;
using UnityEngine;
[DisallowMultipleComponent]
public class EngineIdleSound : BaseSound
{
    [Header("FadeSetting")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private EngineMoveSound moveSound; // Chỉ để gọi FadeOut
    private Coroutine fadeRoutine;

    protected override void Update()
    {
        if (audioSource == null || car == null) return;

        bool isIdle = car.MotorRevolutionRate <= 0.01f;

        if (isIdle)
        {
            // Bật idle
            StopFade();
            fadeRoutine = StartCoroutine(AudioFade.FadeIn(audioSource, fadeDuration, maxVolume));
            if (moveSound != null)
                moveSound.FadeOutSound(fadeDuration); // Gọi method công khai
            ApplySound(maxVolume);
        }
        else
        {
            // Tắt idle
            StopFade();
            fadeRoutine = StartCoroutine(AudioFade.FadeOut(audioSource, fadeDuration));
        }
    }

    private void StopFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }
    //Cross-Fade
    public void FadeOutSound(float duration)
    {
        StopFade();
        fadeRoutine = StartCoroutine(AudioFade.FadeOut(audioSource, duration));
    }
}
