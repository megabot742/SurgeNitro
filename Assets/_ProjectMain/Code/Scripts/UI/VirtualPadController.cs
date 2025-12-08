using UnityEngine;

public class VirtualPadController : MonoBehaviour
{
    public static VirtualPadController Instance { get; private set; }

    [Header("Buttons")]
    public VirtualPadButton leftSteer;
    public VirtualPadButton rightSteer;
    public VirtualPadButton throttle;
    public VirtualPadButton brake;
    // Thêm sau này: nitro, handbrake, cameraSwitch...

    [Header("Settings")]
    public bool enableVirtualPad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject); // Nếu cần giữ qua scene
    }

    // Public getters để bên ngoài chỉ đọc, không sửa trực tiếp
    public bool LeftSteerPressed  => enableVirtualPad && leftSteer  != null && leftSteer.Pressed;
    public bool RightSteerPressed => enableVirtualPad && rightSteer != null && rightSteer.Pressed;
    public bool ThrottlePressed   => enableVirtualPad && throttle   != null && throttle.Pressed;
    public bool BrakePressed      => enableVirtualPad && brake      != null && brake.Pressed;
}
