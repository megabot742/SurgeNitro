using UnityEngine;

public class CameraCarController : MonoBehaviour
{
    [SerializeField] int currentCameraView = 0;

    [SerializeField] private GameObject[] cameras; // Assume cameras are in order: cockpit, close, far, locked, wheel

    private void Start()
    {
        SetCameraView(currentCameraView);
    }

    void OnSwitchCamera() //Input System
    {
        currentCameraView = (currentCameraView + 1) % cameras.Length;
        SetCameraView(currentCameraView);
    }

    private void SetCameraView(int index)
    {
        if (cameras.Length == 0 || index < 0 || index >= cameras.Length) return;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras != null)
            {
                cameras[i].SetActive(false);
            }
        }
        GameObject activeCamera = cameras[index];
        if (activeCamera != null)
        {
            activeCamera.gameObject.SetActive(true);
        }
    }
}
