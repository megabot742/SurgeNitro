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

    [Header("Menu Cameras")]
    [SerializeField] private List<GameObject> menuPanels;          
    [SerializeField] private List<CinemachineCamera> menuCameras;          

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
        //SetTarget
        if (scene.name == "Garage") //Menu Scene Name
        {
            GameObject displayCar = GameObject.FindWithTag("DisplayCar");
            if (displayCar != null)
            {
                //Menu target
                SetMenuCameraTarget(displayCar.transform);
                DisableAllRaceCameras();  //Disable Race Camera
            }
            else
            {
                Debug.LogWarning("DisplayCar tag not found in Garage scene!");
            }
        }
        else if (scene.name.StartsWith("Track") || scene.name == "R&D") //Race Scene Name
        {
            //Race target
            if (RaceManager.Instance.playerCarPrefab!= null)
            {
                SetRaceTarget(RaceManager.Instance.playerCarController.transform);
                DisableAllMenuCameras(); //Disable Menu Camera
            }
        }
    }

    void Update()
    {
        UpdateMenuCameras();
        if (menuTarget != null)
        {
            foreach (var cam in menuCameras)
            {
                if (cam != null)
                {
                    cam.Follow = menuTarget;
                }
            }
        }
    }
    #region MenuCamera
    private void UpdateMenuCameras()
    {
        int count = Mathf.Min(menuPanels.Count, menuCameras.Count);

        for (int i = 0; i < count; i++)
        {
            if (menuPanels[i] != null && menuCameras[i] != null)
            {
                bool panelActive = menuPanels[i].activeSelf;
                menuCameras[i].Priority = panelActive ? menuPriorityActive : inactivePriority;
            }
        }
    }
    public void SetMenuCameraTarget(Transform target)
    {
        menuTarget = target;
        //Debug.Log("CameraManager: Menu target set to " + (target ? target.name : "null"));
        // Set target
        foreach (var cam in menuCameras)
        {
            if (cam != null)
            {
                cam.Follow = menuTarget;
            }
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
    public void DisableAllMenuCameras()
    {
        foreach (var cam in menuCameras)
        {
            if (cam != null) cam.Priority = inactivePriority;
        }
    }

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
