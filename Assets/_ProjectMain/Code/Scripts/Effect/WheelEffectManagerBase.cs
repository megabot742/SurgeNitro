using UnityEngine;
[RequireComponent(typeof(RoadMaterialDetector), typeof(WheelEffectPool))]
public class WheelEffectManagerBase<T> : MonoBehaviour where T : WheelEffectBase
{
    private RoadMaterialDetector _detector;
    private WheelEffectPool _pool;
    private RoadMaterial _currentMaterial;

    private void Awake()
    {
        _detector = GetComponent<RoadMaterialDetector>();
        _pool = GetComponent<WheelEffectPool>();
        _pool.InitializePools(); // Initialize pools on Awake
    }

    private void OnEnable()
    {
        _detector.OnMaterialChanged.AddListener(UpdateEffect); // Subscribe to material change event
    }

    private void OnDisable()
    {
        _detector.OnMaterialChanged.RemoveListener(UpdateEffect); // Unsubscribe
    }

    public void UpdateEffect(RoadMaterial newMat)
    {
        if (newMat == _currentMaterial)
        {
            return;
        }

        // Suppress current effect
        if (_currentMaterial != null)
        {
            var currentEffect = _pool.GetActiveEffect<T>(_currentMaterial);
            if (currentEffect != null)
            {
                currentEffect.Suppression = true;
                _pool.ReleaseEffect<T>(_currentMaterial);
            }
        }

        _currentMaterial = newMat;

        if (_currentMaterial == null)
        {
            return;
        }

        // Get new effect from pool
        var effect = _pool.GetEffect<T>(_currentMaterial);
        if (effect != null)
        {
            effect.Suppression = false;
        }
    }
}

