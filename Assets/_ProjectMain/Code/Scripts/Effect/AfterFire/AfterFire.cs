using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[DisallowMultipleComponent]
public class AfterFire : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _minDuration = 0.05f;
    [SerializeField, Min(0f)] private float _maxDuration = 0.1f;

    private ParticleSystem _particle;

    private CarControllerBase _carController;

    private float _endTime;

    private void Awake()
    {
        _particle = GetComponent<ParticleSystem>();
        _particle.Stop();

        // Tìm CarControllerBase thay vì EngineCarController
        _carController = GetComponentInParent<CarControllerBase>();
        if (_carController != null)
        {
            // Kiểm tra loại controller và gắn listener phù hợp
            if (_carController is RealisticCarController realistic)
            {
                realistic.Engine.OnAfterFire.AddListener(OnAfterFire);
            }
            else if (_carController is ArcadeCarController arcade)
            {
                arcade.OnAfterFire.AddListener(OnAfterFire);
            }
        }
    }

    private void Update()
    {
        if (_carController == null)
        {
            return;
        }

        // Kiểm tra NOS.Injection cho cả hai controller
        bool isNitroActive = false;
        if (_carController is RealisticCarController realistic)
        {
            isNitroActive = realistic.Engine.NOS.Injection;
        }
        else if (_carController is ArcadeCarController arcade)
        {
            isNitroActive = arcade.NOS.Injection;
        }

        if (isNitroActive)
        {
            if (!_particle.isPlaying)
            {
                _particle.Play();
            }
        }
        else
        {
            if (_particle.isPlaying)
            {
                if (Time.time >= _endTime)
                {
                    _particle.Stop();
                }
            }
        }
    }

    private void OnAfterFire()
    {
        if (_particle.isPlaying)
        {
            return;
        }

        _particle.Play();

        _endTime = Time.time + Random.Range(_minDuration, _maxDuration);
    }
    void OnDestroy()
    {
        // Hủy listener để tránh memory leak
        if (_carController != null)
        {
            if (_carController is RealisticCarController realistic)
            {
                realistic.Engine.OnAfterFire.RemoveListener(OnAfterFire);
            }
            else if (_carController is ArcadeCarController arcade)
            {
                arcade.OnAfterFire.RemoveListener(OnAfterFire);
            }
        }
    }
}
