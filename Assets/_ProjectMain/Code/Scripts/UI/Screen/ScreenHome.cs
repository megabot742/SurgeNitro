using UnityEngine;

public class ScreenHome : BaseScreen
{
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        //Camera Show
        if (CameraManager.HasInstance)
        {
            CameraManager.Instance.SwitchMenuCamera(MenuCameraType.Home);
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
    public void OnClickPlay()
    {
        if (UIManager.HasInstance)
        {
            UIEventManager.Instance.PlayBtn();
        }
        this.Hide();
    }
    public void OnClickGarage()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GarageBtn();
        }
        this.Hide();
    }
    public void OnClickShop()
    {
        if (UIManager.HasInstance)
        {
            UIEventManager.Instance.ShopBtn();
        }
        this.Hide();
    }
    public void OnClickToSetting()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.SettingBtn();
        }
        this.Hide();
    }
    public void OnClickExitGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.QuitGameBtn(); //Exit game
        }
        this.Hide();
    }
}
