using UnityEngine;

public class ShopPanel : MonoBehaviour
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
     public void OnClickCarInfo()
    {
        if(UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.carInfoPanel.gameObject);
            //UIManager.Instance.carInfoPanel.buyObject.SetActive(true);
        }
    }
    #endregion
}
