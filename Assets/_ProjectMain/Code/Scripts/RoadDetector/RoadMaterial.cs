using UnityEngine;
[System.Serializable]
public class RoadMaterial
{
    [SerializeField] private string _name = "Asphalt";

    [SerializeField, Min(0f)] private float _peakFrictionSlipAngle = 5f;
    [SerializeField, Min(0f)] private float _mu = 2f;
    [SerializeField, Min(0f)] private float _rollingResistanceCoef = 0.015f;

    [SerializeField, Min(0f)] private float _bumpPeriod = 0f;
    [SerializeField, Min(0f)] private float _bumpAmplitude = 0f;

    [SerializeField] private WheelSmoke _wheelSmokePrefab;
    [SerializeField] private Skidmark _skidmarkPrefab;
    [SerializeField] private SkidSound _skidSoundPrefab;

    public string Name => _name;

    public float PeakFrictionSlipAngle => _peakFrictionSlipAngle;

    public float Mu => _mu;

    public float RollingResistanceCoef => _rollingResistanceCoef;

    public float BumpPeriod => _bumpPeriod;

    public float BumpAmplitude => _bumpAmplitude;

    public T GetEffectPrefab<T>() where T : WheelEffectBase
    {
        if (typeof(T) == typeof(WheelSmoke))
        {
            return _wheelSmokePrefab as T;
        }
        if (typeof(T) == typeof(Skidmark))
        {
            return _skidmarkPrefab as T;
        }
        if (typeof(T) == typeof(SkidSound))
        {
            return _skidSoundPrefab as T;
        }
        return null;
    }
}
