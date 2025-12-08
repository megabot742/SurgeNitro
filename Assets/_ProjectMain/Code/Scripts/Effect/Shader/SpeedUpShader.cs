using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CarStatsProvider))]
public class SpeedUpShader : MonoBehaviour
{
    [Header("Car Reference")]
    [SerializeField, ReadOnly] private CarStatsProvider carStatsProvider;
    [Header("CheckPlayer")]
    

    [Header("Shader Settings")]
    [SerializeField] private Material targetMaterial;  // Material của Screen Shader (kéo vào Inspector)
    [SerializeField] private string alphaPropertyName = "_Alpha";  // Tên property alpha trong shader
    [SerializeField] private float minApha = 0; //default 0
    [SerializeField] private float maxApha = 2; //default 2

    [Header("Speed Threshold")]
    [SerializeField, Range(0.5f, 0.8f)] private float speedThresholdPercent = 0.6f;  // Bắt đầu fade từ 60%

    [Header("Straight Driving Check")]
    [SerializeField, Range(1f, 15f)] private float minSlipAngleDegrees = 8f;
    [SerializeField, Range(1f, 25f)] private float maxSlipAngleDegrees = 20f;

    [Header("Fade Speed")]
    [SerializeField, Range(1f, 20f)] private float fadeSpeed = 2f;  // Tốc độ fade in/out

    [Header("Renderer Feature (Performance)")]
    
    [SerializeField, ReadOnly] private FullScreenPassRendererFeature screenFeature;  // RenderFeature của Screen Shader

    InputCarController inputCarController;

    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private bool featureCached = false;

    private void Awake()
    {
        inputCarController = GetComponent<InputCarController>();
        if(inputCarController.isAICar) //true
        {
            enabled = false;
            return;
        }
        // Cache CarStatsProvider
        if (carStatsProvider == null)
        {
            carStatsProvider = GetComponentInParent<CarStatsProvider>();
        }

        // Cache RenderFeature (1 lần duy nhất, zero GC)
        CacheRenderFeature();

        // Reset alpha
        currentAlpha = 0f;
        UpdateAlpha();
        SetFeatureEnabled(false);
    }

    private void CacheRenderFeature()
    {
        if(RaceManager.HasInstance)
        {
            screenFeature = RaceManager.Instance.screenFeature;
        }
    }

    private void Update()
    {
        if (carStatsProvider == null) return;

        // Logic giống DynamicLensController
        float speedPercent = carStatsProvider.ForwardSpeedPercent;
        float dynamicMaxSlip = Mathf.Lerp(maxSlipAngleDegrees, minSlipAngleDegrees, speedPercent);

        bool isSpeedUpCondition = (speedPercent >= speedThresholdPercent) && 
                                 Mathf.Abs(carStatsProvider.SlipAngle) <= dynamicMaxSlip;

        // Tính target alpha dựa trên speed (0 → 1)
        if (isSpeedUpCondition)
        {
            float t = Mathf.InverseLerp(speedThresholdPercent, 1f, speedPercent);
            targetAlpha = Mathf.Lerp(minApha, maxApha, t);  // Fade từ 0 → 1 theo speed
        }
        else
        {
            targetAlpha = minApha;  // Ẩn shader
        }

        // Smooth lerp alpha
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // PERFORMANCE: Disable feature khi alpha = 0 (tiết kiệm GPU)
        bool shouldEnableFeature = currentAlpha > 0.01f;  // Threshold nhỏ để tránh toggle liên tục

        SetFeatureEnabled(shouldEnableFeature);

        // Update shader alpha
        UpdateAlpha();
    }

    private void UpdateAlpha()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(alphaPropertyName, currentAlpha);
        }
    }

    private void SetFeatureEnabled(bool enabled)
    {
        if (!featureCached || screenFeature == null) return;

        screenFeature.SetActive(enabled);
    }
}
