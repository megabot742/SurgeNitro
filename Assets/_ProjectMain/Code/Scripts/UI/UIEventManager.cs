using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class UIEventManager : BaseManager<UIEventManager>
{
    public bool isPaused = false;
    private Stack<GameObject> panelHistory = new Stack<GameObject>();
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {

    }
    private void Update()
    {
        PauseSetup();
    }
    private void PauseSetup()
    {
        if (UIManager.HasInstance)
        {
            if (UIManager.Instance.hUDPanel.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
    }
    public void PlayGame()
    {
        if(UIManager.HasInstance)
        {
            Instance.OpenNewPanel(UIManager.Instance.raceSetupPanel.gameObject);
        }
    }
    public void SettingGame()
    {
        if (UIManager.HasInstance)
        {
            Instance.OpenNewPanel(UIManager.Instance.settingPanel.gameObject);
        }
    }
    public void RaceGame()
    {
        Debug.LogWarning("Let's race");
        if (UIManager.HasInstance)
        {
            UIManager.Instance.SwitchToScene("R&D");
        }
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

    public void BackGarage()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.SwitchToScene("Garage");
            Time.timeScale = 1f;
            isPaused = !isPaused;
            AudioListener.pause = isPaused;
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


    #region PanelHistory
    public GameObject CurrentActivePanel { get; private set; }
    public GameObject GetPreviousPanel()
    {
        return panelHistory.Count > 0 ? panelHistory.Peek() : null;
    }
    public GameObject GetPanelAtDepth(int depth)
    {
        if (depth < 0 || depth >= panelHistory.Count)
        {
            return null;
        }

        // Convert stack sang array (array[0] = bottom/oldest, array[Count-1] = top/newest)
        var historyArray = panelHistory.ToArray();

        // Tính index: depth 0 = array[Count-1], depth 1 = array[Count-2], v.v.
        return historyArray[depth];
    }
    public void SetCurrentActivePanel(GameObject panel)
    {
        CurrentActivePanel = panel;
        // Optional: Clear history nếu muốn reset toàn bộ khi set initial
        panelHistory.Clear();
    }
    public void OpenNewPanel(GameObject newPanel, bool saveHistory = true)
    {
        // Nếu Current null, có lẽ là bug initialization → log để debug
        if (CurrentActivePanel == null)
        {
            Debug.LogWarning("CurrentActivePanel is null when opening new panel. Check initialization in UpdateUIForScene.");
        }

        // Push và tắt current nếu có
        if (saveHistory && CurrentActivePanel != null)
        {
            panelHistory.Push(CurrentActivePanel);
            CurrentActivePanel.SetActive(false);  // Đảm bảo tắt current
        }

        // Bật newPanel
        newPanel.SetActive(true);
        CurrentActivePanel = newPanel;
    }

    // Thuộc tính để lấy panel đang active (tùy chọn)
    public void GoBackPanel()
    {
        if (panelHistory.Count == 0)
        {
            return;
        }

        //Get previous Panel
        GameObject previousPanel = panelHistory.Pop();

        // Tắt panel hiện tại
        if (CurrentActivePanel != null)
        {
            CurrentActivePanel.SetActive(false);
        }

        // Bật lại panel trước
        previousPanel.SetActive(true);

        // Cập nhật current
        CurrentActivePanel = previousPanel;
    }
    public void GoBackMultiple(int steps = 1)
    {
        if (steps <= 0)
        {
            Debug.LogWarning("GoBackMultiple: steps phải > 0");
            return;
        }

        // Nếu không đủ history để back đủ steps → back hết những gì có
        int actualSteps = Mathf.Min(steps, panelHistory.Count);

        if (actualSteps == 0)
        {
            // Không có gì để back
            return;
        }

        // Pop và tắt các panel trung gian (nếu có)
        for (int i = 1; i < actualSteps; i++)
        {
            // Pop các panel ở giữa (không cần bật lại vì ta sẽ bật panel đích cuối cùng)
            GameObject intermediate = panelHistory.Pop();
            intermediate.SetActive(false);
        }

        // Pop và bật panel đích (panel cuối cùng muốn về)
        GameObject targetPanel = panelHistory.Pop();
        if (CurrentActivePanel != null)
        {
            CurrentActivePanel.SetActive(false);
        }

        targetPanel.SetActive(true);
        CurrentActivePanel = targetPanel;
        //Debug.Log($"GoBackMultiple({steps}): Về panel {targetPanel.name}");
    }

    public void GoToHome()
    {
        if (UIManager.HasInstance)
        {
            // Xóa hết lịch sử
            panelHistory.Clear();

            // Tắt panel hiện tại
            if (CurrentActivePanel != null)
                CurrentActivePanel.SetActive(false);

            // Bật Home Menu
            UIManager.Instance.resourcePanel.gameObject.SetActive(true);
            UIManager.Instance.homeMenuPanel.gameObject.SetActive(true);
            CurrentActivePanel = UIManager.Instance.homeMenuPanel.gameObject;
        }
    }
    #endregion
}
