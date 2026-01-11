using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class UIEventManager : BaseManager<UIEventManager>
{
    [Header("Scene Name")]
    public string currentSceneName;

    [Header("Pause")]
    public bool isPaused = false;

    [Header ("Toggle button index")]
    public int currentFilterIndexGarage = 0; //default 0
    public int currentFilterIndexShop = 0; //default 0
    private Stack<Type> screenHistory = new Stack<Type>();
    private Dictionary<Type, Action<object>> showScreenMap = new Dictionary<Type, Action<object>>();
    protected override void Awake()
    {
        base.Awake();
        currentSceneName = "Garage";

        showScreenMap.Add(typeof(ScreenHome), data => UIManager.Instance.ShowScreen<ScreenHome>(data));
        showScreenMap.Add(typeof(ScreenGarage), data => UIManager.Instance.ShowScreen<ScreenGarage>(data));
        showScreenMap.Add(typeof(ScreenShop), data => UIManager.Instance.ShowScreen<ScreenShop>(data));
        showScreenMap.Add(typeof(ScreenRaceSetup), data => UIManager.Instance.ShowScreen<ScreenRaceSetup>(data));
        showScreenMap.Add(typeof(ScreenCarInfo), data => UIManager.Instance.ShowScreen<ScreenCarInfo>(data));
        showScreenMap.Add(typeof(ScreenCarView), data => UIManager.Instance.ShowScreen<ScreenCarView>());
        // _showScreenMap.Add(typeof(ScreenCarUpgrade), data => UIManager.Instance.ShowScreen<ScreenCarUpgrade>(data));
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        if (UIManager.HasInstance)
        {
            if (scene.name == "Garage")
            {
                UIManager.Instance.ShowPopup<PopupCurrency>();
                UIManager.Instance.ShowScreen<ScreenHome>();
            }
            else if (scene.name == "R&D")
            {
                UIManager.Instance.HideAllPopups();
                UIManager.Instance.ShowScreen<ScreenGame>();
            }
        }
    }
    
    #region Scene Handler
    public void ShowScreenWithHistory<T>(object data = null) where T : BaseScreen
    {
        if (!UIManager.HasInstance) return;

        // Lưu current screen Type
        Type currentType = GetCurrentScreenType();
        if (currentType != null)
        {
            screenHistory.Push(currentType);
        }

        // Show screen mới
        UIManager.Instance.ShowScreen<T>(data);
    }

    public void GoBackMultiple(int steps = 1, object data = null)
    {
        if (steps <= 0 || screenHistory.Count == 0)
        {
            // Fallback về Home
            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowScreen<ScreenHome>();
            }
            return;
        }

        int actualSteps = Mathf.Min(steps, screenHistory.Count);

        // Pop intermediate
        for (int i = 1; i < actualSteps; i++)
        {
            screenHistory.Pop();
        }

        // Pop và show target screen
        Type targetType = screenHistory.Pop();
        if (showScreenMap.TryGetValue(targetType, out var showAction))
        {
            showAction(data);
        }
        else
        {
            Debug.LogError("UIEventManager: No map for type " + targetType.Name);
        }
    }

    public void GoBack(object data = null)
    {
        GoBackMultiple(1, data);
    }

    // Chuyển GetCurrentScreenType từ UIManager sang đây (không LINQ, dùng loop)
    private Type GetCurrentScreenType()
    {
        Dictionary<string, BaseScreen> screens = UIManager.Instance.Screens; // Giả sử Screens là public hoặc thêm getter

        foreach (KeyValuePair<string, BaseScreen> kvp in screens)
        {
            BaseScreen screen = kvp.Value;
            if (screen != null && !screen.GetIsHide)
            {
                return screen.GetType();
            }
        }
        return null;
    }
    public void ReloadCurrentScene() //Restart
    {
        //check currentSceneName
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            SwitchToScene(currentSceneName);
        }
        else
        {
            Debug.LogWarning("No scene name cached");
        }
    }
    public void SwitchToScene(string sceneName) //Change Scene
    {
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;

        // if (BackgroundMusic.HasInstance)
        // {
        //     // Cập nhật nhạc nền cho scene mới
        //     BackgroundMusic.Instance.UpdateMusicForScene(sceneName);
        // }
    }
     public void LoadSceneWithLoading(string sceneName)
    {
        if (UIManager.HasInstance)
        {
            // Show notify loading với data là sceneName
            UIManager.Instance.ShowNotify<NotifyLoadingGame>(data: sceneName);
        }
    }
    #endregion
    
    #region Button Handler
    private void Update()
    {
        PauseSetup();
    }
    private void PauseSetup()
    {
        if (!currentSceneName.StartsWith("Track") && currentSceneName != "R&D")
        {
            return;  //Only run garage scene
        }
        if (UIManager.HasInstance)
        {
            var screenGame = UIManager.Instance.GetExistScreen<ScreenGame>();
            if (screenGame != null && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

    }
    public void PlayBtn()
    {
        if (UIManager.HasInstance)
        {
            ShowScreenWithHistory<ScreenRaceSetup>();
        }
    }
    public void GarageBtn(object data = null) //null = View
    {
        if (UIManager.HasInstance)
        {
            if (data == null)
            {
                data = new CarInfoData { Mode = CarInfoMode.View };
            }
            currentFilterIndexGarage = 0;
            ShowScreenWithHistory<ScreenGarage>(data);
        }
    }
    public void ShopBtn()
    {
        if (UIManager.HasInstance)
        {
            var data = new CarInfoData { Mode = CarInfoMode.Buy };
            currentFilterIndexShop = 0;
            ShowScreenWithHistory<ScreenShop>(data);
        }
    }
    public void CarInfoBtn(object data = null)
    {
        if (UIManager.HasInstance)
        {
            ShowScreenWithHistory<ScreenCarInfo>(data);
        }
    }
    public void CarViewBtn()
    {
        if (UIManager.HasInstance)
        {
            ShowScreenWithHistory<ScreenCarView>();
        }
    }
    public void SettingBtn()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<ScreenSetting>();
        }
    }
    public void HomeBtn()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.HideAllScreens();
            UIManager.Instance.HideAllOverlaps();
            UIManager.Instance.HideAllPopups();
            UIManager.Instance.HideAllNotifies();
            //Show screenHome
            UIManager.Instance.ShowScreen<ScreenHome>();
        }
    }
    public void RaceBtn()
    {
        LoadSceneWithLoading("R&D");

    }
    public void PauseBtn()
    {
        TogglePause();
    }
    public void ResumeBtn()
    {
        TogglePause();
    }
    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;
        if (UIManager.HasInstance)
        {
            if (isPaused == true)
            {
                UIManager.Instance.ShowPopup<PopupPause>();
            }
            else
            {
                UIManager.Instance.HideAllPopups();
            }
        }
    }
    public void RestartBtn()
    {
        if (UIManager.HasInstance)
        {
            isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            ReloadCurrentScene();
        }
    }
    public void BackGarageBtn()
    {
        if (UIManager.HasInstance)
        {
            Time.timeScale = 1f;
            isPaused = false;
            AudioListener.pause = false;
            UIManager.Instance.HideAllScreens();
            LoadSceneWithLoading("Garage");
        }
    }
    public void QuitGameBtn()
    {
        Debug.Log("Exit game");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion 
}
