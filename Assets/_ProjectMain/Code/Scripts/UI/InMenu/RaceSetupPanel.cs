using UnityEngine;

public class RaceSetupPanel : MonoBehaviour
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
    public void OnClickGoBack()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackPanel(); //Back HomeMenu Panel
        }
    }
    public void OnClickToGarage()
    {
        if(UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.garagePanel.gameObject);
        }
    }
    public void OnClickRace()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.RaceGame();
        }
    }
    #endregion
}
