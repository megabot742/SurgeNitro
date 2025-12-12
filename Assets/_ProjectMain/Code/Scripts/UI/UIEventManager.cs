using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class UIEventManager : BaseManager<UIEventManager>
{
    public bool isPaused = false;
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        
    }
    private void Update()
    {
        if (UIManager.HasInstance)
        {
            if (UIManager.Instance.hUDPanel.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
    }
    public void StartGame()
    {
        // if (UIManager.HasInstance && RaceInfoManager.HasInstance)
        // {
        //     UIManager.Instance.homeMenuPanel.gameObject.SetActive(false);
        //     UIManager.Instance.raceSetupPanel.gameObject.SetActive(true);
        // }
    }
    public void SettingGame()
    {
        // if (UIManager.HasInstance)
        // {
        //     UIManager.Instance.homeMenuPanel.gameObject.SetActive(false);
        //     UIManager.Instance.settingPanel.gameObject.SetActive(true);
        // }
    }
    public void Race()
    {
        // Debug.Log("Let's race");
        // if (UIManager.HasInstance)
        // {
        //     RaceInfoManager.Instance.enteredRace = true;
        //     UIManager.Instance.SwitchToScene(RaceInfoManager.Instance.raceToLoad);
        // }
    }
    public void PauseGame()
    {
        isPaused = !isPaused;
        if (UIManager.HasInstance)
        {
            UIManager.Instance.pausePanel.gameObject.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f; //When Pasue mean TimeScale = 0f
            //Pause Volume
            AudioListener.pause = isPaused;
        }
    }

    public void ResumeGame()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.pausePanel.gameObject.SetActive(false);
        }
    }

    public void RestartGame()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ReloadCurrentScene();
            isPaused = !isPaused;
            Time.timeScale = 1f;
            AudioListener.pause = isPaused;
        }

    }

    public void BackMenu()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.SwitchToScene("Menu");
            Time.timeScale = 1f;
        }
    }


    public void QuitGame()
    {
        Debug.Log("Exit game");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
