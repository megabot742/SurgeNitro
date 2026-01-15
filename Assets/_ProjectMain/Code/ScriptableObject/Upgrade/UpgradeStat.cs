using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UpgradeStat
{
    public CarStatType statType;
    public float baseValue;
    public float maxValue;
    public int baseGold; 
    [SerializeField, Range(0,10)] private int currentLevel; // Chỉ editor, runtime dùng PlayerCarData
    [SerializeField, ReadOnly] private float currentValue; 
    [SerializeField, ReadOnly] private float goldUpgrade; 

    private static readonly float[] coefficients = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 1.0f, 1.1f, 1.2f, 1.3f }; //level 0 -> level 10
    private const float totalSum = 6.7f;

    //public int CurrentLevel => currentLevel;
    public int CurrentLevel //Getter current Level for UI
    {
        get => currentLevel;
        set => currentLevel = value;
    }
    public float CurrentValue ////Getter current Value for UI
    {
        get  => currentValue; 
    }  
    public float GoldUpgrade ////Getter current Gold for UI
    {
        get => goldUpgrade; 
        set => goldUpgrade = value;
    }
        
    public float GetPreviewNextValue()
    {
        if(!CanUpgrade())
        {
            return GetCurrentValue(10); //max value
        }
        return GetCurrentValue(currentLevel + 1); //Getter next Value for UI
    } 
    
    public float GetCurrentValue(int levelOverride = -1)
    {
        int level = levelOverride >= 0 ? levelOverride : currentLevel;
        if (level <= 0) return baseValue;

        float partialSum = 0f;
        for (int i = 0; i < level; i++)
            partialSum += coefficients[i];

        float delta = maxValue - baseValue;
        return baseValue + (delta * (partialSum / totalSum));
    }
    public bool CanUpgrade() => currentLevel < 10; //Check upgrade, max level 10

    public bool TryUpgrade(out float newValue)
    {
        if (!CanUpgrade())
        {
            newValue = 0f;
            return false;
        }

        currentLevel++;
        newValue = GetCurrentValue();  // Tính mới
        currentValue = newValue;  // Update field
        return true;
    }
    public int GetNextUpgradeCost()
    {
        if (!CanUpgrade())
        {
            goldUpgrade = 0; //Show 0
            return 0;
        }   //Reach Max level

        int nextIndex = currentLevel;  // coefficients[0] cho lên level 1, coefficients[9] cho lên level 10
        float nextGoldUpgrade = baseGold * ( 1 + coefficients[nextIndex]);
        goldUpgrade = nextGoldUpgrade; //Show inpecter for testing;
        return Mathf.RoundToInt(nextGoldUpgrade);  // Làm tròn thành int (hoặc CeilToInt nếu muốn làm tròn lên)
    }
    public void OnValidate()
    {
#if UNITY_EDITOR
        currentValue = GetCurrentValue();
        GetNextUpgradeCost();
        Debug.Log("Level changed: " + currentLevel + " -> Value: " + currentValue + " Next cost: " + GetNextUpgradeCost());
#endif
    }
}
