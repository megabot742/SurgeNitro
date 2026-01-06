using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CarControllerBase))]
public class SpeedUpShader : MonoBehaviour
{
    [Header("Car Reference")]
    [SerializeField, ReadOnly] private CarControllerBase carControllerBase;

    [Header("Effect Control")]
    [SerializeField] private bool isEffectEnabled = true;

    [Header("Renderer Feature (Performance)")]
    [SerializeField, ReadOnly] private FullScreenPassRendererFeature screenFeature;  // RenderFeature của Screen Shader

    [Header("Shader Settings")]
    [SerializeField] private Material targetMaterial;  // Material của Screen Shader
    [SerializeField] private float minApha = 0; //default 0
    [SerializeField] private float maxApha = 2; //default 2
    private const string ALPHA_PROPERTY_NAME = "_Alpha";
    int alphaPropertyID;

    [Header("Speed Threshold")]
    [SerializeField, Range(0.5f, 0.8f)] private float speedThresholdPercent = 0.6f;  //Start with 60%

    [Header("Straight Driving Check")]
    [SerializeField, Range(1f, 15f)] private float minSlipAngleDegrees = 8f;
    [SerializeField, Range(1f, 25f)] private float maxSlipAngleDegrees = 20f;

    [Header("Fade Speed")]
    [SerializeField, Range(1f, 20f)] private float fadeSpeed = 2f;  //Speed fade in/out

    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private bool featureCached = false;
    #region Setup
    private void Awake()
    {
        //Get carControllerBase
        if (carControllerBase == null)
        {
            carControllerBase = GetComponentInParent<CarControllerBase>();
        }
        //Get RenderFeature
        if (screenFeature == null && RaceManager.HasInstance)
        {
            screenFeature = RaceManager.Instance.screenFeature;
        }
        featureCached = screenFeature != null;
        //Cache ID alpha
        alphaPropertyID = Shader.PropertyToID(ALPHA_PROPERTY_NAME);
        if (targetMaterial != null && !targetMaterial.HasFloat(alphaPropertyID))
        {
            Debug.LogError("Material don't have propertt. Check shader graph.");
        }
        // Reset alpha
        currentAlpha = 0f;
        UpdateAlpha();
        SetFeatureEnabled(false);
    }
    #endregion
    #region Handle Shader
    private void Update()
    {
        if (!isEffectEnabled || carControllerBase == null) return;

        // Check speed parameters
        float speedPercent = carControllerBase.ForwardSpeedPercent;
        float dynamicMaxSlip = Mathf.Lerp(maxSlipAngleDegrees, minSlipAngleDegrees, speedPercent);

        bool isSpeedUpCondition = (speedPercent >= speedThresholdPercent) && Mathf.Abs(carControllerBase.SlipAngle) <= dynamicMaxSlip;

        // Calculate target alpha based on speed (0% → 100% = 0 -> 1) 
        if (isSpeedUpCondition)
        {
            float target = Mathf.InverseLerp(speedThresholdPercent, 1f, speedPercent);
            targetAlpha = Mathf.Lerp(minApha, maxApha, target);  // Fade from 0 → 1
        }
        else
        {
            targetAlpha = minApha;  //Hide shader
        }

        // Smooth lerp alpha
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Disable feature when alpha = 0 (increase performance)
        bool shouldEnableFeature = currentAlpha > 0.01f;  // Small Threshold to avoid continuous toggle

        SetFeatureEnabled(shouldEnableFeature);

        // Update shader alpha
        UpdateAlpha();
    }

    private void UpdateAlpha()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(alphaPropertyID, currentAlpha);
        }
    }

    private void SetFeatureEnabled(bool enabled)
    {
        if (!featureCached || screenFeature == null) return;

        screenFeature.SetActive(enabled);
    }
    #endregion
    #region Reset Shader
    private void ResetShaderState()
    {
        currentAlpha = 0;
        UpdateAlpha();
        SetFeatureEnabled(false);
    }
    void OnDisable() //EX: Call when quit race, change scene...
    {
        ResetShaderState();
    }
    void OnDestroy() //EX: Call when car crash or stop like (ALT + F4)
    {
        ResetShaderState();
    }
    #endregion
}

