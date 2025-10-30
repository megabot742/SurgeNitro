using UnityEngine;
[DisallowMultipleComponent]
public class CarLight : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private Color _emissionColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float _maxEmissionIntensity = 1f;
    [SerializeField, Min(0f)] private float _maxLightIntensity = 1f;

    [SerializeField] private bool _blink = false;
    [SerializeField, Min(0f)] private float _blinkInterval = 1f;

    private Light[] _lights;

    private bool _on;
    private float _turnedOnTime;

    private float _intensity;

    public Renderer[] Renderers
    {
        get => _renderers;
        set => _renderers = value;
    }

    public Color EmissionColor
    {
        get => _emissionColor;
        set => _emissionColor = value;
    }

    public bool Blink
    {
        get => _blink;
        set => _blink = value;
    }

    public bool On => _on;

    private void Awake()
    {
        _lights = GetComponentsInChildren<Light>();

        TurnOff();
    }

    private void Update()
    {
        if (!_on)
        {
            return;
        }

        if (!_blink)
        {
            return;
        }

        var on = ((Time.time - _turnedOnTime) % _blinkInterval) <= (_blinkInterval / 2f);
        var intensity = on ? _intensity : 0f;
        UpdateIntensity(intensity);
    }

    public void TurnOn(float intensity = 1f)
    {
        if (_on && intensity == _intensity)
        {
            return;
        }

        _on = true;
        _turnedOnTime = Time.time;

        _intensity = intensity;
        UpdateIntensity(_intensity);
    }

    public void TurnOff()
    {
        _on = false;

        _intensity = 0f;
        UpdateIntensity(_intensity);
    }

    private void UpdateIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        foreach (var light in _lights)
        {
            if (intensity > 0f)
            {
                if (!light.enabled)
                {
                    light.enabled = true;
                }
                light.intensity = _intensity * _maxLightIntensity;
            }
            else
            {
                if (light.enabled)
                {
                    light.enabled = false;
                }
            }
        }

        foreach (var renderer in _renderers)
        {
            foreach (var mat in renderer.materials)
            {
                var emissonEnabled = mat.IsKeywordEnabled("_EMISSION");
                if (intensity > 0f)
                {
                    if (!emissonEnabled)
                    {
                        mat.EnableKeyword("_EMISSION");
                    }
                    mat.SetColor("_EmissionColor", _emissionColor * _intensity * _maxEmissionIntensity);
                }
                else
                {
                    if (emissonEnabled)
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }
}
