using System;

public static class GameEvent
{
    #region ToggleButton
    public static event Action<int> OnFilterButtonSelected; //button toggle select

    public static void FilterButtonSelected(int index)
    {
        OnFilterButtonSelected?.Invoke(index);
    }
    #endregion
    #region SrceenRace
    public static event Action<int, int> OnLap; //currentLap, totalLaps
    public static event Action<int, int> OnPosition; //currentPosition, totalPosition
    public static event Action<float> OnTimeLap; //current time in lap
    public static event Action<float> OnBestTimeLap; //best time in lap
    public static event Action<float> OnSpeed; // speedKmH
    public static event Action<float, float> OnNitro; //remainTank, maxTank
    public static event Action<float> OnTimeCountDownStartRace; // time countDown for staring race
    public static event Action<float> OnTimeLeftForFinishRace; // time countDown for finish race

    public static void ShowLap(int currentLap, int totalLaps)
    {
        OnLap?.Invoke(currentLap, totalLaps);
    }
    public static void ShowPosition(int currentPosition, int totalPositions)
    {
        OnPosition?.Invoke(currentPosition, totalPositions);
    }
    public static void ShowTimeLap(float time)
    {
        OnTimeLap?.Invoke(time);
    }
    public static void ShowBestTimeLap(float bestTime)
    {
        OnBestTimeLap?.Invoke(bestTime);
    }
    public static void ShowSpeed(float speedKmH)
    {
        OnSpeed?.Invoke(speedKmH);
    }
    public static void ShowNitro(float remainTank, float maxTank)
    {
        OnNitro?.Invoke(remainTank, maxTank);
    }
    public static void ShowCountDownTime(float timeLeft)
    {
        OnTimeCountDownStartRace?.Invoke(timeLeft);
    }
    public static void ShowTimeLeft(int timeLeft)
    {
        OnTimeLeftForFinishRace?.Invoke(timeLeft);
    }
    #endregion
    #region PopupResult
    public static event Action<int, float> OnRaceFinished;  //Finish position, bestLapTime

    public static void ShowRaceFinished(int position, float bestTime)
    {
        OnRaceFinished?.Invoke(position, bestTime);
    }
    #endregion
    #region Currency / Player Data
    public static event Action<long> OnCoinChanged;
    public static event Action<string> OnCarPurchased;  // Truyền carName để biết xe nào được mua
    public static event Action<string> OnCarUpgraded;  // Truyền carName

    public static void CoinChanged(long newCoinAmount)
    {
        OnCoinChanged?.Invoke(newCoinAmount);
    }
    public static void CarPurchased(string carName)
    {
        OnCarPurchased?.Invoke(carName);
    }
    public static void CarUpgraded(string carName)
    {
        OnCarUpgraded?.Invoke(carName);
    }
    #endregion
}
