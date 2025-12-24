using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager<UIManager>
{
    [Header("UIMenu")]
    public HomeMenuPanel homeMenuPanel;

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
        if(currentObject != null)
        {
            currentObject.SetActive(false);
        }
        if(activeObject != null)
        {
            activeObject.SetActive(true);
        }
    }
    private void UpdateUIForScene(string sceneName)
    {
        //Disable all panels first
        ChangeUIGameObject(homeMenuPanel.gameObject);

        ChangeUIGameObject(hUDPanel.gameObject);
        ChangeUIGameObject(pausePanel.gameObject);
        ChangeUIGameObject(resultPanel.gameObject);

        switch (sceneName)
        {
            case "Garage":
                ChangeUIGameObject(null, homeMenuPanel.gameObject);
                break;
            case "R&D":
            case "Track1":
            case "Track2":
            case "Track3":
                ChangeUIGameObject(null, hUDPanel.gameObject);
                break;
        }
    }
}
