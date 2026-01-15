using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCarInfo : BaseCarInfoDisplayScreen
{
    [Header("Upgrade Buttons")]
    [SerializeField] private Button topSpeedBtn;
    [SerializeField] private Button accelerationBtn;
    [SerializeField] private Button handlingBtn;
    [SerializeField] private Button nitroBtn;

    [Header("RaceSetupSetting")]
    public GameObject selectObject;
    public Button selectBtn;

    [Header("ShopSetting")]
    public GameObject buyObject;
    public Button buyBtn;
    public TMP_Text coinTxt;

    private CarInfoData infoData;
    private CarInfoData lastValidInfoData;
    private CarParam currentCar;
    private PlayerCarData currentPlayerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init()
    {
        base.Init();
    }
    public override void Show(object data)
    {
        base.Show(data);
        GameEvent.OnCarPurchased += OnCarPurchasedHandler;  // Subscribe event
        //Camera show
        if (CameraManager.HasInstance)
        {
            CameraManager.Instance.SwitchMenuCamera(MenuCameraType.CarInfo);
        }

        if (data is CarInfoData newData && newData.Car != null)
        {
            infoData = newData;
            lastValidInfoData = newData;  // Lưu để dùng khi back
        }
        else if (lastValidInfoData != null)
        {
            // Back từ Upgrade → dùng data cũ
            infoData = lastValidInfoData;
            Debug.Log("Using last valid CarInfoData from cache");
        }
        else
        {
            Debug.LogError("No CarInfoData available (first show or cache miss)!");
            return;
        }
        //Set currentCar từ infoData (đảm bảo không null)
        currentCar = infoData.Car;
        if (currentCar == null)
        {
            Debug.LogError("CurrentCar is null! Cannot proceed.");
            return;
        }

        currentPlayerData = PlayerManager.Instance.GetPlayerCarData(infoData.Car.carName);
        // Load UI
        LoadCarData(infoData.Car, currentPlayerData);

        // Show car
        if (CarShowManager.HasInstance)
        {
            CarShowManager.Instance.ShowCarModel(infoData.Car.carName);
        }

        // Xóa listener cũ để tránh trùng
        selectBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.RemoveAllListeners();
        RemoveListenerTabButtons();

        // Ẩn hết trước
        selectObject.SetActive(false);
        buyObject.SetActive(false);

        switch (infoData.Mode)
        {
            case CarInfoMode.View:
                // Chỉ xem, không hiện nút nào
                AddListenerUpgradeButtons(); //Can upgrade
                break;

            case CarInfoMode.SelectForRace:
                AddListenerUpgradeButtons(); //Can upgrade
                selectObject.SetActive(true);
                selectBtn.onClick.AddListener(OnSelectForRace);
                break;

            case CarInfoMode.Buy:
                buyObject.SetActive(true);
                buyBtn.onClick.AddListener(OnBuyCar);
                coinTxt.text = infoData.Car.priceCar.ToString("N0");
                break;
        }
    }
    public override void Hide()
    {
        GameEvent.OnCarPurchased -= OnCarPurchasedHandler; //Unsubscribe Event
        base.Hide();
    }
    public override void Clear()
    {
        // Reset UI elements
        selectObject.SetActive(false);
        buyObject.SetActive(false);
        coinTxt.text = "10000";
    }
    //Handler Event
    private void OnCarPurchasedHandler(string carName)
    {
        if (infoData != null && infoData.Car.carName == carName)
        {
            // Refresh UI nếu đang xem xe vừa mua
            currentPlayerData = PlayerManager.Instance.GetPlayerCarData(carName);
            if (currentPlayerData != null)
            {
                // Force update UI mà không cần hide/show
                LoadCarData(infoData.Car, currentPlayerData);
                Debug.Log("Refreshed CarInfo for newly purchased car: " + carName);
            }
        }
    }
    // private PlayerCarData GetPlayerDataIfOwned()
    // {
    //     if (infoData.Mode == CarInfoMode.View || infoData.Mode == CarInfoMode.SelectForRace)
    //     {
    //         return PlayerManager.Instance.GetPlayerCarData(infoData.Car.carName);
    //     }
    //     Debug.Log("Null playdata");
    //     return null;
    // }
    #region Upgrade Handler
    private void AddListenerUpgradeButtons()
    {
        if (topSpeedBtn != null) topSpeedBtn.onClick.AddListener(() => OnUpgradeButtonClicked(CarStatType.TopSpeed));
        if (accelerationBtn != null) accelerationBtn.onClick.AddListener(() => OnUpgradeButtonClicked(CarStatType.Acceleration));
        if (handlingBtn != null) handlingBtn.onClick.AddListener(() => OnUpgradeButtonClicked(CarStatType.Handling));
        if (nitroBtn != null) nitroBtn.onClick.AddListener(() => OnUpgradeButtonClicked(CarStatType.Nitro));
    }
    private void RemoveListenerTabButtons()
    {
        if (topSpeedBtn != null) topSpeedBtn.onClick.RemoveAllListeners();
        if (accelerationBtn != null) accelerationBtn.onClick.RemoveAllListeners();
        if (handlingBtn != null) handlingBtn.onClick.RemoveAllListeners();
        if (nitroBtn != null) nitroBtn.onClick.RemoveAllListeners();
    }
    private void OnUpgradeButtonClicked(CarStatType type)
    {
        if (infoData == null || infoData.Car == null)
        {
            Debug.LogError("Cannot upgrade: No Car data available in ScreenCarInfo!");
            return;  // Không crash, chỉ log lỗi
        }

        var carInfoDataForUpgrade = new CarInfoData
        {
            Mode = infoData.Mode,           // giữ nguyên View / SelectForRace
            Car = infoData.Car
        };

        // Gói thêm StatType vào một object tạm (hoặc mở rộng UpgradeData nếu muốn)
        var upgradePayload = new UpgradePayload
        {
            InfoData = carInfoDataForUpgrade,
            InitialStatType = type
        };

        if (UIManager.HasInstance)
        {
            UIEventManager.Instance.ShowScreenWithHistory<ScreenCarUpgrade>(upgradePayload);
        }
        this.Hide();
    }
    #endregion
    //---Button---
    #region Button
    private void OnSelectForRace()
    {
        if (currentCar == null)
        {
            Debug.LogError("CurrentCar is null in OnSelectForRace! Falling back to default.");
            string defaultCarName = PlayerManager.Instance.defaultCarName; // Lấy default từ PlayerManager
            PlayerManager.Instance.SetCurrentCar(defaultCarName);
            return;
        }
        PlayerManager.Instance.SetCurrentCar(currentCar.carName);
        Debug.Log("Select car: " + currentCar.carName + "Save to PlayerManager");
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackMultiple(2);
        }
        this.Hide();
    }
    private void OnBuyCar()
    {
        //Check data
        if (infoData == null || infoData.Car == null)
        {
            Debug.LogError("No car data for buying!");
            return;
        }
        CarParam car = infoData.Car;
        long price = (long)car.priceCar;  // Giả sử priceCar là float, cast sang long

        if (PlayerManager.HasInstance && PlayerManager.Instance.SpendCoin(price))
        {
            // Mua thành công → Khởi tạo xe trong PlayerManager
            PlayerManager.Instance.InitCarData(car.carName);
            //GameEvent.CarPurchased(car.carName); //Call event
            Debug.Log($"Buy car {car.carName} success! Price: {price}");

            // Quay về ScreenShop (hoặc Garage tùy thiết kế)
            if (UIEventManager.HasInstance)
            {
                UIEventManager.Instance.GoBack();  // Hoặc GoBackMultiple(1) nếu cần
                                                   // Optional: Reset filter nếu muốn
                                                   //UIEventManager.Instance.currentFilterIndexShop = 0;
            }

            this.Hide();
        }
        else
        {
            // Không đủ tiền → Có thể show popup thông báo sau này
            Debug.LogWarning("Không đủ coin để mua xe " + car.carName);
        }
    }
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
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        if (CameraManager.HasInstance)
        {
            CameraManager.Instance.SwitchMenuCamera(MenuCameraType.Home);
        }
        this.Hide();
    }
    #endregion
}
