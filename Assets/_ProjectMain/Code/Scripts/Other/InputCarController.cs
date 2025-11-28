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
    [SerializeField] private VirtualPadButton leftSteerButton;
    [SerializeField] private VirtualPadButton rightSteerButton;
    [SerializeField] private VirtualPadButton throttleButton;
    [SerializeField] private VirtualPadButton brakeButton;
    [SerializeField] private bool enableVirtualPad = true;

    [SerializeField, ReadOnly] Vector2 moveInput;
    [SerializeField, ReadOnly] bool handbrakeInputAction;
    [SerializeField, ReadOnly] float turnNitro;

    [Header("Connect script")]
    [SerializeField, ReadOnly] private CarLightController carLightController;
    [SerializeField, ReadOnly] private CameraCarController cameraController;

    void Start()
    {
        if(carLightController == null)
        {
            carLightController = GetComponentInChildren<CarLightController>();
        }
        if(cameraController == null && RaceManager.HasInstance)
        {
            cameraController = RaceManager.Instance.cameraCarController;
        }
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
        get => enableVirtualPad;
        set => enableVirtualPad = value;
    }
    #region Input System
    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }
    private void OnNitro()
    {
        turnNitro++;
    }
    private void OnHandbrake(InputValue inputValue)
    {
        handbrakeInputAction = inputValue.isPressed;
    }
    private void OnSwitchCamera()
    {
        cameraController?.SwitchCamera(); //Check true for switch camera
    }
    #endregion
    protected override void Drive()
    {
        UpdateSteerInput();
        UpdateThrottleAndBrakeInput();
        ShiftChange();
        UpdateClutchInput();
        UpdateNOSInput();
    }
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
        if (enableVirtualPad && leftSteerButton != null && rightSteerButton != null)
        {
            if (leftSteerButton.Pressed)
            {
                return -1f;
            }
            if (rightSteerButton.Pressed)
            {
                return 1f;
            }
        }
        //Steer Left
        if (moveInput.x < 0 || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //Combine Input System & Input Manager
        {
            return -1f;
        }
        //Steer Right
        if (moveInput.x > 0 || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) //Combine Input System & Input Manager
        {
            return 1f;
        }

        return 0f;
    }
 
    private float GetRawThrottleInput()
    {
        if (enableVirtualPad && throttleButton != null)
        {
            if (throttleButton.Pressed)
            {
                return 1f;
            }
        }
        //Throttle
        if (moveInput.y > 0 || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) //Combine Input System & Input Manager
        {
            return 1f;
        }

        return 0f;
    }

    private float GetRawBrakeInput()
    {
        if (enableVirtualPad && brakeButton != null)
        {
            if (brakeButton.Pressed)
            {
                return 1f;
            }
        }
        //Brake (not Handbrake)
        if (moveInput.y < 0 || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) //Combine Input System & Input Manager
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
        if (car != null) //Check
        {
            if (car is RealisticCarController realistic)
            {
                realistic.NOSInput = Input.GetKey(KeyCode.Space);  // Hoặc từ Input System
            }
            else if (car is ArcadeCarController arcade)
            {
                arcade.NOSInput = Input.GetKey(KeyCode.Space);  // Hoặc từ Input System
            }
        }
    }
    #endregion

    
}

