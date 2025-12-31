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
    }

    public override void Hide()
    {
        base.Hide();
    }

   
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
