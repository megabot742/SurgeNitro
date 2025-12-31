using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenGame : BaseScreen
{
    [Header("Lap & Pos")]
    [SerializeField] private TMP_Text lapText;      // lapTxt
    [SerializeField] private TMP_Text posText;       // posTxt

    [Header("Time")]
    [SerializeField] private TMP_Text lapTimeText;   // lapTimeTxt
    [SerializeField] private TMP_Text bestLapTimeText; // bestLapTimeTxt

    [Header("Speed & Nitro")]
    [SerializeField] private TMP_Text speedText;     // speedDometerText
    [SerializeField] private Slider nitroSlider;     // nitroSlider

    [Header("Countdown")]
    [SerializeField] private TMP_Text countDownNum;  // countDownNum (3,2,1)
    [SerializeField] private TMP_Text countDownTxt;  // countDownTxt (Go)
    [SerializeField] private TMP_Text countDownTimeLeft; // countDownTimeLeft  
    public override void Init() { base.Init(); }
    public override void Show(object data)
    {
        base.Show(data);
        // SUBSCRIBE TO EVENTS
        GameEvent.OnLap += HandleLap;
        GameEvent.OnPosition += HandlePosition;

        GameEvent.OnTimeLap += HandleTimeLap;
        GameEvent.OnBestTimeLap += HandleBestTimeLap;

        GameEvent.OnSpeed += HandleSpeed;
        GameEvent.OnNitro += HandleNitro;

        GameEvent.OnTimeCountDownStartRace += HandleTimeCountDown;
        GameEvent.OnTimeLeftForFinishRace += HandleTimeLeft;
    }

    public override void Hide()
    {
        base.Hide();
        // UNSUBSCRIBE TO AVOID MEMORY LEAK
        GameEvent.OnLap -= HandleLap;
        GameEvent.OnPosition -= HandlePosition;

        GameEvent.OnTimeLap -= HandleTimeLap;
        GameEvent.OnBestTimeLap -= HandleBestTimeLap;

        GameEvent.OnSpeed -= HandleSpeed;
        GameEvent.OnNitro -= HandleNitro;

        GameEvent.OnTimeCountDownStartRace -= HandleTimeCountDown;
        GameEvent.OnTimeLeftForFinishRace -= HandleTimeLeft;


    }
    public override void Clear()
    {
        if (lapText) lapText.text = "0/0";
        if (posText) posText.text = "0/0";
        if (lapTimeText) lapTimeText.text = "00:00.000";
        if (bestLapTimeText) bestLapTimeText.text = "00:00.000";
        if (speedText) speedText.text = "0";
        if (nitroSlider) nitroSlider.value = nitroSlider.maxValue; // full default
        if (countDownNum) countDownNum.gameObject.SetActive(false);
        if (countDownTxt) countDownTxt.gameObject.SetActive(false);
        if (countDownTimeLeft) countDownTimeLeft.gameObject.SetActive(false);
    }
    void Update()
    {
        if (UIManager.HasInstance && UIManager.Instance.isCountingDown) // Poll cho time left finish
        {
            int timeLeft = Mathf.RoundToInt(UIManager.Instance.GetEndCountDown());
            GameEvent.ShowTimeLeft(timeLeft); // Publish để handler nhận
        }
        else
        {
            countDownTimeLeft.gameObject.SetActive(false);
        }
    }
    // ========== SETTERS PUBLIC - Gọi từ ngoài ==========
    private void HandleLap(int currentLap, int totalLaps)
    {
        if (lapText != null)
            lapText.text = $"{currentLap}/{totalLaps}";
    }

    private void HandlePosition(int currentPosition, int totalPositions)
    {
        if (posText != null)
        {
            posText.text = $"{currentPosition}/{totalPositions}";
        }
    }
    private void HandleTimeLap(float timeLap)
    {
        var time = System.TimeSpan.FromSeconds(timeLap);
        if (lapTimeText != null)
        {
            lapTimeText.text = string.Format("{0:00}:{1:00}.{2:00}", time.Minutes, time.Seconds, time.Milliseconds);
        }
    }
    private void HandleBestTimeLap(float bestTimeLap)
    {
        var bestTime = System.TimeSpan.FromSeconds(bestTimeLap);
        if (bestLapTimeText != null)
        {
            bestLapTimeText.text = string.Format("{0:00}:{1:00}.{2:00}", bestTime.Minutes, bestTime.Seconds, bestTime.Milliseconds);
        }
    }
    private void HandleSpeed(float speedKmH)
    {
        if (speedText != null)
            speedText.text = Mathf.RoundToInt(speedKmH).ToString();
    }

    private void HandleNitro(float minTank, float maxTank)
    {
        if (nitroSlider != null)
        {
            nitroSlider.minValue = minTank;
            nitroSlider.maxValue = maxTank;
        }

    }
    private void HandleRaceStarted()
    {
        if (countDownNum != null) countDownNum.gameObject.SetActive(false);
        if (countDownTxt != null) countDownTxt.gameObject.SetActive(true); //Show "Go!"
    }
    private void HandleTimeCountDown(float timeCountdown)
    {
        if (countDownNum != null)
        {
            countDownNum.gameObject.SetActive(true);
            countDownNum.text = Mathf.RoundToInt(timeCountdown).ToString();
            if (timeCountdown <= 0) HandleRaceStarted();
        }
    }

    private void HandleTimeLeft(float timeLeft)
    {
        if (countDownTimeLeft != null)
        {
            countDownTimeLeft.gameObject.SetActive(true);
            countDownTimeLeft.text = "Timeleft: " + Mathf.RoundToInt(timeLeft).ToString();
        }

    }

    private void OnDestroy()
    {
        // UNSUBSCRIBE KHI OBJECT BỊ DESTROY (chuyển scene, reload, quit,...)
        GameEvent.OnLap -= HandleLap;
        GameEvent.OnPosition -= HandlePosition;

        GameEvent.OnTimeLap -= HandleTimeLap;
        GameEvent.OnBestTimeLap -= HandleBestTimeLap;

        GameEvent.OnSpeed -= HandleSpeed;
        GameEvent.OnNitro -= HandleNitro;
        
        GameEvent.OnTimeCountDownStartRace -= HandleTimeCountDown;
        GameEvent.OnTimeLeftForFinishRace -= HandleTimeLeft;
    }
    //Button
    public void OnClickPause()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PauseBtn();
        }
    }
}
