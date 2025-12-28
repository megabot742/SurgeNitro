using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager<UIManager>
{   

    [Header("Other")]
    public ScreenLoadingPanel screenLoadingPanel;

    [Header("UIMenu")]
    public ResourcePanel resourcePanel;
    public HomeMenuPanel homeMenuPanel;
    public GaragePanel garagePanel;
    public RaceSetupPanel raceSetupPanel;
    public ShopPanel shopPanel;
    public SettingPanel settingPanel;

    [Header("UICar")]
    public CarInfoPanel carInfoPanel;
    public CarUpgradePanel carUpgradePanel;
    public CarViewPanel carViewPanel;

    [Header("UIRace")]
    public HUDPanel hUDPanel;
    public PausePanel pausePanel;
    public ResultPanel resultPanel;

    [Header("EndingRaceTime")]
    public float timeIfNotFinish = 10f; //default 10s
    public bool isCountingDown; //false
    private float endCountDown;

    [Header("Scene Name")]
    public string currentSceneName;



    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        SwitchToScene("Garage");
    }
    void Update()
    {
        if (isCountingDown && endCountDown > 0)
        {
            endCountDown -= Time.deltaTime;
            if (endCountDown <= 0)
            {
                isCountingDown = false;
            }
        }
    }
    public void SetEndCountDown(float value) //reset value
    {
        if (value >= 0)
        {
            endCountDown = value;
            isCountingDown = true;
        }
    }

    public float GetEndCountDown()
    {
        return endCountDown;
    }

    public void StopCountdown()
    {
        isCountingDown = false;
    }
    public void ReloadCurrentScene()
    {
        //check currentSceneName
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            SwitchToScene(currentSceneName);
        }
    }
    public void SwitchToScene(string sceneName)
    {
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;

        // Update UI based on the loaded scene
        UpdateUIForScene(sceneName);

        // if (BackgroundMusic.HasInstance)
        // {
        //     // Cập nhật nhạc nền cho scene mới
        //     BackgroundMusic.Instance.UpdateMusicForScene(sceneName);
        // }
    }
    public void ChangeUIGameObject(GameObject currentObject = null, GameObject activeObject = null)
    {
        if (currentObject != null)
        {
            currentObject.SetActive(false);
        }
        if (activeObject != null)
        {
            activeObject.SetActive(true);
        }
    }
    private void UpdateUIForScene(string sceneName)
    {
        //Disable all panels first
        //Other
        ChangeUIGameObject(screenLoadingPanel.gameObject);
        //Menu
        ChangeUIGameObject(resourcePanel.gameObject);
        ChangeUIGameObject(homeMenuPanel.gameObject);
        ChangeUIGameObject(garagePanel.gameObject);
        ChangeUIGameObject(raceSetupPanel.gameObject);
        ChangeUIGameObject(settingPanel.gameObject);
        ChangeUIGameObject(shopPanel.gameObject);
        //CarUpgrade
        ChangeUIGameObject(carInfoPanel.gameObject);
        ChangeUIGameObject(carUpgradePanel.gameObject);
        ChangeUIGameObject(carViewPanel.gameObject);
        //InRace
        ChangeUIGameObject(hUDPanel.gameObject);
        ChangeUIGameObject(pausePanel.gameObject);
        ChangeUIGameObject(resultPanel.gameObject);
        if (CameraManager.HasInstance)
        {
            GameObject initialPanel = null;
            switch (sceneName)
            {
                case "Garage":
                    ChangeUIGameObject(null, homeMenuPanel.gameObject);
                    initialPanel = homeMenuPanel.gameObject;
                    break;
                case "R&D":
                case "Track1":
                case "Track2":
                case "Track3":
                    initialPanel = hUDPanel.gameObject;
                    ChangeUIGameObject(null, hUDPanel.gameObject);
                    break;
            }
            if (UIEventManager.HasInstance && initialPanel != null)
            {
                UIEventManager.Instance.SetCurrentActivePanel(initialPanel);
            }
        }
    }
}
