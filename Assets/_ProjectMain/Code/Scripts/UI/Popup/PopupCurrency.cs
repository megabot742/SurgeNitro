using TMPro;
using UnityEngine;

public class PopupCurrency : BasePopup
{
    [SerializeField] TMP_Text coinTxt;
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
    public void OnClickGoHome()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.HideAllScreens();
            UIManager.Instance.ShowScreen<ScreenHome>();
        }
    }
}
