using System;

public static class GameEvent
{
    #region SrceenRace
    public static event Action<int, int> OnLap; //currentLap, totalLaps
    public static event Action<int, int> OnPosition; //currentPosition, totalPosition
    public static event Action<float> OnTimeLap; //current time in lap
    public static event Action<float> OnBestTimeLap; //best time in lap
    public static event Action<float> OnSpeed; // speedKmH
    public static event Action<float, float> OnNitro; //minTank, maxTank
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
    public static void ShowNitro(float minTank, float maxTank)
    {
        OnNitro?.Invoke(minTank, maxTank);
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
}
