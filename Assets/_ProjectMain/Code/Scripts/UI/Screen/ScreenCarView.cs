using UnityEngine;

public class ScreenCarView : BaseScreen
{
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        if (CameraManager.HasInstance)
        {
            CameraManager.Instance.SwitchMenuCamera(MenuCameraType.CarView);
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
    //Button
    public void OnClickGoBack()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
}
