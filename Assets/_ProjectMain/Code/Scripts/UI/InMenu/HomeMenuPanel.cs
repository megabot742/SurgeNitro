using Unity.VisualScripting;
using UnityEngine;

public class HomeMenuPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Button
    public void OnClickRaceSetup()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PlayGame();
        }
    }
    public void OnClickToGarage()
    {
        if(UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.garagePanel.gameObject);
        }
    }
    public void OnClickToShop()
    {
        if(UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.shopPanel.gameObject);
        }
    }
    public void OnClickToSetting()
    {
        if(UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.SettingGame();
        }
    }
    public void OnClickExitGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.QuitGame(); //Exit game
        }
    }
    #endregion
}
