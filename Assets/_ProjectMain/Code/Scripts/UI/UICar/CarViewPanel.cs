using UnityEngine;

public class CarViewPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region 
    public void OnClickGoBack()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackPanel(); //Back CarInfo Panel
        }
    }
    #endregion
}
