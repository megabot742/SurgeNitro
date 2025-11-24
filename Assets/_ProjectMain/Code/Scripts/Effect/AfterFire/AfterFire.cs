using UnityEngine;

[DisallowMultipleComponent]
public class AfterFire : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _minDuration = 0.05f;
    [SerializeField, Min(0f)] private float _maxDuration = 0.1f;

    [SerializeField, ReadOnly] private ParticleSystem particleEffect;
    [SerializeField, ReadOnly] private CarControllerBase carController;

    private float endTime;

    private void Awake()
    {
        particleEffect = GetComponent<ParticleSystem>();
        particleEffect.Stop();

        //Find CarControllerBase
        carController = GetComponentInParent<CarControllerBase>();
        if (carController != null)
        {
            //Check style controller
            if (carController is RealisticCarController realistic)
            {
                realistic.Engine.OnAfterFire.AddListener(OnAfterFire);
            }
            else if (carController is ArcadeCarController arcade)
            {
                arcade.OnAfterFire.AddListener(OnAfterFire);
            }
        }
    }

    private void Update()
    {
        if (carController == null)
        {
            return;
        }

        //Check NOS
        bool isNitroActive = false;
        if (carController is RealisticCarController realistic)
        {
            isNitroActive = realistic.Engine.NOS.Injection;
        }
        else if (carController is ArcadeCarController arcade)
        {
            isNitroActive = arcade.NOS.Injection;
        }

        if (isNitroActive)
        {
            if (!particleEffect.isPlaying)
            {
                particleEffect.Play();
            }
        }
        else
        {
            if (particleEffect.isPlaying)
            {
                if (Time.time >= endTime)
                {
                    particleEffect.Stop();
                }
            }
        }
    }

    private void OnAfterFire()
    {
        if (particleEffect.isPlaying)
        {
            return;
        }

        particleEffect.Play();

        endTime = Time.time + Random.Range(_minDuration, _maxDuration);
    }
    void OnDestroy()
    {
        // Hủy listener để tránh memory leak
        if (carController != null)
        {
            if (carController is RealisticCarController realistic)
            {
                realistic.Engine.OnAfterFire.RemoveListener(OnAfterFire);
            }
            else if (carController is ArcadeCarController arcade)
            {
                arcade.OnAfterFire.RemoveListener(OnAfterFire);
            }
        }
    }
}
