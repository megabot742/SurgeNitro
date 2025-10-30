using UnityEngine;
public enum HeadLightType
{
    TurnOff,
    LowBeam,
    HighBeam,
}

public enum BlinkerType
{
    TurnOff,
    Left,
    Right,
    Hazard,
}
[DisallowMultipleComponent]
public class CarLightManager : MonoBehaviour
{
    [SerializeField] private CarLight _headLight;
    [SerializeField] private CarLight _leftBlinker;
    [SerializeField] private CarLight _rightBlinker;
    [SerializeField] private CarLight _brakeLight;
    [SerializeField] private CarLight _backLight;

    private CarControllerBase _carController;

    private HeadLightType _headLightType = HeadLightType.TurnOff;
    private BlinkerType _blinkerType = BlinkerType.TurnOff;

    public CarLight HeadLight
    {
        get => _headLight;
        set => _headLight = value;
    }

    public CarLight LeftBlinker
    {
        get => _leftBlinker;
        set => _leftBlinker = value;
    }

    public CarLight RightBlinker
    {
        get => _rightBlinker;
        set => _rightBlinker = value;
    }

    public CarLight BrakeLight
    {
        get => _brakeLight;
        set => _brakeLight = value;
    }

    public CarLight BackLight
    {
        get => _backLight;
        set => _backLight = value;
    }

    public HeadLightType HeadLightType
    {
        get => _headLightType;
        set
        {
            if (_headLightType == value)
            {
                return;
            }

            _headLightType = value;

            switch (_headLightType)
            {
                case HeadLightType.TurnOff:
                    _headLight?.TurnOff();
                    _brakeLight?.TurnOff();
                    break;
                case HeadLightType.LowBeam:
                    _headLight?.TurnOn(0.5f);
                    _brakeLight?.TurnOn(0.5f);
                    break;
                case HeadLightType.HighBeam:
                    _headLight?.TurnOn(1f);
                    _brakeLight?.TurnOn(0.5f);
                    break;
            }
        }
    }

    public BlinkerType BlinkerType
    {
        get => _blinkerType;
        set
        {
            if (_blinkerType == value)
            {
                return;
            }

            _blinkerType = value;

            _leftBlinker?.TurnOff();
            _rightBlinker?.TurnOff();

            switch (_blinkerType)
            {
                case BlinkerType.Left:
                    _leftBlinker?.TurnOn(1f);
                    break;
                case BlinkerType.Right:
                    _rightBlinker?.TurnOn(1f);
                    break;
                case BlinkerType.Hazard:
                    _leftBlinker?.TurnOn(1f);
                    _rightBlinker?.TurnOn(1f);
                    break;
            }
        }
    }

    private void Awake()
    {
        _carController = GetComponentInParent<CarControllerBase>();
    }

    private void Update()
    {
        var reverse = _carController.Reverse;
        if (reverse)
        {
            _backLight?.TurnOn(1f);
        }
        else
        {
            _backLight?.TurnOff();
        }

        var brake = _carController.BrakeInput > 0f;
        if (brake)
        {
            _brakeLight?.TurnOn(1f);
        }
        else
        {
            if (_headLightType != HeadLightType.TurnOff)
            {
                _brakeLight?.TurnOn(0.5f);
            }
            else
            {
                _brakeLight?.TurnOff();
            }
        }
    }
}
