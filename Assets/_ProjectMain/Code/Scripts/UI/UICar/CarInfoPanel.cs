using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarInfoPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("RaceSetupPanel")]
    public GameObject selectObject;
    public Button selectBtn;

    [Header("ShopPanel")]
    public GameObject buyObject;
    public Button buyBtn;
    public TMP_Text coinTxt;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnEnable()
    {
        if (UIEventManager.HasInstance && UIManager.HasInstance)
        {
            // Mặc định tắt hết để reset
            selectObject.SetActive(false);
            buyObject.SetActive(false);

            // Kiểm tra stack history để biết panel trước là gì
            GameObject previousPanel = UIEventManager.Instance.GetPanelAtDepth(0);
            GameObject secondPreviousPanel = UIEventManager.Instance.GetPanelAtDepth(1);
            if (previousPanel != null)
            {
                if (previousPanel == UIManager.Instance.garagePanel.gameObject && secondPreviousPanel == UIManager.Instance.raceSetupPanel.gameObject)
                {
                    //RaceSetup -> Garage -> CarInfo -> Select Option
                    Debug.Log("setup");
                    selectObject.SetActive(true);

                    // Attach listener cho selectBtn (nếu chưa attach, để chọn xe và back)
                    selectBtn.onClick.RemoveAllListeners(); // Xóa cũ để tránh duplicate
                    selectBtn.onClick.AddListener(OnClickSelect);
                }
                else if (previousPanel == UIManager.Instance.shopPanel.gameObject)
                {
                    // Từ ShopPanel -> bật buyObject (nếu cần, dựa trên logic hiện tại của bạn)
                    Debug.Log("shop");
                    buyObject.SetActive(true);
                    // Attach listener cho buyBtn nếu cần (giả sử bạn đã có)
                    buyBtn.onClick.RemoveAllListeners();
                    buyBtn.onClick.AddListener(OnClickBuy);
                }
                // Thêm case khác nếu cần, ví dụ từ GaragePanel thì không bật gì đặc biệt
            }
            Debug.Log($"CarInfoPanel - Previous: {previousPanel?.name ?? "None"}, Second Previous: {secondPreviousPanel?.name ?? "None"}");
        }
    }
    void OnDisable() //When panel disable
    {
        selectObject.SetActive(false);
        buyObject.SetActive(false);
    }
    #region Button
    public void OnClickToUpgrade()
    {
        if (UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.carUpgradePanel.gameObject);
        }
    }
    public void OnClickToView()
    {
        if (UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.OpenNewPanel(UIManager.Instance.carViewPanel.gameObject);
        }
    }
    public void OnClickSelect()
    {
        if (UIEventManager.HasInstance)
        {
            GameObject secondPrevious = UIEventManager.Instance.GetPanelAtDepth(1);

            if (secondPrevious == UIManager.Instance.raceSetupPanel.gameObject)
            {
                UIEventManager.Instance.GoBackMultiple(2); //CarInfo -> Garage -> RaceSetup
                Debug.LogWarning($"Select Car For Race");
            }
            else
            {
                UIEventManager.Instance.GoBackPanel();
            }
        }
    }
    public void OnClickBuy()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackPanel(); //CarInfo -> Shop
            Debug.LogWarning($"Car Has Buy");
        }
    }
    public void OnClickGoBack()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackPanel();//CarInfo -> Garage or CarInfo -> Shop
        }
    }
    #endregion
}
