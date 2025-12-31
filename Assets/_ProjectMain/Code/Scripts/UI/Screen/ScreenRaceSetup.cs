using UnityEngine;

public class ScreenRaceSetup : BaseScreen
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
    public void OnClickGoGarage()
    {
        if (UIEventManager.HasInstance)
        {
            var data = new CarInfoData { Mode = CarInfoMode.SelectForRace };
            UIEventManager.Instance.GarageBtn(data);
        }
        this.Hide();
    }

    public void OnClickStartGame()
    {
        if (UIEventManager.HasInstance)
        {
           UIEventManager.Instance.RaceBtn();
        }
        this.Hide();
    }
    public void OnClickGoBack()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
}
