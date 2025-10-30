using UnityEngine;
[DisallowMultipleComponent]
public class EngineSound : MonoBehaviour
{
    [SerializeField] private float _minPitch = 0.5f;
    [SerializeField] private float _maxPitch = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _maxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _throttleOffVolume = 0.5f;
    [SerializeField, Min(0f)] private float _damping = 20f;

    private AudioSource engineAudioSource;
    private CarControllerBase car;
    private void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        car = GetComponentInParent<CarControllerBase>();
    }
    private void Update()
    {
        ControlEngineSound();
    }
    void ControlEngineSound()
    {
        if (engineAudioSource == null || car == null)
        {
            return;
        }

        var revRate = car.MotorRevolutionRate;
        //Debug.Log(car.MotorRevolutionRate);
        var pitch = Mathf.Lerp(_minPitch, _maxPitch, revRate);

        var volume = Mathf.Lerp(_throttleOffVolume, 1f, car.ThrottleInput) * _maxVolume;

        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, pitch, _damping * Time.deltaTime);
        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, volume, _damping * Time.deltaTime);    
    }
}
