using UnityEngine;

public class CarUpgradePanel : MonoBehaviour
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
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject, UIManager.Instance.carInfoPanel.gameObject);
        }
    }
    #endregion
}
