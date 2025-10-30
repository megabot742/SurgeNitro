using UnityEngine;
[System.Serializable]
public class Transmission
{
    public const int ReverseGear = 0;

        [SerializeField]
        private float[] _gearRatios = new float[]
        {
            -3.166f,
            2.916f,
            1.684f,
            1.115f,
            0.833f,
            0.666f,
        };
        [SerializeField, Min(0f)] private float _finalGearRatio = 4.933f;

        [SerializeField, Min(0.1f)] private float _shiftTime = 0.3f;

        [SerializeField] private bool _autoShift = true;
        [SerializeField, Min(0f)] private float _shiftUpRPM = 6250f;
        [SerializeField, Range(0f, 1f)] private float _throttleOnShiftDownCoef = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _throttleOffShiftDownCoef = 0.9f;

        private int _gear = 1;

        private float _remainShiftTime;

        private float _speedKPH;
        private float _wheelRadius;
        private float _throttleInput;

        public bool AutoShift
        {
            get => _autoShift;
            set => _autoShift = value;
        }

        public float CurrentTotalGearRatio => GetTotalGearRatio(_gear);

        public bool IsShiftingGear => _remainShiftTime > 0f;

        public float NormalizedRemainShiftTime => _remainShiftTime / _shiftTime;

        public int Gear
        {
            get => _gear;
            set => _gear = value;
        }

        public float[] GearRatios => _gearRatios;

        public float FinalGearRatio => _finalGearRatio;

        public float RemainShiftTime
        {
            get => _remainShiftTime;
            set => _remainShiftTime = value;
        }

        public void Update(float speedKPH, float wheelRadius, float throttleInput)
        {
            _speedKPH = speedKPH;
            _wheelRadius = wheelRadius;
            _throttleInput = throttleInput;

            AutoShiftGear();

            UpdateRemainShiftTime();
        }

        private void AutoShiftGear()
        {
            if (!_autoShift)
            {
                return;
            }

            if (_gear == ReverseGear)
            {
                return;
            }

            var nextGear = GetNextGear();
            ShiftGear(nextGear);
        }

        private int GetNextGear()
        {
            var shiftUpSpeed = GetShiftUpSpeedKPH();
            var shiftDownSpeed = GetShiftDownSpeedKPH();

            if (shiftUpSpeed >= 0f && _speedKPH > shiftUpSpeed)
            {
                return _gear + 1;
            }
            if (shiftDownSpeed >= 0f && _speedKPH < shiftDownSpeed)
            {
                return _gear - 1;
            }
            return _gear;
        }

        private float GetShiftUpSpeedKPH()
        {
            if (_gear >= _gearRatios.Length - 1)
            {
                return -1f;
            }

            var speedKPH = CarMath.EngineRPMToSpeedKPH(_shiftUpRPM, _gearRatios[_gear], _finalGearRatio, _wheelRadius);
            return speedKPH;
        }

        private float GetShiftDownSpeedKPH()
        {
            if (_gear <= 1)
            {
                return -1f;
            }

            var speedKPH = CarMath.EngineRPMToSpeedKPH(_shiftUpRPM, _gearRatios[_gear - 1], _finalGearRatio, _wheelRadius);
            speedKPH *= _throttleInput > 0f ? _throttleOnShiftDownCoef : _throttleOffShiftDownCoef;
            return speedKPH;
        }

        public void ShiftGear(int gear)
        {
            if (IsShiftingGear)
            {
                return;
            }

            var nextGear = Mathf.Clamp(gear, 0, _gearRatios.Length - 1);

            if (nextGear == _gear)
            {
                return;
            }

            _gear = nextGear;

            _remainShiftTime = _shiftTime;
        }

        public void ShiftUp()
        {
            ShiftGear(_gear + 1);
        }

        public void ShiftDonw()
        {
            ShiftGear(_gear - 1);
        }

        private void UpdateRemainShiftTime()
        {
            _remainShiftTime = Mathf.Max(_remainShiftTime - Time.fixedDeltaTime, 0f);
        }

        private float GetTotalGearRatio(int gear)
        {
            return _gearRatios[gear] * _finalGearRatio;
        }
}
