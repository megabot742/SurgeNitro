using UnityEngine;

public class ScreenCarUpgrade : BaseScreen
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
    //Button
    public void OnClickGoBack()
    {
        if(UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<ScreenCarInfo>();
        }
        this.Hide();
    }
}
