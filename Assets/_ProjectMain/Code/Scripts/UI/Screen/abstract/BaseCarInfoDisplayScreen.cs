using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseCarInfoDisplayScreen : BaseScreen
{
    [Header("References ")]
    [SerializeField] protected CarDatabaseSO carDatabaseSO;
    [SerializeField] protected TMP_Text carName;
    [SerializeField] protected Image carClassColor;
    [SerializeField] protected TMP_Text carClass;
    [SerializeField] protected TMP_Text carRank;

    [SerializeField] protected TMP_Text carSpeedTxt; // value 100 - 400 (km/h)
    [SerializeField] protected Slider carSpeedSlider;

    [SerializeField] protected TMP_Text carAccelerationTxt; // value 5 - 2 (second form 0 -> 100km/h)
    [SerializeField] protected Slider carAccelerationSlider;

    [SerializeField] protected TMP_Text carHandingTxt; // 0.2% - 0.9% (percentage of controllability when cornering)
    [SerializeField] protected Slider carHandingSlider;

    [SerializeField] protected TMP_Text carNitroTxt; // 5-10s (time for using nitro)
    [SerializeField] protected Slider carNitroSlider;

    protected void LoadCarData(CarParam car, PlayerCarData playerData = null)
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

        // Rank: Nếu có playerData → dùng currentRank từ player (sau upgrade), else baseRank
        int rank = playerData != null ? playerData.currentRank : car.carBaseRank;
        int maxRank = car.carMaxRank;
        if (carRank != null) carRank.text = $"{rank}/{maxRank}";

        // Parameters with sliders (tùy nguồn)
        // Speed
        float speedValue = GetStatValue(CarStatType.TopSpeed, car, playerData);
        if (carSpeedTxt != null) carSpeedTxt.text = $"{speedValue:F2}";
        if (carSpeedSlider != null)
        {
            carSpeedSlider.minValue = 80f;
            carSpeedSlider.maxValue = 420f;
            carSpeedSlider.value = speedValue;
        }

        // Acceleration (invert slider)
        float accValue = GetStatValue(CarStatType.Acceleration, car, playerData);
        if (carAccelerationTxt != null) carAccelerationTxt.text = $"{accValue:F2}";
        if (carAccelerationSlider != null)
        {
            float minAcc = 1.5f;
            float maxAcc = 6f;
            carAccelerationSlider.minValue = 0f;
            carAccelerationSlider.maxValue = 1f;
            float normalizedAcc = (maxAcc - accValue) / (maxAcc - minAcc); // Invert: thấp = tốt → gần 1
            carAccelerationSlider.value = Mathf.Clamp(normalizedAcc, 0f, 1f);
        }

        // Handling
        float handlingValue = GetStatValue(CarStatType.Handling, car, playerData);
        if (carHandingTxt != null) carHandingTxt.text = $"{handlingValue:F2}";
        if (carHandingSlider != null)
        {
            carHandingSlider.minValue = 0.1f;
            carHandingSlider.maxValue = 1f;
            carHandingSlider.value = handlingValue;
        }

        // Nitro
        float nitroValue = GetStatValue(CarStatType.Nitro, car, playerData);
        if (carNitroTxt != null) carNitroTxt.text = $"{nitroValue:F2}";
        if (carNitroSlider != null)
        {
            carNitroSlider.minValue = 1f;
            carNitroSlider.maxValue = 20f;  // Adjust nếu cần
            carNitroSlider.value = nitroValue;
        }
    }

    protected Color GetClassColor(CarClass carClass)
    {
        if (carDatabaseSO == null) return Color.white;

        var container = carDatabaseSO.GetContainer(carClass);
        return container != null ? container.ClassColor : Color.white;
    }
    private float GetStatValue(CarStatType type, CarParam car, PlayerCarData playerData)
    {
        if (playerData != null)
        {
            // Xe sở hữu → lấy current từ PlayerData (sau upgrade)
            UpgradeStat stat = playerData.stats[(int)type];
            return stat != null ? stat.CurrentValue : car.GetCurrentValue(type);  // Fallback nếu stat null
        }
        else
        {
            // Xe mới → lấy base từ CarParam
            return car.GetCurrentValue(type);  // Hiện tại là baseValue vì level=0
        }
    }
}
