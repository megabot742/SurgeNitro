using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CinemachineCamera))]
public class DynamicLensController : MonoBehaviour
{
    [Header("Car Reference")]
    [SerializeField] private CarControllerBase carControllerBase;

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
        //Set targetFOV = minFOV
        targetFOV = minFieldOfView;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; //Subscribe
        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; //Unsubscribe
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Track") || scene.name == "R&D")  // Race scene
        {
            if (RaceManager.HasInstance)
            {
                enabled = true;  // Enable script in race scenes
                StartCoroutine(WaitForPlayerCar());  // Wait for spawn
            }
        }
        else  //Disable script in scenes that are not needed
        {
            carControllerBase = null;
            enabled = false;  
        }
    }
    IEnumerator WaitForPlayerCar()
    {
        while (carControllerBase == null)
        {
            if (RaceManager.HasInstance)
            {
                carControllerBase = RaceManager.Instance.playerCarController;
            }
            if (carControllerBase != null)
            {
                Debug.Log("DynamicLens: Found playerCarController");
                yield break;
            }
            yield return new WaitForSeconds(1f);  // Poll every 0.1s
        }
    }
    private void LateUpdate()
    {
        if (cinemachineCamera == null || carControllerBase == null) return;

        //value % (speed/max sppeed)
        float speedPercent = carControllerBase.ForwardSpeedPercent;
        float dynamicMaxSlip = Mathf.Lerp(maxSlipAngleDegrees, minSlipAngleDegrees, speedPercent);  // Speed fast = slip small

        // Check "run straight" with forwardSpeed and slipAngle 
        bool isDrivingStraight = (speedPercent >= speedThresholdPercent) && Mathf.Abs(carControllerBase.SlipAngle) <= dynamicMaxSlip;

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
