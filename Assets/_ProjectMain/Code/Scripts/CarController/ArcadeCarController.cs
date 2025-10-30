using UnityEngine;
using UnityEngine.Events;

public class ArcadeCarController : CarControllerBase
{
    [SerializeField, Min(0f)] private float _maxForwardSpeedKPH = 120f;
    [SerializeField, Min(0f)] private float _maxBackwardSpeedKPH = 40f;
    [SerializeField, Min(0f)] private float _maxMotorTorque = 300f;
    [SerializeField, Min(0f)] private float _minMotorFrictionTorque = 15f;
    [SerializeField, Min(0f)] private float _maxMotorFrictionTorque = 75f;
    [SerializeField, Min(0.001f)] private float _motorInertia = 0.3f;
    [SerializeField, Min(0f)] private float _finalGearRatio = 8f;

    [Header("Boots System")]
    [SerializeField] Turbocharger _turbocharger;
    [SerializeField] Nitro _nos;
    public Turbocharger Turbocharger => _turbocharger;
    public Nitro NOS => _nos;

    [Header("AfterFire")]
    [SerializeField, Min(0f)] private float _mixtureUnbalanceTime = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _afterFireProbability = 0.25f;
    [SerializeField, Min(0f)] private float _minAfterFireRPM = 3000f;
    private float _prevThrottleInput;
    private float _remainMixtureUnbalanceTime;
    private UnityEvent _onAfterFire = new UnityEvent();
    public UnityEvent OnAfterFire => _onAfterFire;
    
    

    private float _maxMotorForwardRPM;
    private float _maxMotorBackwardRPM;

    private float _motorRPM;

    private bool _reverse;

    public override bool Reverse
    {
        get => _reverse;
        set
        {
            if (!replayPlaying)
            {
                _reverse = value;
            }
        }
    }
    //Nitro
    private bool _nosInput;

    public bool NOSInput
    {
        get => _nosInput;
        set
        {
            if (!replayPlaying)
            {
                _nosInput = value;
            }
        }
    }
    
    
    public override float MotorRevolutionRate
    {
        get => _motorRPM / Mathf.Max(_maxMotorForwardRPM, _maxMotorBackwardRPM);
    }

    public float MotorRPM
    {
        get => _motorRPM;
    }

    private bool IsExceedMaxMotorRPM
    {
        get
        {
            var maxRPM = _reverse ? _maxMotorBackwardRPM : _maxMotorForwardRPM;
            var rpm = Mathf.Abs(_motorRPM);
            return rpm > maxRPM;
        }
    }

    public override float MaxSpeedKPH => Mathf.Max(_maxForwardSpeedKPH, _maxBackwardSpeedKPH);

    // public override CarReplayData CarData
    // {
    //     get
    //     {
    //         var replayData = new SimpleCarReplayData();

    //         SetReplayData(replayData);

    //         replayData.MotorRPM = _motorRPM;
    //         replayData.Reverse = _reverse;

    //         return replayData;
    //     }

    //     set
    //     {
    //         var replayData = value as SimpleCarReplayData;
    //         Debug.Assert(replayData != null, "リプレイデータの種類が違う");

    //         if (!_replayPlaying)
    //         {
    //             StartReplay();
    //         }

    //         RestoreFromReplayData(replayData);

    //         _motorRPM = replayData.MotorRPM;
    //         _reverse = replayData.Reverse;
    //     }
    // }

    protected override void Awake()
    {
        base.Awake();

        _maxMotorForwardRPM = CalcMotorRPMFromSpeedKPH(_maxForwardSpeedKPH);
        _maxMotorBackwardRPM = CalcMotorRPMFromSpeedKPH(_maxBackwardSpeedKPH);

        _nos.Init();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        AddDriveTorque();
    }

    private void AddDriveTorque()
    {
        var throttleInput = ThrottleInput;
        if (IsExceedMaxMotorRPM)
        {
            throttleInput = 0f;
        }
        UpdateMixture(throttleInput);
        // NEW: Cập nhật Turbo và Nitro trước khi tính torque (dùng _motorRPM thay vì engineRPM)
        _turbocharger.Update(_motorRPM, throttleInput);
        _nos.Update(_nosInput);
        if (IsGrounded())
        {
            _motorRPM = CalcMotorRPMFromSpeedKPH(SpeedKPH);
            var motorTorque = GetMotorTorque(_motorRPM) * throttleInput;
            var motorFriTorque = GetMotorFrictionTorque(_motorRPM) * (1f - throttleInput);

            // NEW: Nhân hệ số từ Turbo và Nitro vào motorTorque
            motorTorque *= _turbocharger.EngineTorqueCoef * _nos.EngineTorqueCoef;

            var driveTorque = motorTorque * _finalGearRatio;
            var friTorque = motorFriTorque * _finalGearRatio;

            AddDriveTorque(driveTorque);
            AddBrakeTorque(friTorque);
        }
        else
        {
            var motorTorque = GetMotorFrictionTorque(_motorRPM) * throttleInput;
            var motorFiTorque = GetMotorFrictionTorque(_motorRPM) * (1f - throttleInput);

            // NEW: Nhân hệ số từ Turbo và Nitro vào motorTorque (khi inAir, turbo/nitro vẫn ảnh hưởng acceleration)
            motorTorque *= _turbocharger.EngineTorqueCoef * _nos.EngineTorqueCoef;

            var totalBrakeTorque = MaxBrakeTorque * BrakeInput * Wheels.Length;

            var driveTorque = motorTorque * _finalGearRatio;
            var drivetrainI = _finalGearRatio * _finalGearRatio * _motorInertia;

            var friTorque = motorFiTorque * _finalGearRatio;

            var brakeTorque = totalBrakeTorque * _finalGearRatio;

            _motorRPM += (driveTorque / drivetrainI) * Time.fixedDeltaTime * CarMath.RPSToRPM;
            DecelerateMotor(friTorque, drivetrainI);
            DecelerateMotor(brakeTorque, drivetrainI);
        }
    }
    // NEW: Copy logic UpdateMixture từ Engine.cs
    private void UpdateMixture(float throttleInput)
    {
        var diff = Mathf.Clamp01(_prevThrottleInput - throttleInput);
        _prevThrottleInput = throttleInput;

        var newRemainMixtureUnbalanceTime = _mixtureUnbalanceTime * diff;
        _remainMixtureUnbalanceTime = Mathf.Max(newRemainMixtureUnbalanceTime, _remainMixtureUnbalanceTime);

        _remainMixtureUnbalanceTime = Mathf.Max(_remainMixtureUnbalanceTime - Time.deltaTime, 0f);

        if ((_motorRPM >= _minAfterFireRPM) && (_remainMixtureUnbalanceTime > 0f))
        {
            var normRemainTime = _remainMixtureUnbalanceTime / _mixtureUnbalanceTime;
            var prob = _afterFireProbability * normRemainTime;

            if (Random.value <= prob)
            {
                OnAfterFire.Invoke();
            }
        }
    }

    public float CalcMotorRPMFromSpeedKPH(float speedKPH)
    {
        return CarMath.SpeedKPHToEngineRPM(speedKPH, 1f, _finalGearRatio, wheelRadius);
    }

    private float GetMotorTorque(float motorRPM)
    {
        if (IsExceedMaxMotorRPM)
        {
            return 0f;
        }

        var revRate = Mathf.Clamp01(MotorRevolutionRate);

        var coef = 1f;
        if (revRate >= 0.5f)
        {
            coef = (1f - revRate) * 2f;
            coef *= coef;
        }

        var sign = _reverse ? -1f : 1f;

        return sign * _maxMotorTorque * coef;
    }

    private float GetMotorFrictionTorque(float motorRPM)
    {
        return Mathf.Lerp(_minMotorFrictionTorque, _maxMotorFrictionTorque, motorRPM * motorRPM);
    }

    private void DecelerateMotor(float torque, float inertia)
    {
        var acc = -Mathf.Sign(_motorRPM) * (torque / inertia) * Time.fixedDeltaTime * CarMath.RPSToRPM;
        if (Mathf.Abs(acc) > _motorRPM)
        {
            _motorRPM = 0f;
        }
        else
        {
            _motorRPM += acc;
        }
    }
}
