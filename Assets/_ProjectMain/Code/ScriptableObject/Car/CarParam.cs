using UnityEngine;

[System.Serializable]
public class CarParam
{
    [Header("Car Setting")]
    public CarClass carClass;
    public int carBaseRank;
    public int carCurrentRank;
    [SerializeField, ReadOnly] private int carMaxRankAuto;
    public string carName;
    public GameObject carPrefab;
    public GameObject carShowModel;
    public Sprite carSprite;

    [Header("Price")]
    public float priceCar = 2000f;

    // [Header("Car Parameter current")]
    // public float topSpeed; // Current value (calculated from stats[0])
    // public float acceleration; // Current value (calculated from stats[0])
    // public float handling; // Current value (calculated from stats[0])
    // public float nitro; // Current value (calculated from stats[0])

    [Header("Upgradable Stats")]
    public UpgradeStat[] stats = new UpgradeStat[4]; // Khởi tạo ở Inspector

    // Get current stat value (dùng trong gameplay)
    
    public float GetCurrentValue(CarStatType type)
    {
        foreach (var stat in stats)
        {
            if (stat.statType == type) return stat.CurrentValue;
        }
        return 0f;
    }

    public float GetBaseValue(CarStatType type)
    {
        foreach (var stat in stats)
        {
            if (stat.statType == type) return stat.baseValue;
        }
        return 0f;
    }

    public float GetMaxValue(CarStatType type)
    {
        foreach (var stat in stats)
        {
            if (stat.statType == type) return stat.maxValue;
        }
        return 0f;
    }

    // Auto caculate carMaxRank
    public int carMaxRank
    {
        get
        {
            if (carMaxRankAuto != carBaseRank + 200)
            {
                carMaxRankAuto = carBaseRank + 200;
            }
            return carMaxRankAuto;
        }
    }
    
    #region OnValidate
    public void OnValidate() {
#if UNITY_EDITOR
        // Đồng bộ rank ban đầu nếu level=0
        if (carCurrentRank != 0) 
        {
            carCurrentRank = carBaseRank;
        }   
#endif
    }
    #endregion
}
