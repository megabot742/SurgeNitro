using UnityEngine;

public class WheelEffectBase : MonoBehaviour
{
    [SerializeField, Range(0f, 90f)] private float _minSlipAngle = 5f;
    [SerializeField, Range(0f, 90f)] private float _maxSlipAngle = 20f;
    [SerializeField, Min(0f)] private float _minSpeed = 9999f;
    [SerializeField, Min(0f)] private float _maxSpeed = 9999f;

    private CarControllerBase _car;
    private Wheel _wheel;

    private bool _suppression;

    public bool Suppression
    {
        get => _suppression;
        set => _suppression = value;
    }

    protected Wheel Wheel => _wheel;

    protected float SlipAmount
    {
        get
        {
            if (_car == null || _wheel == null)
            {
                return 0f;
            }

            if (_suppression)
            {
                return 0f;
            }

            if (!_wheel.Grounded)
            {
                return 0f;
            }

            var s1 = Mathf.InverseLerp(_minSlipAngle, _maxSlipAngle, Mathf.Abs(_car.SlipAngle));
            var s2 = Mathf.InverseLerp(_minSpeed, _maxSpeed, _car.Speed);

            var s3 = 0f;
            if (_car.HandbrakeInput
                && _car.Speed > 1f
                && (_wheel == _car.Wheels[^2] || _wheel == _car.Wheels[^1]))
            {
                s3 = 1f;
            }

            return Mathf.Max(s1, s2, s3);
        }
    }

    protected virtual void Awake()
    {
        _car = GetComponentInParent<CarControllerBase>();
        _wheel = GetComponentInParent<Wheel>();
    }

    protected virtual void Update()
    {
        MoveToContactPosition();
    }

    private void MoveToContactPosition()
    {
        if (_wheel == null || !_wheel.Grounded)
        {
            return;
        }

        transform.position = _wheel.HitInfo.point;
    }
}
