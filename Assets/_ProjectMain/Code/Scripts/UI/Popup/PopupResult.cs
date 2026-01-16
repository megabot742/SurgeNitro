using TMPro;
using UnityEngine;

public class PopupResult : BasePopup
{
    public TMP_Text posNumberTxt;
    public TMP_Text bestTimeTxt;
    public TMP_Text unclockTrackTxt;
    [Header("Calculate Coin")]
    [SerializeField] TMP_Text baseRewardText;
    [SerializeField] TMP_Text bonusRewardText;
    [SerializeField] TMP_Text totalRewardText;
    private RaceResultData currentData;
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        // Subscribe to events
        GameEvent.OnRaceFinished += HandleRaceFinished;  // Subscribe
        if (data is RaceResultData result)
        {
            currentData = result;

            // Vị trí
            posNumberTxt.text = RaceManager.Instance.GetOrdinalText(result.position);

            // Thời gian tốt nhất
            var time = System.TimeSpan.FromSeconds(result.bestLapTime);
            bestTimeTxt.text = string.Format("{0:00}:{1:00}.{2:000}", 
                time.Minutes, time.Seconds, time.Milliseconds);

            // Hiển thị thưởng chi tiết
            if (baseRewardText) 
                baseRewardText.text = $"+{result.baseReward:N0}";

            if (bonusRewardText) 
                bonusRewardText.text = $"+{result.randomBonus:N0}";

            if (totalRewardText)
            {
                totalRewardText.text = $"+{result.totalReward:N0}";
            }
        }
    }

    public override void Hide()
    {
        base.Hide();
        // Unsubscribe to avoid memory leak
        GameEvent.OnRaceFinished -= HandleRaceFinished;
    }
    public override void Clear()
    {
        if (posNumberTxt) posNumberTxt.text = "N/A";
        if (bestTimeTxt) bestTimeTxt.text = "00:00.000";
        if (unclockTrackTxt) unclockTrackTxt.text = "New track unlock";
    }
    // Event handling function
    private void HandleRaceFinished(int position, float bestTime)
    {
        if (RaceManager.HasInstance)
        {
            posNumberTxt.text = RaceManager.Instance.GetOrdinalText(position);

            var time = System.TimeSpan.FromSeconds(bestTime);
            bestTimeTxt.text = string.Format("{0:00}:{1:00}.{2:00}", time.Minutes, time.Seconds, time.Milliseconds);
        }
    }
   //Button
    public void OnClickRestart()
    {
        if (UIEventManager.HasInstance && UIManager.HasInstance)
        {
            UIEventManager.Instance.RestartBtn();
            UIManager.Instance.ShowScreen<ScreenGame>();
        }
        this.Hide();
    }
    public void OnClickGarage()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.BackGarageBtn(); //Return scene Garage
        }
        this.Hide();
    }
}
