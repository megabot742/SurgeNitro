using UnityEngine;

public class RaceRewardCalculator
{
    // [SerializeField] private RaceRewardConfig config;

    // public RaceRewardCalculator(RaceRewardConfig config)
    // {
    //     this.config = config;
    // }

    // public long CalculateReward(int position, int totalLaps)
    // {
    //     long baseReward = GetBaseReward(position);
    //     long lapBonus = Mathf.RoundToInt(baseReward * (totalLaps - 1) * config.lapMultiplier);
    //     long randomBonus = Random.Range(config.bonusMin, config.bonusMax + 1);

    //     return baseReward + lapBonus + randomBonus;
    // }

    // // private long GetBaseReward(int position)
    // // {
    // //     return position switch
    // //     {
    // //         1 => config.reward1st,
    // //         2 => config.reward2nd,
    // //         3 => config.reward3rd,
    // //         _ => config.rewardDefault
    // //     };
    // // }
}
