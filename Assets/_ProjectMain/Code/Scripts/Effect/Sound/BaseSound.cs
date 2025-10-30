using UnityEngine;

public class BaseSound : MonoBehaviour
{
    [SerializeField] protected float minPitch = 0.5f;
    [SerializeField] protected float maxPitch = 1.5f;
    [SerializeField, Range(0f, 1f)] protected float maxVolume = 1f;
    [SerializeField, Range(0f, 1f)] protected float throttleOffVolume = 0.5f;
    [SerializeField, Min(0f)] protected float damping = 20f;

    protected AudioSource audioSource;
    protected CarControllerBase car;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        car = GetComponentInParent<CarControllerBase>();

        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;
            if (!audioSource.isPlaying) audioSource.Play();
        }
        if (car == null)
        {
            Debug.LogWarning("CarControllerBase not found in parent hierarchy of " + gameObject.name);
        }
    }

    //Apply Sound For Move and Idle
    protected void ApplySound(float targetVolume)
    {
        if (audioSource == null || car == null) return;

        float revRate = car.MotorRevolutionRate;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, revRate);

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, damping * Time.deltaTime);
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, damping * Time.deltaTime);
    }

    //Override subclass to decide when to run
    protected virtual void Update() { }
}
