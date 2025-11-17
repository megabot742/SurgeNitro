using UnityEngine;
[RequireComponent(typeof(CarControllerBase))]
public class CarStatsProvider : MonoBehaviour
{
    [SerializeField, ReadOnly] private CarControllerBase carController;  // Reference đến base controller
    private void Awake()
    {
        // Cache references để tránh GetComponent mỗi frame
        carController = GetComponent<CarControllerBase>();
        if (carController == null)
        {
            Debug.LogError("CarStatsProvider requires CarControllerBase on the same GameObject.");
            enabled = false;
        }
    }
    // Properties để expose thông số (read-only để an toàn)
    public float MaxSpeedKPH => carController.MaxSpeedKPH;  // Max speed(KPH)
    public float ForwardSpeedKPH => carController.ForwardSpeedKPH;  //Current forward speed(KPH)
    public float ForwardSpeedPercent => Mathf.Clamp01(ForwardSpeedKPH / MaxSpeedKPH);  //Clampe percent for only use forwardSpeed >0, to avoid backward
    public float SlipAngle => carController.SlipAngle; //SlipAngle
}
