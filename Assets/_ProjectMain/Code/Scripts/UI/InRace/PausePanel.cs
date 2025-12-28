using UnityEngine;

public class PausePanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickResume()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PauseGame();
        }
    }
    public void OnClickRestart()
    {
        if (UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject);
            UIEventManager.Instance.RestartGame();
        }
    }
    public void OnClickGarage()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.BackGarage(); //Return scene Garage
        }
    }
    public void OnClickClose()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PauseGame();
        }
    }
}
