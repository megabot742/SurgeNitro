using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : BaseManager<CameraManager>
{
    [Header("Target")]
    [SerializeField] Transform menuTarget;
    [SerializeField] Transform raceTarget;

    [Header("Priorities")]
    public int menuPriorityActive = 10;
    public int racePriorityActive = 15;   // Cao hơn menu để race luôn override khi cần
    public int inactivePriority = 0;

    [Header("Cinemachine Brain")]
    [SerializeField] CinemachineBrain cinemachineBrain;
    [SerializeField] private float menuBlendTime = 1.0f;         // Thời gian blend khi ở menu (Garage)
    [SerializeField] private float raceBlendTime = 0.1f;

    [Header("Menu Cameras")]
    public CinemachineCamera screenHomeCamera;  // Chỉ 1 camera cho toàn bộ menu/Garage
    public CinemachineCamera screenCarInfo;
    public CinemachineCamera screenCarView;

    [Header("Race Cameras")]
    [SerializeField] private List<CinemachineCamera> raceCameras;
    private int currentRaceCameraIndex = 0;
    protected override void Awake()
    {
        base.Awake();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;  //Register callback
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  //Unsub to avoid leaks
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetAllStates();
        //SetTarget
        if (scene.name == "Garage") //Menu Scene Name
        {
            GameObject displayCar = GameObject.FindWithTag("DisplayCar");
            if (displayCar != null)
            {
                SetMenuCameraTarget(displayCar.transform);   
            }
            else
            {
                Debug.LogWarning("DisplayCar tag not found in Garage scene!");
            }
            UpdateBrainBlendTime(menuBlendTime);
            SwitchMenuCamera(MenuCameraType.Home);
        }
        else if (scene.name.StartsWith("Track") || scene.name == "R&D") //Race Scene Name
        {
            GameObject playerCar = GameObject.FindWithTag("Player");
            //Race target
            if (playerCar != null)
            {
                if (RaceManager.Instance.playerCarPrefab != null)
                {
                    //SetRaceTarget(RaceManager.Instance.playerCarController.transform);
                    SetRaceTarget(playerCar.transform);
                }
            }
            UpdateBrainBlendTime(raceBlendTime);
            //Disable race camera
            if (screenHomeCamera != null) screenHomeCamera.Priority = inactivePriority;
            if (screenCarInfo != null) screenCarInfo.Priority = inactivePriority;
            if (screenCarView != null) screenCarView.Priority = inactivePriority;
        }
    }
    private void ResetAllStates()
    {
        // Reset priorities và targets
        menuTarget = null;
        raceTarget = null;
        currentRaceCameraIndex = 0;

        if (screenHomeCamera != null) screenHomeCamera.Priority = inactivePriority;
        DisableAllRaceCameras();
    }
    private void UpdateBrainBlendTime(float blendTime)
    {
        if (cinemachineBrain == null) return;

        cinemachineBrain.DefaultBlend= new CinemachineBlendDefinition();
        cinemachineBrain.DefaultBlend.Time = blendTime;
        // Style giữ Linear hoặc EaseInOut tùy ý, mình recommend Linear cho race
        cinemachineBrain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Linear;
    }
    #region MenuCamera
    public void SetMenuCameraTarget(Transform target)
    {
    menuTarget = target;

    if (screenHomeCamera != null)
    {
        screenHomeCamera.Follow = target;
        screenHomeCamera.Priority = menuPriorityActive;
    }
}
    public void SwitchMenuCamera(MenuCameraType type)
    {
        // Set all menu cameras to inactive first
        if (screenHomeCamera != null) screenHomeCamera.Priority = inactivePriority;
        if (screenCarInfo != null) screenCarInfo.Priority = inactivePriority;
        if (screenCarView != null) screenCarView.Priority = inactivePriority;

        // Set active based on type
        switch (type)
        {
            case MenuCameraType.Home:
                if (screenHomeCamera != null)
                {
                    screenHomeCamera.Follow = menuTarget;
                    screenHomeCamera.Priority = menuPriorityActive;    
                }
                break;
            case MenuCameraType.CarInfo:
                if (screenCarInfo != null) 
                {
                    screenCarInfo.Follow = menuTarget;
                    screenCarInfo.Priority = menuPriorityActive;
                }   
                break;
            case MenuCameraType.CarView:
                if (screenCarView != null) 
                {
                    screenCarView.Follow = menuTarget;
                    screenCarView.Priority = menuPriorityActive;
                }
                break;
        }
    }
    #endregion
    #region RaceCamera
    public void SetRaceTarget(Transform playerCarTransform)
    {
        raceTarget = playerCarTransform;
        foreach (var cam in raceCameras)
        {
            if (cam != null)
            {
                cam.Follow = playerCarTransform;
            }
        }
        //Enable first camera (Action Camera)
        SwitchToRaceCamera(0);
    }

    public void SwitchRaceCamera() //SwitchCamera with button
    {
        if (raceCameras.Count == 0) return;

        currentRaceCameraIndex = (currentRaceCameraIndex + 1) % raceCameras.Count;
        SwitchToRaceCamera(currentRaceCameraIndex);
    }

    private void SwitchToRaceCamera(int index)
    {
        if (raceCameras.Count == 0) return;
        if (index < 0 || index >= raceCameras.Count) index = 0;

        for (int i = 0; i < raceCameras.Count; i++)
        {
            if (raceCameras[i] != null)
            {
                raceCameras[i].Priority = (i == index) ? racePriorityActive : inactivePriority;
            }
        }

        currentRaceCameraIndex = index;
    }
    #endregion
    #region DisableCam
    // Khi vào race scene: tắt hết menu camera để tránh conflict


    // Khi vào menu scene: tắt hết race camera
    public void DisableAllRaceCameras()
    {
        foreach (var cam in raceCameras)
        {
            if (cam != null) cam.Priority = inactivePriority;
        }
    }
    #endregion
}
