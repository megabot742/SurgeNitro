using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraMenuController : MonoBehaviour
{
    [SerializeField] private List<GameObject> panles;
    [SerializeField] private List<CinemachineCamera> cameras;

    //[SerializeField] private int activePriority = 10;
    //[SerializeField] private int inactivePriority = 0;

    void Update()
    {
        // int count = Mathf.Min(panles.Count, cameras.Count);

        // for (int i = 0; i < count; i++)
        // {
        //     // Kiểm tra null hoặc destroyed cho cả panel và camera
        //     if (panles[i] != null && cameras[i] != null)
        //     {
        //         if (panles[i].activeSelf)
        //         {
        //             cameras[i].Priority = activePriority;
        //         }
        //         else
        //         {
        //             cameras[i].Priority = inactivePriority;
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"CameraMenuController: Missing reference at index {i} " +
        //                          $"(Panel: {panles[i]}, Camera: {cameras[i]})");
        //     }
        // }

        // // Nếu có phần tử thừa (list không cùng kích thước), cảnh báo
        // if (panles.Count != cameras.Count)
        // {
        //     Debug.LogError($"CameraMenuController: Panels count ({panles.Count}) != Cameras count ({cameras.Count}). Please fix in Inspector!");
        // }
    }
}
