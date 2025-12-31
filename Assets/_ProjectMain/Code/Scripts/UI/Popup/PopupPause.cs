using UnityEngine;

public class PopupPause : BasePopup
{
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

    public void OnClickResume()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.ResumeBtn();
        }
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
    public void OnClickClose()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.ResumeBtn()
 ;       }
    }
}
