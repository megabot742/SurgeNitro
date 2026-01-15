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
        GameEvent.OnCoinChanged += UpdateCoinText;
        if(PlayerManager.HasInstance)
        {
            UpdateCoinText(PlayerManager.Instance.GetCoin());
        }
    }
    public override void Hide()
    {
        base.Hide();
        GameEvent.OnCoinChanged -= UpdateCoinText;
    }
    public override void Clear()
    {
        coinTxt.text = "";
    }
    private void UpdateCoinText(long newCoin)
    {
        if (coinTxt != null)
        {
            coinTxt.text = newCoin.ToString("N0");
        }
    }
    //Button
    public void OnClickGoHome()
    {
        if (UIManager.HasInstance && UIEventManager.HasInstance)
        {
            UIManager.Instance.HideAllScreens();
            UIManager.Instance.ShowScreen<ScreenHome>();
            UIEventManager.Instance.currentFilterIndexGarage = 0; //reset to default
            UIEventManager.Instance.currentFilterIndexShop = 0; //reset to default
        }
    }
}
