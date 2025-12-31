using UnityEngine;

public class ScreenGarage : BaseScreen
{
    private CarInfoData garageData;
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        garageData = data as CarInfoData; // Lưu lại chế độ
    }

    public override void Hide()
    {
        base.Hide();
    }
    public void OnClickCarInfo()
    {
       if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.CarInfoBtn(garageData);
        }
        this.Hide();
    }
    public void OnClickGoBack()
    {
        // if(UIManager.HasInstance)
        // {
        //     UIManager.Instance.ShowScreen<ScreenHome>();
        // }
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
}
