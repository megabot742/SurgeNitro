using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class UIEventManager : BaseManager<UIEventManager>
{
    public bool isPaused = false;
    private Stack<Type> screenHistory = new Stack<Type>();
    private Dictionary<Type, Action<object>> _showScreenMap = new Dictionary<Type, Action<object>>();
    protected override void Awake()
    {
        base.Awake();

        _showScreenMap.Add(typeof(ScreenHome), data => UIManager.Instance.ShowScreen<ScreenHome>(data));
        _showScreenMap.Add(typeof(ScreenGarage), data => UIManager.Instance.ShowScreen<ScreenGarage>(data));
        _showScreenMap.Add(typeof(ScreenShop), data => UIManager.Instance.ShowScreen<ScreenShop>(data));
        _showScreenMap.Add(typeof(ScreenRaceSetup), data => UIManager.Instance.ShowScreen<ScreenRaceSetup>(data));
        _showScreenMap.Add(typeof(ScreenCarInfo), data => UIManager.Instance.ShowScreen<ScreenCarInfo>(data));
        _showScreenMap.Add(typeof(ScreenCarView), data => UIManager.Instance.ShowScreen<ScreenCarView>());
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
        if (_showScreenMap.ContainsKey(targetType))
        {
            _showScreenMap[targetType](data); // Gọi show với data (có thể null)
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
    private void Start()
    {

    }
    #region Button
    private void Update()
    {
        PauseSetup();
    }
    private void PauseSetup()
    {
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
    public void GarageBtn(object data = null)
    {
        if (UIManager.HasInstance)
        {
            if (data == null)
            {
                data = new CarInfoData { Mode = CarInfoMode.View };
            }
            ShowScreenWithHistory<ScreenGarage>(data);
        }
    }
    public void ShopBtn()
    {
        if (UIManager.HasInstance)
        {
            var data = new CarInfoData { Mode = CarInfoMode.Buy };
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
        if (UIManager.HasInstance)
        {
            UIManager.Instance.LoadSceneWithLoading("R&D");
        }
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
            UIManager.Instance.ReloadCurrentScene();
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
            UIManager.Instance.LoadSceneWithLoading("Garage");
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
