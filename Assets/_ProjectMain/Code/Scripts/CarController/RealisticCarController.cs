using UnityEngine;

public class RealisticCarController : CarControllerBase
{
    [SerializeField] private Engine _engine;
    [SerializeField] private Transmission _transmission;

    private bool _clutchInput;
    private bool _nosInput;

    public bool ClutchInput
    {
        get => _clutchInput;
        set
        {
            if (!replayPlaying)
            {
                _clutchInput = value;
            }
        }
    }

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

    public Engine Engine => _engine;
    public Transmission Transmission => _transmission;

    public override float MaxSpeedKPH
    {
        get
        {
            var maxSpeed = 0f;
            for (var i = 0; i < _transmission.GearRatios.Length; i++)
            {
                var maxSpeedInCurrGear = CarMath.EngineRPMToSpeedKPH(
                    _engine.RevLimitRPM,
                    _transmission.GearRatios[i],
                    _transmission.FinalGearRatio,
                    wheelRadius);
                maxSpeed = Mathf.Max(maxSpeedInCurrGear, maxSpeed);
            }
            return maxSpeed;
        }
    }

    public override bool Reverse
    {
        get => _transmission.Gear == Transmission.ReverseGear;
        set
        {
            if (!replayPlaying)
            {
                var gear = value ? Transmission.ReverseGear : 1;
                _transmission.ShiftGear(gear);
            }
        }
    }


    public override float MotorRevolutionRate => _engine.RevolutionRate;

    protected override void Awake()
    {
        base.Awake();

        _engine.Init();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (replayPlaying)
        {
            return;
        }

        AddDriveTorque();
    }

    private void AddDriveTorque()
    {
        _transmission.Update(SpeedKPH, wheelRadius, base.throttleInput);

        var clutchInput = _clutchInput;
        var throttleInput = base.throttleInput;
        if (_transmission.IsShiftingGear)
        {
            clutchInput = true;
            throttleInput = 0f;
        }

        var inputShaftRPM = CalcInputShaftRPMFromSpeed();
        var grounded = IsGrounded();
        var totalBrakeTorque = MaxBrakeTorque * brakeInput * Wheels.Length;

        _engine.Update(
            inputShaftRPM,
            throttleInput,
            clutchInput,
            _nosInput,
            !grounded,
            _transmission.CurrentTotalGearRatio,
            totalBrakeTorque);

        if (grounded)
        {
            var engineTorque = _engine.OutputTorque;
            var engineFriTorque = _engine.OutputFrictionTorque;
            var totalGearRatio = _transmission.CurrentTotalGearRatio;

            var driveTorque = engineTorque * totalGearRatio;
            var friTorque = engineFriTorque * totalGearRatio;

            AddDriveTorque(driveTorque);
            AddBrakeTorque(friTorque);
        }
    }

    public float CalcInputShaftRPMFromSpeed()
    {
        return CarMath.SpeedKPHToEngineRPM(
            ForwardSpeedKPH,
            _transmission.GearRatios[_transmission.Gear],
            _transmission.FinalGearRatio,
            wheelRadius);
    }
}
