using UnityEngine;
using UnityEngine.InputSystem;

public class InputCarController : DriverBase
{
    [SerializeField, Min(0.001f)] private float steerTime = 0.1f; //default 0.1
    [SerializeField, Min(0.001f)] private float steerReleaseTime = 0.1f; //default 0.1

    [SerializeField, Min(0.001f)] private float throttleTime = 0.1f;
    [SerializeField, Min(0.001f)] private float throttleReleaseTime = 0.1f;

    [SerializeField, Min(0.001f)] private float brakeTime = 0.1f;
    [SerializeField, Min(0.001f)] private float brakeReleaseTime = 0.1f;

    [SerializeField] private bool steerLimitByFriction = false;
    [SerializeField, Min(0f)] private float steerMu = 2f;

    [SerializeField] private bool autoShiftToReverse = true;
    [SerializeField, Min(0f)] private float switchToReverseSpeedKPH = 1f;

    [Header("Virtual Button Setup")]
    [SerializeField] private bool useVirtualPad = true;

    [Header("AI Setup")]
    [SerializeField] public AIMode aIMode;
    [SerializeField] public bool isAICar = true;
    [SerializeField] private bool isAvoidingCars = true;
    [SerializeField, Range(0.3f, 1f)] private float skillLevel = 1f;
    [SerializeField] private float avoidanceDistance = 12f;
    [SerializeField] private float avoidanceStrength = 1.3f;
    private Vector3 avoidanceVectorLerped = Vector3.zero;
    private BoxCollider detectionCollider;

    [Header("Waypoint")]
    [SerializeField] private string wayPointName = "Waypoint"; //default "Waypoint"
    [SerializeField, ReadOnly] public WaypointNode currentWaypoint;
    [SerializeField, ReadOnly] public WaypointNode previousWaypoint;
    [SerializeField, ReadOnly] private WaypointNode[] allWayPoints;
    private Vector3 currentTargetPos = Vector3.zero;
    private Vector3 avoidanceDirectionLerped = Vector3.zero;

    [Header("Input System Param")]
    [SerializeField, ReadOnly] Vector2 moveInput;
    [SerializeField, ReadOnly] bool handbrakeInputAction;
    [SerializeField, ReadOnly] bool nitroInputAction;

    [Header("Reset to track")]
    [SerializeField] private float resetCooldown = 3f;
    //[SerializeField] private float resetHeightOffset = 2f; // để xe không chìm dưới đất
    private float resetTimer;

    [Header("Connect script")]
    [SerializeField, ReadOnly] private CarLightController carLightController;
    [SerializeField, ReadOnly] private CameraCarController cameraController;

    protected override void Awake()
    {
        base.Awake();
        //AICar
        InitializeWaypoint();
        if (isAICar)
        {
            useVirtualPad = false;
        }
    }
    private void InitializeWaypoint()
    {
        //Find Object
        allWayPoints = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);

        //Manual sorting for simple and run 1 time 
        if (allWayPoints.Length > 1)
        {
            System.Array.Sort(allWayPoints, (a, b) =>
            {
                if (a.name == wayPointName) return -1;
                if (b.name == wayPointName) return 1;

                string aName = a.name.Replace(wayPointName, "").Replace("(", "").Replace(")", "");
                string bName = b.name.Replace(wayPointName, "").Replace("(", "").Replace(")", "");

                if (int.TryParse(aName, out int indexA) && int.TryParse(bName, out int indexB))
                    return indexA.CompareTo(indexB);

                return string.Compare(a.name, b.name, System.StringComparison.Ordinal);
            });
        }
    }
    public void SetupPlayerInput(bool isPlayer)
    {
        if (isPlayer)
        {
            // Nếu là player → add PlayerInput nếu chưa có
            if (GetComponent<PlayerInput>() == null)
            {
                var playerInput = gameObject.AddComponent<PlayerInput>();
                // Config cơ bản (bạn cần set Action Map, Default Map, v.v. ở đây hoặc prefab gốc)
                // Ví dụ:
                // playerInput.actions = YourInputActionsAsset;  // Kéo Input Action Asset vào Inspector nếu có
                // playerInput.defaultMap = "Player";
                // playerInput.defaultControlScheme = "Keyboard&Mouse"; // Hoặc "Gamepad"
                playerInput.enabled = true;
                Debug.Log($"Added PlayerInput for player car: {gameObject.name}");
            }
            isAICar = false;
            useVirtualPad = true;
        }
        else
        {
            // Nếu là AI → remove PlayerInput nếu có (sau race hoặc spawn AI)
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                Destroy(playerInput);
                Debug.Log($"Removed PlayerInput for AI car: {gameObject.name}");
            }
            isAICar = true;
            useVirtualPad = false;
        }
    }
    void Start()
    {
        //Camera
        if (cameraController == null && RaceManager.Instance)
        {
            cameraController = RaceManager.Instance.cameraCarController;
        }
        //Light
        if (carLightController == null)
        {
            carLightController = GetComponentInChildren<CarLightController>();
        }
        //Collider
        detectionCollider = GetComponentInChildren<BoxCollider>();
    }
    public bool SteerLimitByFriction
    {
        get => steerLimitByFriction;
        set => steerLimitByFriction = value;
    }
    public bool AutoSwitchToReverse
    {
        get => autoShiftToReverse;
        set => autoShiftToReverse = value;
    }
    public bool EnableVirtualPad
    {
        get => useVirtualPad;
        set => useVirtualPad = value;
    }
    #region Input System
    private void OnMove(InputValue inputValue) //4 directions
    {
        moveInput = inputValue.Get<Vector2>();
    }
    private void OnNitro(InputValue inputValue)
    {
        nitroInputAction = inputValue.isPressed;
    }
    private void OnHandbrake(InputValue inputValue)
    {
        handbrakeInputAction = inputValue.isPressed;
    }
    private void OnSwitchCamera()
    {
        cameraController?.SwitchCamera(); //Check true for switch camera
    }
    private void OnBackOnTrack()
    {
        if (isAICar) return; // AI can't reset to track by button

        if (resetTimer > 0f) return;

        ResetCarToTrack();
        resetTimer = resetCooldown;
    }
    #endregion
    protected override void Drive()
    {
        if (RaceManager.Instance.isCountdown == false) //Player + AI need for end countDown before race
        {
            if (currentWaypoint == null)
            {
                currentWaypoint = FindClosestWaypoint();
                if (currentWaypoint == null) return;
            }
            UpdateWaypointTarget();
            if (isAICar)
            {
                AIInputDrive(); // Chạy AI với random waypoint
            }
            else
            {
                PlayerInputDrive();
            }
        }

        if (resetTimer > 0f)
        {
            resetTimer -= Time.unscaledDeltaTime;
        }

    }
    private void PlayerInputDrive()
    {
        UpdateSteerInput();
        UpdateThrottleAndBrakeInput();
        ShiftChange();
        UpdateClutchInput();
        UpdateNOSInput();
    }
    #region AI Car
    private void AIInputDrive()
    {
        // if (currentWaypoint == null)
        // {
        //     currentWaypoint = FindClosestWaypoint();
        //     if (currentWaypoint == null) return;
        // }

        //UpdateWaypointTarget();

        // <<<< ÁP DỤNG TỐC ĐỘ TỪ WAYPOINT (phần quan trọng nhất)
        float targetSpeedMultiplier = 1f; // mặc định full speed

        if (currentWaypoint != null && carController is ArcadeCarController arcade)
        {
            CarClass thisCarClass = arcade.CarClass;
            //Debug.Log(thisCarClass);
            targetSpeedMultiplier = currentWaypoint.GetMaxSpeedMultiplierForClass(thisCarClass);
            //Debug.Log(targetSpeedMultiplier);
        }


        float maxSpeedMPS = carController.MaxSpeedKPH / 3.6f; // Chuyển km/h sang m/s (tương đương CarMath.KPHToMPS)
        float currentMaxSpeed = maxSpeedMPS * targetSpeedMultiplier;

        // Giới hạn tốc độ hiện tại (rất mượt, không giật)
        if (carController.ForwardSpeed > currentMaxSpeed + 5f) // +5f để có độ trễ tự nhiên
        {
            carController.BrakeInput = Mathf.MoveTowards(carController.BrakeInput, 1f, Time.deltaTime * 3f);
        }
        else
        {
            carController.BrakeInput = Mathf.MoveTowards(carController.BrakeInput, 0f, Time.deltaTime * 5f);
        }

        Vector3 toTarget = (currentTargetPos - transform.position);
        float distanceToTarget = toTarget.magnitude;
        if (distanceToTarget < 0.5f) return;

        Vector3 dirToTarget = toTarget.normalized;

        // === TRÁNH XE ===
        if (isAvoidingCars && carController.ForwardSpeed > 8f)
            AvoidCars(ref dirToTarget);

        // === TÍNH GÓC LÁI ===
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Vector3 flatTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
        float angle = Vector3.SignedAngle(flatForward, flatTarget, Vector3.up);
        float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

        // === GA & PHANH ===
        // float speedNorm = carController.ForwardSpeed / 50f;
        // float throttle = 1f - (Mathf.Abs(steerInput) * 0.8f * (1f - skillLevel)) - speedNorm * 0.15f;
        // throttle = Mathf.Clamp01(throttle);

        // Tự động giảm ga khi vào cua (kết hợp với brake ở trên)
        float baseThrottle = 1f;
        float corneringReduction = Mathf.Abs(steerInput) * 0.7f * (1f - skillLevel);
        float speedReduction = Mathf.Clamp01((carController.ForwardSpeed - currentMaxSpeed) / 30f);
        float throttle = baseThrottle - corneringReduction - speedReduction;
        throttle = Mathf.Clamp01(throttle);

        // Áp dụng mượt như player
        carController.SteerInput = Mathf.MoveTowards(carController.SteerInput, steerInput,
            Time.deltaTime / (steerInput != 0f ? steerTime : steerReleaseTime));

        carController.ThrottleInput = Mathf.MoveTowards(carController.ThrottleInput, throttle,
            Time.deltaTime / throttleTime);

        carController.BrakeInput = Mathf.MoveTowards(carController.BrakeInput, 0f,
            Time.deltaTime / brakeReleaseTime);



        // Handbrake khi cua quá gắt (drift nhẹ cho đẹp)
        bool hardDrift = Mathf.Abs(steerInput) > 0.85f && carController.ForwardSpeed > 30f && targetSpeedMultiplier < 0.7f;
        carController.HandbrakeInput = hardDrift && Random.value < 0.3f;

        carLightController.ToggleRearLights(carController.HandbrakeInput);
    }
    private void UpdateWaypointTarget()
    {
        currentTargetPos = currentWaypoint.transform.position;

        float dist = Vector3.Distance(transform.position, currentTargetPos);
        if (dist > 30f && previousWaypoint != null)
        {
            Vector3 nearest = NearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);
            currentTargetPos = Vector3.Lerp(currentTargetPos, nearest, 0.6f);
        }

        if (dist < currentWaypoint.minDistanceToReachWaypoint)
        {
            previousWaypoint = currentWaypoint;
            var nexts = currentWaypoint.nextWaypointNode;
            currentWaypoint = nexts != null && nexts.Length > 0
                ? nexts[Random.Range(0, nexts.Length)]
                : FindClosestWaypoint() ?? currentWaypoint;
        }
    }

    private void AvoidCars(ref Vector3 dirToTarget)
    {
        Vector3 origin = transform.position + transform.forward * 1f;
        if (Physics.SphereCast(origin, 2.2f, transform.forward, out RaycastHit hit, avoidanceDistance, LayerMask.GetMask("Car")))
        {
            if (hit.collider.transform != transform && hit.rigidbody != carController.Rigidbody)
            {
                Vector3 reflectDir = Vector3.Reflect((hit.point - transform.position).normalized, hit.collider.transform.right);
                avoidanceDirectionLerped = Vector3.Lerp(avoidanceDirectionLerped, reflectDir, Time.deltaTime * 6f);

                float influence = avoidanceStrength * (1f - hit.distance / avoidanceDistance);
                dirToTarget = Vector3.Lerp(dirToTarget, avoidanceDirectionLerped, influence).normalized;
            }
        }
    }

    private WaypointNode FindClosestWaypoint()
    {
        WaypointNode best = null;
        float bestDist = float.MaxValue;
        foreach (var wp in allWayPoints)
        {
            if (wp == null) continue;
            float d = Vector3.Distance(transform.position, wp.transform.position);
            if (d < bestDist) { bestDist = d; best = wp; }
        }
        return best;
    }

    private Vector3 NearestPointOnLine(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 heading = b - a;
        float mag = heading.magnitude;
        if (mag < 0.001f) return a;
        heading.Normalize();
        float dot = Mathf.Clamp(Vector3.Dot(p - a, heading), 0f, mag);
        return a + heading * dot;
    }
    #endregion

    #region Player Car
    protected override void Stop()
    {
        carController.BrakeInput = 1f;

        var engineCar = carController as RealisticCarController;
        if (engineCar != null)
        {
            engineCar.ClutchInput = true;
        }

        var throttleInput = GetRawThrottleInput();

        var throttleTime = throttleInput != 0f ? this.throttleTime : throttleReleaseTime;
        carController.ThrottleInput = Mathf.MoveTowards(carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);
    }

    private float GetRawSteerInput()
    {
        //VirtualPad (Mobile - Future feature) 
        if (useVirtualPad && VirtualPadController.Instance != null)
        {
            if (VirtualPadController.Instance.LeftSteerPressed)
            {
                return -1f;
            }
            else if (VirtualPadController.Instance.RightSteerPressed)
            {
                return 1f;
            }
        }
        //Keyboard (PC)
        //Steer Left
        if (moveInput.x < 0 || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //Combine Input System & Input Manager
        {
            return -1f;
        }
        //Steer Right
        else if (moveInput.x > 0 || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) //Combine Input System & Input Manager
        {
            return 1f;
        }

        return 0f;
    }

    private float GetRawThrottleInput()
    {
        //VirtualPad (Mobile - Future feature)
        if (useVirtualPad && VirtualPadController.Instance != null && VirtualPadController.Instance.ThrottlePressed)
        {
            return 1f;
        }
        //Keyboard (PC)
        else if (moveInput.y > 0 || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) //Combine Input System & Input Manager
        {
            return 1f;
        }

        return 0f;
    }

    private float GetRawBrakeInput() //Brake (not Handbrake)
    {
        //VirtualPad (Mobile - Future feature)
        if (useVirtualPad && VirtualPadController.Instance != null && VirtualPadController.Instance.BrakePressed)
        {
            return 1f;
        }
        //Keyboard (PC)
        else if (moveInput.y < 0 || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) //Combine Input System & Input Manager
        {
            return 1f;
        }
        return 0f;
    }
    #region Steer
    private void UpdateSteerInput()
    {
        var maxSteerInput = 1f;
        if (steerLimitByFriction)
        {
            var speed = carController.Speed;
            var minTurnR = (speed * speed) / (steerMu * UnityEngine.Physics.gravity.magnitude);
            if (minTurnR > 0f)
            {
                var optimalSteerAngle = Mathf.Asin(carController.Wheelbase / minTurnR) * Mathf.Rad2Deg;
                maxSteerInput = Mathf.Min(optimalSteerAngle / carController.MaxSteerAngle, 1f);
            }
        }

        var steerInput = GetRawSteerInput();

        steerInput = Mathf.Clamp(steerInput, -maxSteerInput, maxSteerInput);

        var steerTime = steerInput != 0f ? this.steerTime : steerReleaseTime;

        if (steerInput != 0f && Mathf.Sign(steerInput) != Mathf.Sign(carController.SteerInput))
        {
            carController.SteerInput = 0f;
        }

        carController.SteerInput = Mathf.MoveTowards(carController.SteerInput, steerInput, Time.deltaTime / steerTime);
    }
    #endregion

    #region Throttle & Brake
    private void UpdateThrottleAndBrakeInput()
    {
        var throttleInput = GetRawThrottleInput();

        var brakeInput = GetRawBrakeInput();

        if (autoShiftToReverse)
        {
            if (carController.IsGrounded())
            {
                var speedKPH = carController.ForwardSpeed * CarMath.MPSToKPH;
                if (carController.Reverse)
                {
                    if (throttleInput > 0f && speedKPH > -switchToReverseSpeedKPH)
                    {
                        carController.Reverse = false;
                    }
                }
                else
                {
                    if (brakeInput > 0f && speedKPH < switchToReverseSpeedKPH)
                    {
                        carController.Reverse = true;
                    }
                }
            }
            if (carController.Reverse)
            {
                (throttleInput, brakeInput) = (brakeInput, throttleInput);
            }
        }

        var throttleTime = throttleInput != 0f ? this.throttleTime : throttleReleaseTime;
        carController.ThrottleInput = Mathf.MoveTowards(carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);

        var brakeTime = brakeInput != 0f ? this.brakeTime : brakeReleaseTime;
        carController.BrakeInput = Mathf.MoveTowards(carController.BrakeInput, brakeInput, Time.deltaTime / brakeTime);

        //Handbrake
        //Input Manager
        bool handbrakeInput = handbrakeInputAction || Input.GetKey(KeyCode.LeftShift); //Combine Input System & Input Manager
        carController.HandbrakeInput = handbrakeInput;

        //Eneble Light handbrake
        carLightController.ToggleRearLights(handbrakeInput);
    }
    #endregion

    #region Engine Car (R&D)
    private void ShiftChange()
    {
        var engineCar = carController as RealisticCarController;
        if (engineCar == null)
        {
            return;
        }

        if (engineCar.Transmission.AutoShift)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!autoShiftToReverse
                || (autoShiftToReverse && engineCar.Transmission.Gear != Transmission.ReverseGear))
            {
                engineCar.Transmission.ShiftDonw();
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            engineCar.Transmission.ShiftUp();
        }
    }

    private void UpdateClutchInput()
    {
        var engineCar = carController as RealisticCarController;
        if (engineCar == null)
        {
            return;
        }

        engineCar.ClutchInput = Input.GetKey(KeyCode.Z);
    }
    #endregion

    #region NITRO
    private void UpdateNOSInput()
    {
        CarControllerBase car = carController;
        bool nitroInput = nitroInputAction || Input.GetKey(KeyCode.Space); //Combine Input System & Input Manager
        if (car != null)
        {
            if (car is ArcadeCarController electric)
            {
                electric.NOSInput = nitroInput;
            }
            else if (car is RealisticCarController realistic)
            {
                realistic.NOSInput = nitroInput;
            }
        }
    }
    #endregion
    #endregion
    private void ResetCarToTrack()
    {
        // if (RaceController.Instance == null || RaceController.Instance.allCheckPoints == null || RaceController.Instance.allCheckPoints.Length == 0)
        // {
        //     Debug.LogWarning("[InputCarController] Không tìm thấy checkpoint để reset!");
        //     return;
        // }

        // // Lấy checkpoint cuối cùng xe đã đi qua
        // int targetCheckpointIndex = carController.nextCheckPoint - 1;
        // if (targetCheckpointIndex < 0)
        //     targetCheckpointIndex = RaceController.Instance.allCheckPoints.Length - 1;

        // Transform checkpoint = RaceController.Instance.allCheckPoints[targetCheckpointIndex];

        // // Reset vị trí + hướng
        // transform.position = checkpoint.position + Vector3.up * resetHeightOffset;
        // transform.rotation = checkpoint.rotation;

        // // Dừng hoàn toàn xe
        // Rigidbody rb = carController.Rigidbody;
        // if (rb != null)
        // {
        //     rb.linearVelocity = Vector3.zero;
        //     rb.angularVelocity = Vector3.zero;
        // }

        // // Reset thêm một số trạng thái nếu cần (tùy car controller bạn dùng)
        // carController.ThrottleInput = 0f;
        // carController.BrakeInput = 0f;
        // carController.SteerInput = 0f;
        // carController.HandbrakeInput = false;
    }
}

