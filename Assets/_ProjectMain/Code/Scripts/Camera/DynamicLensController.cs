using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class DynamicLensController : MonoBehaviour
{
    [Header("Lens Settings")]
    [SerializeField, Range(40f, 80f)] private float minFieldOfView = 60f;  // FOV default
    [SerializeField, Range(40f, 100f)] private float maxFieldOfView = 80f;  // FOV max

    [Header("Speed Threshold")]
    [SerializeField, Range(0.5f, 0.8f)] private float speedThresholdPercent = 0.6f;  //Lerp form 60% maxSpeed

    [Header("Straight Driving Check")]
    [SerializeField, Range(1f, 15f)] private float minSlipAngleDegrees = 8f;
    [SerializeField, Range(1f, 25f)] private float maxSlipAngleDegrees = 20f;  // SlipAngle check

    [Header("Lerp Speed")]
    [SerializeField, Range(1f, 20f)] private float fovLerpSpeed = 2f;  //Speed Lerp FOV (high = fast, low = smooth)

    [Header("Car Reference")]
    [SerializeField] private CarStatsProvider carStatsProvider;
    private CinemachineCamera cinemachineCamera;
    private float targetFOV;

    private void Awake()
    {
        //CinemachineCamera
        cinemachineCamera = GetComponent<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            Debug.LogWarning("Can't find CinemachineCamera");
            enabled = false;
            return;
        }

        //CarStatsProvider
        if (carStatsProvider == null)
        {
            carStatsProvider = GetComponentInParent<CarStatsProvider>();
            if (carStatsProvider != null)
            {
                Debug.Log("CarController: " + carStatsProvider.name);
            }
            else
            {
                Debug.LogError("Can't find CarController");
                enabled = false;
                return;
            }
        }
        //Set targetFOV = minFOV
        targetFOV = minFieldOfView;
    }
    
    private void LateUpdate()
    {
        if (cinemachineCamera == null || carStatsProvider == null) return;

        //Get forwardSpeed KPH
        float forwardSpeedKPH = carStatsProvider.ForwardSpeedKPH;
        //Debug.Log(forwardSpeedKPH);

        //value % (speed/max sppeed)
        float speedPercent = carStatsProvider.ForwardSpeedPercent;
        float dynamicMaxSlip = Mathf.Lerp(maxSlipAngleDegrees, minSlipAngleDegrees, speedPercent);  // Speed fast = slip small

        // Check "run straight" with forwardSpeed and slipAngle 
        bool isDrivingStraight = (speedPercent >= speedThresholdPercent) && Mathf.Abs(carStatsProvider.SlipAngle) <= dynamicMaxSlip;

        if (isDrivingStraight)
        {
            //Increase Lerp form minFOV -> to maxFOV (speed% - threshold) / (1 - threshold)
            float t = Mathf.InverseLerp(speedThresholdPercent, 1f, speedPercent);
            targetFOV = Mathf.Lerp(minFieldOfView, maxFieldOfView, t);
        }
        else
        {
            //Reduce to minFOV
            targetFOV = minFieldOfView;
        }

        // Smooth Lerp FOV current to target
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(cinemachineCamera.Lens.FieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }
}
