using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
[DisallowMultipleComponent]
public class WheelSmoke : WheelEffectBase
{
    private ParticleSystem _particle;

        private float _defaultRateOverTimeConstant;

        protected override void Awake()
        {
            base.Awake();

            _particle = GetComponent<ParticleSystem>();

            _defaultRateOverTimeConstant = _particle.emission.rateOverTime.constant;

            SetRateOverTimeConstant(0f);
        }

        protected override void Update()
        {
            base.Update();

            var constant = _defaultRateOverTimeConstant * SlipAmount;
            SetRateOverTimeConstant(constant);
        }

        private void SetRateOverTimeConstant(float constant)
        {
            var e = _particle.emission;
            var rot = e.rateOverTime;
            rot.constant = constant;
            e.rateOverTime = rot;
        }
}
