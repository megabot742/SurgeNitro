using UnityEngine;

[DisallowMultipleComponent]
public class Skidmark : WheelEffectBase
{
    [SerializeField] private Color _color = Color.black;
        [SerializeField, Range(0f, 1f)] private float _maxOpacity = 0.5f;

        private SkidmarkMesh _mesh;

        private int _lastIndex = -1;

        protected override void Awake()
        {
            base.Awake();

            _mesh = FindAnyObjectByType<SkidmarkMesh>();
        }

        protected override void Update()
        {
            base.Update();

            if (_mesh == null)
            {
                return;
            }

            var alpha = SlipAmount * _maxOpacity;

            if (alpha > 0f)
            {
                var color = _color;
                color.a = alpha;

                _lastIndex = _mesh.Leave(
                    Wheel.HitInfo.point,
                    Wheel.HitInfo.normal,
                    color,
                    Wheel.Width,
                    _lastIndex);
            }
            else
            {
                _lastIndex = -1;
            }
        }
}
