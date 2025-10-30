using UnityEngine;
[System.Serializable]
public class Nitro
{
    [SerializeField] private bool _install = false;

        [SerializeField, Min(1f)] private float _engineTorqueCoef = 1.5f;

        [SerializeField, Min(0f)] private float _maxTankCapacity = 30f;

        private float _remainTankCapacity;
        private bool _injection;

        public bool Install
        {
            get => _install;
            set => _install = value;
        }

        public bool Injection
        {
            get => _injection;
            set => _injection = value;
        }

        public float EngineTorqueCoef => _injection ? _engineTorqueCoef : 1f;

        public float RemainTankCapacity
        {
            get => _remainTankCapacity;
            set => _remainTankCapacity = value;
        }

        public void Init()
        {
            if (!_install)
            {
                return;
            }

            _remainTankCapacity = _maxTankCapacity;
        }

        public void Update(bool nosInput)
        {
            if (!_install)
            {
                _injection = false;
                return;
            }

            if (nosInput)
            {
                _remainTankCapacity = Mathf.Max(_remainTankCapacity - Time.deltaTime, 0f);
                _injection = _remainTankCapacity > 0f;
            }
            else
            {
                _injection = false;
            }
        }
}
