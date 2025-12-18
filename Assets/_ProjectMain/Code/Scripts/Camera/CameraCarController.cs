using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Collections;
using UnityEngine;

public class CameraCarController : MonoBehaviour
{
    [Header("Setup Camera")]
    [SerializeField, ReadOnly] private int currentIndexCameraView = 0;
    [SerializeField] private int activePriority = 15; //Active
    [SerializeField] private int inactivePriority = 0; //Inactive

    [Header("CameraList")]
    [SerializeField, ReadOnly] private List<CinemachineCamera> cinemachineCameras = new();
    

    void Awake()
    {
        CollectCameras();
    }
    private void Start()
    {
        if (cinemachineCameras.Count > 0)
        {
            SetTarget();
            SetCameraView(currentIndexCameraView);
        }
    }
    private void CollectCameras()
    {
        cinemachineCameras.Clear();
        var allCameras = GetComponentsInChildren<CinemachineCamera>(true); //check and get all cameras
        cinemachineCameras.AddRange(allCameras); //add camera to list
    }
    private void SetTarget() //call in ManagerRace for tracking only player
    {
        foreach (CinemachineCamera camera in cinemachineCameras)
        {
            if(RaceManager.Instance)
            {
                camera.Follow = RaceManager.Instance.playerCarController.transform; //tracking player
            }
        }
    }
    public void SwitchCamera() //Call in Input System
    {
        if (cinemachineCameras.Count == 0) return;//check list
        
        currentIndexCameraView = (currentIndexCameraView + 1) % cinemachineCameras.Count;
        SetCameraView(currentIndexCameraView);
    }

    private void SetCameraView(int index)
    {
        if (cinemachineCameras.Count == 0) return;
        if (index < 0 || index >= cinemachineCameras.Count) index = 0;

        for (int i = 0; i < cinemachineCameras.Count; i++)
        {
            cinemachineCameras[i].Priority = (i == index) ? activePriority : inactivePriority;
        }

        currentIndexCameraView = index; //Update value
    }
}
