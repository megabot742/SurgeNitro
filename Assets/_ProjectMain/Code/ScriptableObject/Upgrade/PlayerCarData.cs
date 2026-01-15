using System;
using UnityEngine;

[System.Serializable]
public class PlayerCarData
{
    public string carName; // Thường là carName hoặc một unique ID
    public int currentRank;
    public UpgradeStat[] stats = new UpgradeStat[4]; // 4 stats

    public int GetTotalUpgradesDone()
    {
        int total = 0;
        foreach (var stat in stats) total += stat.CurrentLevel;
        return total;
    }

    // Gọi khi upgrade 1 stat
    public void ApplyUpgrade(CarStatType type, int rankIncreasePerUpgrade = 5)
    {
        foreach (var stat in stats)
        {
            if (stat.statType == type && stat.TryUpgrade(out float _))
            {
                currentRank += rankIncreasePerUpgrade;
                break;
            }
        }
    }
}
