using UnityEngine;
[System.Serializable]
public class Turbocharger
{
    [SerializeField] private bool _install = false;

        [SerializeField, Min(0f)] private float _maxBoostPressure = 1f;

        [SerializeField] private float _throttleOffBoostPressure = -0.7f;

        [SerializeField, Min(0f)] private float _engineRPMAtBoostPressureMax = 4000f;
        
        [SerializeField, Range(0f, 1f)] private float _loss = 0.5f;

        [SerializeField, Min(0.001f)] private float _turbineAccelerationTime = 0.3f;
        [SerializeField, Min(0.001f)] private float _turbineDecelerationTime = 0.1f;

        private float _normalizedBoostPressure;

        public bool Install
        {
            get => _install;
            set => _install = value;
        }

        public float NormalizedBoostPressure
        {
            get => _normalizedBoostPressure;
            set => _normalizedBoostPressure = value;
        }

        public float BoostPressure => Mathf.Lerp(_throttleOffBoostPressure, _maxBoostPressure, _normalizedBoostPressure);

        public float EngineTorqueCoef => BoostPressure * (1f - _loss) + 1f;

        public void Update(float engineRPM, float throttleInput)
        {
            if (!_install)
            {
                _normalizedBoostPressure = 0f;
                return;
            }

            var targetNormBoostPressure = Mathf.Clamp01(engineRPM / _engineRPMAtBoostPressureMax) * throttleInput;

            if (throttleInput > 0f)
            {
                _normalizedBoostPressure = Mathf.MoveTowards(
                    _normalizedBoostPressure,
                    targetNormBoostPressure,
                    Time.deltaTime / _turbineAccelerationTime);
            }
            else
            {
                _normalizedBoostPressure = Mathf.MoveTowards(
                    _normalizedBoostPressure,
                    targetNormBoostPressure,
                    Time.deltaTime / _turbineDecelerationTime);
            }
        }
}
