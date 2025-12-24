using UnityEngine;

public class CarInfoPanel : MonoBehaviour
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
    public void OnClickToUpgrade()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject, UIManager.Instance.carUpgradePanel.gameObject);
        }
    }
    public void OnClickToView()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject, UIManager.Instance.carViewPanel.gameObject);
        }
    }
    public void OnClickGoBack()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ChangeUIGameObject(this.gameObject, UIManager.Instance.homeMenuPanel.gameObject);
        }
    }
    #endregion
}
