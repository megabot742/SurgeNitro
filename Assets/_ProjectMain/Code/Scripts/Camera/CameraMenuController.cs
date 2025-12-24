using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraMenuController : MonoBehaviour
{
    [SerializeField] private List<GameObject> panles;
    [SerializeField] private List<CinemachineCamera> cameras;

    [SerializeField] private int activePriority = 10;
    [SerializeField] private int inactivePriority = 0;

    void Update()
    {
        for (int i = 0; i < panles.Count; i++)
        {
            if(panles[i].activeSelf)
            {
                cameras[i].Priority = activePriority;
            }   
            else
            {
                cameras[i].Priority = inactivePriority;
            }
        }
    }
}
