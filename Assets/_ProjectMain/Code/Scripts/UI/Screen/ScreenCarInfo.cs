using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCarInfo : BaseScreen
{
    [Header("References")]
    [SerializeField] private CarDatabaseSO carDatabaseSO;
    [SerializeField] private TMP_Text carName;
    [SerializeField] private Image carClassColor;
    [SerializeField] private TMP_Text carClass;
    [SerializeField] private TMP_Text carRank;

    [SerializeField] private TMP_Text carSpeedTxt; // value 100 - 400 (km/h)
    [SerializeField] private Slider carSpeedSlider;

    [SerializeField] private TMP_Text carAccelerationTxt; // value 5 - 2 (second form 0 -> 100km/h)
    [SerializeField] private Slider carAccelerationSlider;

    [SerializeField] private TMP_Text carHandingTxt; // 0.2% - 0.9% (percentage of controllability when cornering)
    [SerializeField] private Slider carHandingSlider;

    [SerializeField] private TMP_Text carNitroTxt; // 5-10s (time for using nitro)
    [SerializeField] private Slider carNitroSlider;

    [Header("RaceSetupSetting")]
    public GameObject selectObject;
    public Button selectBtn;

    [Header("ShopSetting")]
    public GameObject buyObject;
    public Button buyBtn;
    public TMP_Text coinTxt;

    private CarInfoData infoData;
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
        if (infoData.Car != null)
        {
            LoadCarData(infoData.Car);
        }
        else
        {
            Debug.LogWarning("No CarParam provided in CarInfoData");
        }

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
    private void LoadCarData(CarParam car)
    {
        if (car == null) return;

        // Basic info
        if (carName != null) carName.text = car.carName;

        if (carClassColor != null)
        {
            Color classColor = GetClassColor(car.carClass);
            carClassColor.color = classColor;
        }

        if (carClass != null)
        {
            string displayClass = car.carClass.ToString().Replace("class", "").ToUpper();
            carClass.text = displayClass;
        }

        if (carRank != null) carRank.text = $"{car.carCurrentRank}/{car.carMaxRank}";

        // Parameters with sliders (normalize nếu cần, dựa trên ranges)
        // Speed: 100-400 km/h (cao hơn tốt hơn)
        if (carSpeedTxt != null) carSpeedTxt.text = $"{car.topSpeed}";
        if (carSpeedSlider != null)
        {
            carSpeedSlider.minValue = 100f;
            carSpeedSlider.maxValue = 400f;
            carSpeedSlider.value = car.topSpeed;
        }

        // Acceleration: 2-5 seconds (thấp hơn tốt hơn → invert cho slider progress: 1 = tốt nhất)
        if (carAccelerationTxt != null) carAccelerationTxt.text = $"{car.acceleration}";
        if (carAccelerationSlider != null)  // Lưu ý: code bạn có typo "carAccelerationlider" → sửa thành carAccelerationSlider nếu cần
        {
            float minAcc = 2f;
            float maxAcc = 5f;
            carAccelerationSlider.minValue = 0f;  // Normalize to 0-1
            carAccelerationSlider.maxValue = 1f;
            float normalizedAcc = (maxAcc - car.acceleration) / (maxAcc - minAcc);  // Invert: thấp = tốt → gần 1
            carAccelerationSlider.value = Mathf.Clamp(normalizedAcc, 0f, 1f);
        }

        // Handling: 0.2-0.9 (cao hơn tốt hơn)
        if (carHandingTxt != null) carHandingTxt.text = $"{car.handling}";
        if (carHandingSlider != null)
        {
            carHandingSlider.minValue = 0.2f;
            carHandingSlider.maxValue = 1f;
            carHandingSlider.value = car.handling;
        }

        // Nitro: 2-10 seconds (cao hơn tốt hơn, adjust từ default 2)
        if (carNitroTxt != null) carNitroTxt.text = $"{car.nitro}";
        if (carNitroSlider != null)
        {
            carNitroSlider.minValue = 2f;
            carNitroSlider.maxValue = 10f;
            carNitroSlider.value = car.nitro;
        }
    }
    private Color GetClassColor(CarClass carClass)
    {
        if (carDatabaseSO == null) return Color.white;

        var container = carDatabaseSO.GetContainer(carClass);
        return container != null ? container.ClassColor : Color.white;
    }
    //---Button---
    #region Button
    private void OnSelectForRace()
    {
        Debug.Log("Select car: " + infoData.Car);
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBackMultiple(2);
        }
        this.Hide();
    }
    private void OnBuyCar()
    {
        Debug.Log("Buy car " + infoData.Car);

        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
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
        this.Hide();
    }
    #endregion
}
