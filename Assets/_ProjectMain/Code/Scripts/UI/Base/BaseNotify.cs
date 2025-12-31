using UnityEngine;

public class BaseNotify : BaseUIElement
{
     public override void Init() //Initializes the notify and sets its UI type.
    {
        base.Init();
        this.uiType = UIType.Notify;
    }

    public override void Hide() //Hides the notify from view.
    {
        base.Hide();
    }

    public override void Show(object data) //Shows the notify with optional data.
    {
        base.Show(data);
    }

    public override void OnClickedBackButton() //Handles back button click events.
    {
        base.OnClickedBackButton(); //Override in derived classes to implement specific back button behavior.
    }
}
