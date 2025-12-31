using UnityEngine;

public class ScreenShop : BaseScreen
{
    private CarInfoData shopData;
    public override void Init()
    {
        base.Init();
        
    }
    public override void Show(object data)
    {
        base.Show(data);
        shopData = data as CarInfoData;
    }

    public override void Hide()
    {
        base.Hide();
    }
    public void OnClickCarInfo()
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.CarInfoBtn(shopData);
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
