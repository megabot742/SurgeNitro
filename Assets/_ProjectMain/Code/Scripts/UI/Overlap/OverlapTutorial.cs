using UnityEngine;

public class OverlapTutorial : BaseOverlap
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
    public void OnClickConfirm()
    {
        this.Hide();
    }
}
