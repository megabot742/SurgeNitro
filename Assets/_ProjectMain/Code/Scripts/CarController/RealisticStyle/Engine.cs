using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class Engine
{
    [SerializeField, Min(0f)] private float _idleRPM = 1000f;
    [SerializeField, Min(0f)] private float _idleTorque = 225f;

    [SerializeField, Min(0f)] private float _peakRPM = 4500f;
    [SerializeField, Min(0f)] private float _peakTorque = 275f;

    [SerializeField, Min(0f)] private float _revLimitRPM = 6500f;
    [SerializeField, Min(0f)] private float _revLimitTorque = 225f;

    [SerializeField, Min(0f)] private float _frictionFormulaIntercept = 25f;
    [SerializeField, Min(0f)] private float _frictionFormulaCoef1 = 0.0001f;
    [SerializeField, Min(0f)] private float _frictionFormulaCoef2 = 0.1f;

    [SerializeField, Min(0f)] private float _inertia = 0.3f;

    [SerializeField, Min(0f)] private float _fuelCutTime = 0.05f;

    [SerializeField, Min(0f)] private float _mixtureUnbalanceTime = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _afterFireProbability = 0.25f;
    [SerializeField, Min(0f)] private float _minAfterFireRPM = 3000f;

    [SerializeField] private Turbocharger _turbocharger;
    [SerializeField] private Nitro _nos;

    private float _rpm;

    private float _outputTorque;
    private float _outputFrictionTorque;
    private float _outputInertia;

    private float _remainFuelCutTime;

    private float _prevThrottleInput;
    private float _remainMixtureUnbalanceTime;

    private UnityEvent _onAfterFire = new UnityEvent();

    public float IdleRPM => _idleRPM;

    public float RevLimitRPM => _revLimitRPM;

    public float RPM
    {
        get => _rpm;
        set => _rpm = value;
    }

    public float RevolutionRate => _rpm / RevLimitRPM;

    public Turbocharger Turbocharger => _turbocharger;

    public Nitro NOS => _nos;

    public float OutputTorque => _outputTorque;

    public float OutputFrictionTorque => _outputFrictionTorque;

    public float OutputInertia => _outputInertia;

    public UnityEvent OnAfterFire => _onAfterFire;

    public void Init()
    {
        _nos.Init();
    }

    public void Update(
        float inputShaftRPM,
        float throttleInput,
        bool clutchInput,
        bool nosInput,
        bool inAir,
        float totalGearRatio,
        float totaBrakeTorque)
    {
        _turbocharger.Update(_rpm, throttleInput);

        _nos.Update(nosInput);

        FuelCut(ref throttleInput);

        UpdateMixture(throttleInput);

        var clutchEngagement = clutchInput ? 0f : 1f;

        var rpmDisengaged = UpdateRPMClutchDisengaged(throttleInput);
        var rpmEngaged = inputShaftRPM;

        if (inAir)
        {
            rpmEngaged = UpdateRPMClutchEngaged(throttleInput, totalGearRatio, totaBrakeTorque);
        }

        _rpm = Mathf.Lerp(rpmDisengaged, rpmEngaged, clutchEngagement);
        _rpm = ClampRPM(_rpm);

        var torque = GetTorque(_rpm, throttleInput);

        _outputTorque = torque * _turbocharger.EngineTorqueCoef * _nos.EngineTorqueCoef * clutchEngagement;

        var friTorque = GetFrictionTorque(_rpm, throttleInput);

        _outputFrictionTorque = friTorque * clutchEngagement;

        _outputInertia = _inertia * clutchEngagement;
    }

    private float GetTorque(float rpm, float throttleInput)
    {
        if (rpm <= _peakRPM)
        {
            var t = Mathf.InverseLerp(_idleRPM, _peakRPM, rpm);
            return Mathf.Lerp(_idleTorque, _peakTorque, t * t) * throttleInput;
        }
        if (rpm <= _revLimitRPM)
        {
            var t = Mathf.InverseLerp(_peakRPM, _revLimitRPM, rpm);
            return Mathf.Lerp(_peakTorque, _revLimitTorque, t * t) * throttleInput;
        }
        return 0f;
    }

    private float GetFrictionTorque(float rpm, float throttleInput)
    {
        var angVel = rpm * CarMath.RPMToRPS;
        var torque = angVel * angVel * _frictionFormulaCoef1 + angVel * _frictionFormulaCoef2 + _frictionFormulaIntercept;
        torque *= (1f - throttleInput);
        return torque;
    }

    private float UpdateRPMClutchDisengaged(float throttleInput)
    {
        var torque = GetTorque(_rpm, throttleInput);
        var friTorque = GetFrictionTorque(_rpm, throttleInput);

        var newRPM = _rpm;
        newRPM += (torque / _inertia) * Time.deltaTime * CarMath.RPSToRPM;
        newRPM -= (friTorque / _inertia) * Time.deltaTime * CarMath.RPSToRPM;
        newRPM = ClampRPM(newRPM);

        return newRPM;
    }

    private float UpdateRPMClutchEngaged(float throttleInput, float totalGearRatio, float totalBrakeTorque)
    {
        var engineTorque = GetTorque(_rpm, throttleInput);
        var engienFriTorque = GetFrictionTorque(_rpm, throttleInput);

        var driveTorque = engineTorque * totalGearRatio;
        var drivetrainI = totalGearRatio * totalGearRatio * _inertia;

        var friTorque = engienFriTorque * totalGearRatio;

        var brakeTorque = totalBrakeTorque * totalGearRatio;

        var newRPM = _rpm;
        newRPM += (driveTorque / drivetrainI) * Time.fixedDeltaTime * CarMath.RPSToRPM;
        newRPM -= (friTorque / drivetrainI) * Time.fixedDeltaTime * CarMath.RPSToRPM;
        newRPM -= (brakeTorque / drivetrainI) * Time.fixedDeltaTime * CarMath.RPSToRPM;
        newRPM = ClampRPM(newRPM);

        return newRPM;
    }

    private float ClampRPM(float rpm)
    {
        return Mathf.Max(rpm, IdleRPM);
    }

    private void FuelCut(ref float throttleInput)
    {
        if (_remainFuelCutTime > 0f)
        {
            _remainFuelCutTime = Mathf.Max(_remainFuelCutTime - Time.deltaTime, 0f);

            throttleInput = 0f;
        }
        else
        {
            if (_rpm > RevLimitRPM)
            {
                _remainFuelCutTime = _fuelCutTime;

                throttleInput = 0f;
            }
        }
    }

    private void UpdateMixture(float throttleInput)
    {
        var diff = Mathf.Clamp01(_prevThrottleInput - throttleInput);
        _prevThrottleInput = throttleInput;

        var newRemainMixtureUnbalanceTime = _mixtureUnbalanceTime * diff;
        _remainMixtureUnbalanceTime = Mathf.Max(newRemainMixtureUnbalanceTime, _remainMixtureUnbalanceTime);

        _remainMixtureUnbalanceTime = Mathf.Max(_remainMixtureUnbalanceTime - Time.deltaTime, 0f);

        if ((_rpm >= _minAfterFireRPM) && (_remainMixtureUnbalanceTime > 0f))
        {
            var normRemainTime = _remainMixtureUnbalanceTime / _mixtureUnbalanceTime;
            var prob = _afterFireProbability * normRemainTime;

            if (Random.value <= prob)
            {
                _onAfterFire.Invoke();
            }
        }
    }

    public (float maxWatt, float maxWattRPM) CalcMaxPower(float step = 1f)
    {
        step = Mathf.Max(step, 1f);
        var maxWatt = 0f;
        var maxWattRpm = 0f;
        for (var rpm = IdleRPM; rpm <= RevLimitRPM; rpm += step)
        {
            var torque = GetTorque(rpm, 1f);
            var watt = torque * rpm * (2f * Mathf.PI / 60f);
            if (watt > maxWatt)
            {
                maxWatt = watt;
                maxWattRpm = rpm;
            }
        }
        return (maxWatt, maxWattRpm);
    }

    public (float maxTorque, float maxTorqueRPM) CalcMaxTorque(float step = 1f)
    {
        step = Mathf.Max(step, 1f);
        var maxTorque = 0f;
        var maxTorqueRPM = 0f;
        for (var rpm = IdleRPM; rpm <= RevLimitRPM; rpm += step)
        {
            var torque = GetTorque(rpm, 1f);
            if (torque > maxTorque)
            {
                maxTorque = torque;
                maxTorqueRPM = rpm;
            }
        }
        return (maxTorque, maxTorqueRPM);
    }
}
