using UnityEngine;

public class OverlapRestartGame : BaseOverlap
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
    public void OnClickExitGame()
    {
        if(UIEventManager.HasInstance && PlayerManager.HasInstance)
        {
            UIEventManager.Instance.QuitGameBtn();
            PlayerManager.Instance.ResetPlayerPrefs();
        }
        this.Hide();
    }
}
