using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public abstract class CarControllerBase : MonoBehaviour
{
    #region Setting Car
    [SerializeField] Wheel[] steerableWheels; //Wheel list

    [SerializeField, Min(0f)] protected float maxSteerAngle = 30f; //Maximun Stree Angle - Góc đánh lái tối đa

    [SerializeField, Min(0f)] protected float maxTurnSpeed = 60f; //Maximum rotation speed when driving - Tốc độ dánh lái tối đa
    [SerializeField, Min(0f)] protected float turnSpeedWithHandbrake = 120f; //Rotation speed while using handbrake - Tốc độ quay khi đang phanh tay

    [SerializeField, Min(0f)] protected float peakFrictionSlipAngle = 5f; //The sliding angle at which friction reaches its peak - Góc trước tại đó ma sát đạt tối đa
    [SerializeField, Min(0f)] protected float mu = 2f; //Static friction coefficient (μ) - Hệ số ma sát tĩnh 

    [SerializeField] protected bool useAddTorque = false; 

    [SerializeField, Min(0.001f)] protected float wheelRadius = 0.3f; //Wheel Radius - Bánh kính bánh xe

    [SerializeField] protected float centerOfMassHeight = 0.3f; //Center of mass height - Chiều cao tâm khối lượng

    [SerializeField, Min(0f)] protected float maxBrakeTorque = 1000f; //Max Brake Torque - Mô-mem phanh tối đa mỗi bánh

    [SerializeField, Min(0f)] protected float rollingResistanceCoef = 0.015f; //Rolling resistance coefficient - Hệ số cản khi lăn 
    [SerializeField, Min(0f)] protected float airResistanceCoef = 1.5f; //Air resistance coefficien - Hệ số cản không khí
    [SerializeField, Min(0f)] protected float downforceCoef = 0f; //Downforce - Hệ số lực ép xuống

    [SerializeField, Range(0f, 1f)] protected float airResistanceReduction = 0f;
    #endregion

    #region Suspension
    [SerializeField] protected bool autoAdjustSuspension = true; //Auto Suspension - Bật/tắt tự động Hệ thống treo
    [SerializeField, Min(0.001f)] protected float suspensionStroke = 0.1f;
    [SerializeField, Min(0f)] protected float suspensionNaturalFrequency = 2f; //Frequency - Tần số 
    [SerializeField, Range(0f, 1f)] protected float suspensionDampingRatio = 0.35f; //Damping - Hệ số giảm chấn
    #endregion
    [SerializeField] protected float addForceOffset = -0.1f;

    #region RaceTracking
    [SerializeField, ReadOnly] private InputCarController inputCarController;
    [SerializeField] public int currentLap = 1; //default = 1
    [SerializeField] public float lapTime;
    [SerializeField] public float bestLapTime;
    [SerializeField] public int nextCheckPoint;
    public void CheckPointHit(int checkPointNumber)
    {
        if (checkPointNumber == nextCheckPoint)
        {
            nextCheckPoint++;

            if (nextCheckPoint == RaceManager.Instance.allCheckPoints.Length)
            {
                nextCheckPoint = 0;
                LapCompleted();
            }

        }
    }
    void LapCompleted()
    {
        currentLap++; //When completed lap
        if (lapTime < bestLapTime || bestLapTime == 0f)
        {
            bestLapTime = lapTime;
        }

        if (currentLap <= RaceManager.Instance.totalLaps)
        {
            lapTime = 0f; //Reset value for new lap
            //Show best lap time
            if (inputCarController != null && !inputCarController.isAICar)
            {
                var bestTime = System.TimeSpan.FromSeconds(bestLapTime);
                if (UIManager.HasInstance)
                {
                    //DisplayBestTime
                    UIManager.Instance.hUDPanel.bestLapTimeTxt.text = string.Format("{0:00}:{1:00}.{2:00}", bestTime.Minutes, bestTime.Seconds, bestTime.Milliseconds);
                    //UIManager.Instance.resultPanel.bestTimeTxt.text = string.Format("{0:00}:{1:00}.{2:00}", bestTime.Minutes, bestTime.Seconds, bestTime.Milliseconds);
                }
                //Show current lap
                DisplayLap();
            }
        }
        else
        {
            // If player completes the race
            if (!inputCarController.isAICar)
            {
                bestLapTime = lapTime; //Update laterTime
                var bestTime = System.TimeSpan.FromSeconds(bestLapTime);
                inputCarController.isAICar = true; // Stop updating Pos, Time, Lap
                if (UIManager.HasInstance)
                {
                    UIManager.Instance.StopCountdown(); // Stop countdown if running
                    UIManager.Instance.resultPanel.bestTimeTxt.text = string.Format("{0:00}:{1:00}.{2:00}", bestTime.Minutes, bestTime.Seconds, bestTime.Milliseconds);
                }
                RaceManager.Instance.FinishRace();

            }
            // If AI completes the race
            else
            {
                if (UIManager.HasInstance && !UIManager.Instance.isCountingDown && !RaceManager.Instance.raceCompleted)//Check countDown, raceCompleted
                {
                    UIManager.Instance.SetEndCountDown(UIManager.Instance.timeIfNotFinish); // Start countdown
                }
            }
        }
    }
    private void DisplayLap()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.lapTxt.text = currentLap + "/" + RaceManager.Instance.totalLaps;
        }
    }
    private void DisplayTime()
    {
        var time = System.TimeSpan.FromSeconds(lapTime);
        if (UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.lapTimeTxt.text = string.Format("{0:00}:{1:00}.{2:00}", time.Minutes, time.Seconds, time.Milliseconds);
        }
    }
    private void DisplaySpeed()
    {
        if(UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.speedDometerText.text = Mathf.RoundToInt(SpeedKPH).ToString();
        }
    }
    public void AISetup(bool isAI) //This use for spawn car
    {
        if (inputCarController != null)
        {
            inputCarController.isAICar = isAI;
        }
    }
    #endregion

    #region Variable
    //---Physics---
    protected Rigidbody carRB;
    protected Collider carCollider;
    //---Wheell---
    protected Wheel[] carWheels;
    protected float wheelbase;
    //---Input Handler---
    protected float steerInput;
    protected float throttleInput;
    protected float brakeInput;
    protected bool handbrakeInput;
    //---Angular Velocity---
    protected float angularVelocity;
    //---Ground Dection---
    protected Vector3 groundNormal;
    protected Vector3 groundForward;
    protected Vector3 groundSideways;
    //---Speed---
    protected float forwardSpeed; 
    protected float sidewaysSpeed;
    protected float speed;
    //---Force---
    protected float normalForce;
    protected Vector3 addForcePosition;
    protected Vector3 totalForce;
    //---Angle---
    protected float slipAngle;
    protected float tiltAngle;

    protected bool replayPlaying;
    #endregion

    public abstract float MaxSpeedKPH { get; }

    public float Weight
    {
        get
        {
            if (carRB == null)
            {
                carRB = GetComponent<Rigidbody>();
            }
            return carRB.mass;
        }
    }

    public Wheel[] SteerableWheels
    {
        get => steerableWheels;
        set => steerableWheels = value;
    }

    public float Wheelbase => wheelbase;

    public float SlipAngle => slipAngle;

    #region Input
    public float SteerInput
    {
        get => steerInput;
        set
        {
            if (!replayPlaying)
            {
                steerInput = Mathf.Clamp(value, -1f, 1f);
            }
        }
    }

    public float ThrottleInput
    {
        get => throttleInput;
        set
        {
            if (!replayPlaying)
            {
                throttleInput = Mathf.Clamp(value, 0f, 1f);
            }
        }
    }

    public float BrakeInput
    {
        get => brakeInput;
        set
        {
            if (!replayPlaying)
            {
                brakeInput = Mathf.Clamp(value, 0f, 1f);
            }
        }
    }

    public bool HandbrakeInput
    {
        get => handbrakeInput;
        set
        {
            if (!replayPlaying)
            {
                handbrakeInput = value;
            }
        }
    }
    #endregion
    #region ParamCar
    public abstract bool Reverse { get; set; }

    public float ForwardSpeed => Vector3.Dot(carRB.linearVelocity, transform.forward);
    public float ForwardSpeedKPH => ForwardSpeed * CarMath.MPSToKPH;

    public float Speed => speed;
    public float SpeedKPH => Speed * CarMath.MPSToKPH;

    public Vector3 Velocity => carRB.linearVelocity;

    public float MaxSteerAngle => maxSteerAngle;

    public float MaxTurnSpeed
    {
        get => maxTurnSpeed;
        set => maxTurnSpeed = Mathf.Max(value, 0f);
    }

    public float PeakFrictionSlipAngle
    {
        get => peakFrictionSlipAngle;
        set => peakFrictionSlipAngle = Mathf.Max(value, 0f);
    }

    public float Mu
    {
        get => mu;
        set => mu = Mathf.Max(value, 0f);
    }

    public float WheelRadius
    {
        get => wheelRadius;
        set => wheelRadius = Mathf.Max(value, 0.001f);
    }

    public float RollingResistanceCoef
    {
        get => rollingResistanceCoef;
        set => rollingResistanceCoef = Mathf.Max(value, 0f);
    }

    public float AirResistanceReduction
    {
        get => airResistanceReduction;
        set => airResistanceReduction = Mathf.Clamp01(value);
    }

    public float MaxBrakeTorque => maxBrakeTorque;

    public Rigidbody Rigidbody => carRB;

    public Collider Collider => carCollider;

    public Wheel[] Wheels => carWheels;

    public bool Replay => replayPlaying;

    public abstract float MotorRevolutionRate { get; }
    #endregion
    #region Awake
    protected virtual void Awake()
    {
        carRB = GetComponent<Rigidbody>();
        carCollider = GetComponentInChildren<Collider>();
        carWheels = GetComponentsInChildren<Wheel>();

        CalcWheelbase();

        AdjustCenterOfMass();

        if (autoAdjustSuspension)
        {
            AdjustSuspension();
        }
    }
    #endregion
    #region Start
    protected virtual void Start()
    {
        inputCarController = GetComponent<InputCarController>();
        if(inputCarController.isAICar == false)
        {
            DisplayLap();
            DisplayTime();
            DisplaySpeed();
        }
    }
    #endregion
    #region Update
    protected virtual void Update()
    {
        if (!RaceManager.Instance.isCountdown)
        {
            lapTime += Time.deltaTime;
            if (!inputCarController.isAICar)
            {
                DisplayTime();
                DisplaySpeed();
                if (UIManager.HasInstance && UIManager.Instance.isCountingDown && UIManager.Instance.GetEndCountDown() <= 0.1f)//When end time, just finishRace
                {
                    inputCarController.isAICar = true;
                    RaceManager.Instance.FinishRace();    
                }
            }
        }
    }
    #endregion
    #region Fixupdate
    protected virtual void FixedUpdate()
    {
        groundNormal = Vector3.zero;
        groundForward = Vector3.zero;
        groundSideways = Vector3.zero;
        forwardSpeed = 0f;
        sidewaysSpeed = 0f;
        slipAngle = 0f;
        normalForce = 0f;
        addForcePosition = Vector3.zero;
        totalForce = Vector3.zero;

        speed = carRB.linearVelocity.magnitude;

        UpdateSteerAngle();

        if (replayPlaying)
        {
            return;
        }

        AddAirResistanceForce();
        AddDownforce();

        if (!IsGrounded())
        {
            return;
        }

        groundNormal = GetGroundNormal();
        groundForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
        groundSideways = Vector3.ProjectOnPlane(transform.right, groundNormal).normalized;

        forwardSpeed = Vector3.Dot(carRB.linearVelocity, groundForward);
        sidewaysSpeed = Vector3.Dot(carRB.linearVelocity, groundSideways);

        var denom = Mathf.Max(Mathf.Abs(forwardSpeed), 1f);
        slipAngle = Mathf.Atan2(sidewaysSpeed, denom) * Mathf.Rad2Deg;
        tiltAngle = Vector3.Angle(groundNormal, transform.up);

        normalForce = carRB.mass * UnityEngine.Physics.gravity.magnitude;

        addForcePosition = carRB.worldCenterOfMass + transform.up * addForceOffset;

        Turn();
        AddFrictionForce();

        AddRollingResistanceForce();
        AddBrakeForce();
    }
    #endregion
    private void OnDrawGizmosSelected()
    {
        if (carRB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.TransformPoint(carRB.centerOfMass), totalForce * 0.0001f);
        }
    }

    private void CalcWheelbase() //calculate Wheel Base
    {
        var minLocalZ = float.MaxValue;
        var maxLocalZ = float.MinValue;
        foreach (var wheel in carWheels)
        {
            minLocalZ = Mathf.Min(wheel.transform.localPosition.z, minLocalZ);
            maxLocalZ = Mathf.Max(wheel.transform.localPosition.z, maxLocalZ);
        }
        wheelbase = Mathf.Abs(minLocalZ - maxLocalZ);
    }

    private void AdjustCenterOfMass()
    {
        if (carWheels.Length == 0)
        {
            return;
        }

        var com = Vector3.zero;
        foreach (var wheel in carWheels)
        {
            com += wheel.transform.localPosition;
        }
        com /= (float)carWheels.Length;

        com.y = centerOfMassHeight;

        carRB.centerOfMass = com;
    }

    private void AdjustSuspension()
    {
        var mass = carRB.mass / carWheels.Length;

        var spring = 4f * Mathf.PI * Mathf.PI * suspensionNaturalFrequency * suspensionNaturalFrequency * mass;
        var damper = 2f * Mathf.Sqrt(mass * spring) * suspensionDampingRatio;

        foreach (var wheel in carWheels)
        {
            wheel.SuspensionStroke = suspensionStroke;
            wheel.SuspensionSpring = spring;
            wheel.SuspensionDamper = damper;
        }
    }

    private void UpdateSteerAngle()
    {
        var steerAngle = maxSteerAngle * steerInput;
        foreach (var wheel in steerableWheels)
        {
            wheel.SteerAngle = steerAngle;
        }
    }

    public bool IsGrounded()
    {
        foreach (var wheel in carWheels)
        {
            if (wheel.Grounded)
            {
                return true;
            }
        }
        return false;
    }
    #region Calculate Force
    private Vector3 GetGroundNormal()
    {
        var normal = Vector3.zero;
        var count = 0;
        foreach (var wheel in carWheels)
        {
            if (!wheel.Grounded)
            {
                continue;
            }
            normal += wheel.HitInfo.normal;
            count++;
        }

        if (count == 0)
        {
            return Vector3.zero;
        }

        normal /= (float)count;
        return normal.normalized;
    }

    private void Turn()
    {
        var steerAngle = maxSteerAngle * steerInput;

        var targetAngVel = 0f;
        if (steerAngle != 0f)
        {
            var turnR = wheelbase / Mathf.Sin(steerAngle * Mathf.Deg2Rad);
            targetAngVel = forwardSpeed / turnR;

            var minTurnR = (speed * speed) / (mu * UnityEngine.Physics.gravity.magnitude);
            var maxAngVel1 = speed / minTurnR;

            var maxTurnSpeed = handbrakeInput ? turnSpeedWithHandbrake : this.maxTurnSpeed;
            var maxAngVel2 = maxTurnSpeed * Mathf.Deg2Rad;

            var maxAngVel = Mathf.Max(maxAngVel1, maxAngVel2);

            targetAngVel = Mathf.Clamp(targetAngVel, -maxAngVel, maxAngVel);
        }

        var currAngVel = useAddTorque ? carRB.angularVelocity.y : angularVelocity;
        var angVelDiff = targetAngVel - currAngVel;
        var velDiff = (wheelbase / 2f) * angVelDiff;
        var torque = ((normalForce / 2f) * velDiff) * 2f;
        var maxFriction = normalForce * mu;
        torque = Mathf.Clamp(torque, -maxFriction, maxFriction);

        if (useAddTorque)
        {
            carRB.AddTorque(transform.up * torque);

            angularVelocity = carRB.angularVelocity.y;
        }
        else
        {
            angularVelocity += torque / carRB.inertiaTensor.y * Time.deltaTime;

            var angVel = carRB.angularVelocity;
            angVel.y = angularVelocity;
            carRB.angularVelocity = angVel;
        }
    }

    private void AddFrictionForce()
    {
        var friForce = (carRB.mass * -sidewaysSpeed) / Time.fixedDeltaTime;

        var fri = Mathf.InverseLerp(0f, peakFrictionSlipAngle, Mathf.Abs(slipAngle)) * mu;
        var tilt = Mathf.Cos(tiltAngle * Mathf.Deg2Rad);
        var maxFriForce = normalForce * fri * tilt;
        friForce = Mathf.Clamp(friForce, -maxFriForce, maxFriForce);

        var friForceVec = groundSideways * friForce;
        carRB.AddForceAtPosition(friForceVec, addForcePosition);

        totalForce += friForceVec;
    }

    protected void AddDriveTorque(float driveTorque)
    {
        var driveForce = driveTorque / wheelRadius;

        var maxDriveForce = normalForce * mu;
        driveForce = Mathf.Clamp(driveForce, -maxDriveForce, maxDriveForce);

        var driveForceVec = groundForward * driveForce;
        carRB.AddForceAtPosition(driveForceVec, addForcePosition);

        totalForce += driveForceVec;
    }

    protected void AddBrakeTorque(float brakeTorque)
    {
        var brakeForce = -Mathf.Sign(forwardSpeed) * Mathf.Abs(brakeTorque / wheelRadius);

        var maxBrakeForce1 = (carRB.mass * Mathf.Abs(forwardSpeed)) / Time.fixedDeltaTime;
        var maxBrakeForce2 = normalForce * mu;
        var maxBrakeForce = Mathf.Min(maxBrakeForce1, maxBrakeForce2);
        brakeForce = Mathf.Clamp(brakeForce, -maxBrakeForce, maxBrakeForce);

        var brakeForceVec = groundForward * brakeForce;
        carRB.AddForceAtPosition(brakeForceVec, addForcePosition);

        totalForce += brakeForceVec;
    }

    private void AddRollingResistanceForce()
    {
        var rollResForce = normalForce * rollingResistanceCoef * wheelRadius;
        AddBrakeTorque(rollResForce);
    }

    private void AddBrakeForce()
    {
        var brakeTorque = maxBrakeTorque * brakeInput;
        var totalBrakeForce = brakeTorque * carWheels.Length;

        var handbrakeTorque = handbrakeInput ? maxBrakeTorque : 0f;
        var totalHandbrakeForce = handbrakeTorque * 2f;

        totalBrakeForce = Mathf.Max(totalBrakeForce, totalHandbrakeForce);

        AddBrakeTorque(totalBrakeForce);
    }

    private void AddAirResistanceForce()
    {
        var vel = carRB.linearVelocity;
        var force = -vel.normalized * vel.sqrMagnitude * airResistanceCoef * (1f - airResistanceReduction);
        carRB.AddForce(force);

        totalForce += force;
    }

    private void AddDownforce()
    {
        var forwardVel = Vector3.Dot(carRB.linearVelocity, transform.forward);
        var force = -transform.up * forwardVel * forwardVel * downforceCoef * (1f - airResistanceReduction);
        carRB.AddForce(force);

        totalForce += force;
    }
    #endregion
}