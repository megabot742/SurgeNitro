using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCarInfo : BaseScreen
{
    private CarInfoData infoData;
    [Header("RaceSetupSetting")]
    public GameObject selectObject;
    public Button selectBtn;

    [Header("ShopSetting")]
    public GameObject buyObject;
    public Button buyBtn;
    public TMP_Text coinTxt;

    private Enum currentMode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init()
    {
        base.Init();
    }
    public override void Hide()
    {
        base.Hide();
    }
    public override void Show(object data)
    {
        base.Show(data);
        //Camera show
        if (CameraManager.HasInstance)
        {
            CameraManager.Instance.SwitchMenuCamera(MenuCameraType.CarInfo);
        }
        //Check data
        infoData = data as CarInfoData;
        if (infoData == null) return;

        // Load thông tin xe
        LoadCarData(infoData.CarId ?? "default");

        // Xóa listener cũ để tránh trùng
        selectBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.RemoveAllListeners();

        // Ẩn hết trước
        selectObject.SetActive(false);
        buyObject.SetActive(false);

        switch (infoData.Mode)
        {
            case CarInfoMode.View:
                // Chỉ xem, không hiện nút nào
                break;

            case CarInfoMode.SelectForRace:
                selectObject.SetActive(true);
                selectBtn.onClick.AddListener(OnSelectForRace);
                break;

            case CarInfoMode.Buy:
                buyObject.SetActive(true);
                buyBtn.onClick.AddListener(OnBuyCar);
                // coinTxt.text = GiáXe(infoData.CarId);
                break;
        }
    }
    public override void Clear()
    {
        // Reset UI elements
        selectObject.SetActive(false);
        buyObject.SetActive(false);
        coinTxt.text = "10000";
    }
    private void LoadCarData(string carId)
    {
        // Logic load data xe từ database hoặc manager (giả sử)
        // carNameText.text = CarManager.GetCarName(carId);
        // ...
    }
    #region Button
    private void OnSelectForRace()
    {
        Debug.Log("Select car: " + infoData.CarId);        
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackMultiple(2);
        }
        this.Hide();
    }
    private void OnBuyCar()
    {
        Debug.Log("Buy car " + infoData.CarId);

        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
    //Button
    public void OnClickToView()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.CarViewBtn();
        }
        this.Hide();
    }
    public void OnClickToUpgrade()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<ScreenCarUpgrade>();
        }
        this.Hide();
    }
    public void OnClickGoBack()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
    #endregion
}
