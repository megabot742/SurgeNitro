using UnityEngine;
using UnityEngine.InputSystem;

public class InputCarController : DriverBase
{
    [SerializeField, Min(0.001f)] private float _steerTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _steerReleaseTime = 0.1f;

    [SerializeField, Min(0.001f)] private float _throttleTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _throttleReleaseTime = 0.1f;

    [SerializeField, Min(0.001f)] private float _brakeTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _brakeReleaseTime = 0.1f;

    [SerializeField] private bool _steerLimitByFriction = false;
    [SerializeField, Min(0f)] private float _steerMu = 2f;

    [SerializeField] private bool _autoShiftToReverse = true;
    [SerializeField, Min(0f)] private float _switchToReverseSpeedKPH = 1f;

    [SerializeField] private VirtualPadButton _leftSteerButton;
    [SerializeField] private VirtualPadButton _rightSteerButton;
    [SerializeField] private VirtualPadButton _throttleButton;
    [SerializeField] private VirtualPadButton _brakeButton;

    [SerializeField] private bool _enableVirtualPad = true;

    [SerializeField] Vector2 moveInput;
    [SerializeField] bool handbrakeInputAction;
    [SerializeField] float turnNitro;

    public bool SteerLimitByFriction
    {
        get => _steerLimitByFriction;
        set => _steerLimitByFriction = value;
    }

    public bool AutoSwitchToReverse
    {
        get => _autoShiftToReverse;
        set => _autoShiftToReverse = value;
    }

    public bool EnableVirtualPad
    {
        get => _enableVirtualPad;
        set => _enableVirtualPad = value;
    }
    //New Input System
    void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }
    void OnNitro()
    {
        turnNitro++;
    }
    void OnHandbrake(InputValue inputValue)
    {
        handbrakeInputAction = inputValue.isPressed;
    }
    protected override void Drive()
    {
        UpdateSteerInput();
        UpdateThrottleAndBrakeInput();

        ShiftChange();

        UpdateClutchInput();
        UpdateNOSInput();
    }
    // void Update()
    // {
    //     Debug.Log(handbrakeInput);     
    // }
    protected override void Stop()
    {
        carController.BrakeInput = 1f;

        var engineCar = carController as RealisticCarController;
        if (engineCar != null)
        {
            engineCar.ClutchInput = true;
        }

        var throttleInput = GetRawThrottleInput();

        var throttleTime = throttleInput != 0f ? _throttleTime : _throttleReleaseTime;
        carController.ThrottleInput = Mathf.MoveTowards(carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);
    }

    private float GetRawSteerInput()
    {
        if (_enableVirtualPad && _leftSteerButton != null && _rightSteerButton != null)
        {
            if (_leftSteerButton.Pressed)
            {
                return -1f;
            }
            if (_rightSteerButton.Pressed)
            {
                return 1f;
            }
        }

        //if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        if (moveInput.x < 0)
        {
            return -1f;
        }
        //if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        if (moveInput.x > 0)
        {
            return 1f;
        }

        return 0f;
    }

    private float GetRawThrottleInput()
    {
        if (_enableVirtualPad && _throttleButton != null)
        {
            if (_throttleButton.Pressed)
            {
                return 1f;
            }
        }

        //if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.C))
        if (moveInput.y > 0)
        {
            return 1f;
        }

        return 0f;
    }

    private float GetRawBrakeInput()
    {
        if (_enableVirtualPad && _brakeButton != null)
        {
            if (_brakeButton.Pressed)
            {
                return 1f;
            }
        }

        //if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.X))
        if (moveInput.y < 0)
        {
            return 1f;
        }

        return 0f;
    }

    private void UpdateSteerInput()
    {
        var maxSteerInput = 1f;
        if (_steerLimitByFriction)
        {
            var speed = carController.Speed;
            var minTurnR = (speed * speed) / (_steerMu * UnityEngine.Physics.gravity.magnitude);
            if (minTurnR > 0f)
            {
                var optimalSteerAngle = Mathf.Asin(carController.Wheelbase / minTurnR) * Mathf.Rad2Deg;
                maxSteerInput = Mathf.Min(optimalSteerAngle / carController.MaxSteerAngle, 1f);
            }
        }

        var steerInput = GetRawSteerInput();

        steerInput = Mathf.Clamp(steerInput, -maxSteerInput, maxSteerInput);

        var steerTime = steerInput != 0f ? _steerTime : _steerReleaseTime;

        if (steerInput != 0f && Mathf.Sign(steerInput) != Mathf.Sign(carController.SteerInput))
        {
            carController.SteerInput = 0f;
        }

        carController.SteerInput = Mathf.MoveTowards(carController.SteerInput, steerInput, Time.deltaTime / steerTime);
    }

    private void UpdateThrottleAndBrakeInput()
    {
        var throttleInput = GetRawThrottleInput();

        var brakeInput = GetRawBrakeInput();

        if (_autoShiftToReverse)
        {
            if (carController.IsGrounded())
            {
                var speedKPH = carController.ForwardSpeed * CarMath.MPSToKPH;
                if (carController.Reverse)
                {
                    if (throttleInput > 0f && speedKPH > -_switchToReverseSpeedKPH)
                    {
                        carController.Reverse = false;
                    }
                }
                else
                {
                    if (brakeInput > 0f && speedKPH < _switchToReverseSpeedKPH)
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

        var throttleTime = throttleInput != 0f ? _throttleTime : _throttleReleaseTime;
        carController.ThrottleInput = Mathf.MoveTowards(carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);

        var brakeTime = brakeInput != 0f ? _brakeTime : _brakeReleaseTime;
        carController.BrakeInput = Mathf.MoveTowards(carController.BrakeInput, brakeInput, Time.deltaTime / brakeTime);

        //Input Manager
        // var handbrakeInput = Input.GetKey(KeyCode.LeftShift);
        // _carController.HandbrakeInput = handbrakeInput;
        //Input Action
        carController.HandbrakeInput = handbrakeInputAction;


    }
    //ENGINE CAR
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
            if (!_autoShiftToReverse
                || (_autoShiftToReverse && engineCar.Transmission.Gear != Transmission.ReverseGear))
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

    private void UpdateNOSInput()
    {
        var car = carController;  // Sử dụng base CarControllerBase
        if (car != null)
        {
            // NEW: Set NOSInput cho bất kỳ controller nào có property này (sử dụng reflection hoặc kiểm tra type)
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
}
