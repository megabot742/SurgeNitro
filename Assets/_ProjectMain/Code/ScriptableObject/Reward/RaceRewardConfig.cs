using UnityEngine;

[CreateAssetMenu(menuName = "Game/Race/Race Reward Config")]
public class RaceRewardConfig : ScriptableObject
{
    [Header("Base reward per position")]
    public int reward1st = 50000;
    public int reward2nd = 30000;
    public int reward3rd = 20000;
    public int rewardDefault = 10000;     // từ vị trí 4 trở đi

    [Header("Bonus")]
    public int bonusMin = 5000;
    public int bonusMax = 10000;
}
