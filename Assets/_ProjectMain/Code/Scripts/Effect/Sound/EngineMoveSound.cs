using UnityEngine;
[DisallowMultipleComponent]
public class EngineMoveSound : BaseSound
{
    [Header("FadeSetting")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private EngineIdleSound idleSound;
    private Coroutine fadeRoutine;
    protected override void Update()
    {
        if (audioSource == null || car == null) return;

        bool isMoving = car.MotorRevolutionRate > 0.01f;
        Debug.Log(car.MotorRevolutionRate);
        if (isMoving)
        {
            StopFade();
            float targetVol = Mathf.Lerp(throttleOffVolume, maxVolume, car.ThrottleInput);
            fadeRoutine = StartCoroutine(AudioFade.FadeIn(audioSource, fadeDuration, targetVol));
            if (idleSound != null)
                idleSound.FadeOutSound(fadeDuration);
            ApplySound(targetVol);
        }
        else
        {
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
