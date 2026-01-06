using TMPro;
using UnityEngine;

public class PopupResult : BasePopup
{
    public TMP_Text posNumberTxt;
    public TMP_Text bestTimeTxt;
    public TMP_Text unclockTrackTxt;
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        // Subscribe to events
        GameEvent.OnRaceFinished += HandleRaceFinished;  // Subscribe
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
