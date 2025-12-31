using UnityEngine;

public class BasePopup : BaseUIElement
{
    public override void Init() //Initializes the popup and sets its UI type.
    {
        base.Init();
        this.uiType = UIType.Popup;
    }

    public override void Hide() //Hides the popup from view.
    {
        base.Hide();
    }

    public override void Show(object data) //Shows the popup with optional data.
    {
        base.Show(data);
    }

    public override void OnClickedBackButton() //Handles back button click events.
    {
        base.OnClickedBackButton(); //Override in derived classes to implement specific back button behavior (e.g., close popup).
    }
}
