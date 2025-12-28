using TMPro;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    public TMP_Text posNumberTxt;
    public TMP_Text bestTimeTxt;
    public TMP_Text unclockTrackTxt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region 
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
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.BackGarage();
        }
    }
    #endregion
}
