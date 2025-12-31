using UnityEngine;

public class BaseOverlap : BaseUIElement
{
    public override void Init() //Initializes the overlap and sets its UI type.
    {
        base.Init();
        this.uiType = UIType.Overlap;
    }

    public override void Hide() //Hides the overlap from view.
    {
        base.Hide();
    }

    public override void Show(object data) //Shows the overlap with optional data.
    {
        base.Show(data);
    }

    public override void OnClickedBackButton() //Handles back button click events.
    {
        base.OnClickedBackButton(); //Override in derived classes to implement specific back button behavior.
    }
}
