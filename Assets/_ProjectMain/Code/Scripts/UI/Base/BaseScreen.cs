using UnityEngine;

public class BaseScreen : BaseUIElement
{
    public override void Init() //Initializes the screen and sets its UI type.
    {
        base.Init();
        this.uiType = UIType.Screen;
    }

    public override void Hide() //Hides the screen from view.
    {
        base.Hide();
    }

    public override void Show(object data) //Shows the screen with optional data.
    {
        base.Show(data);
    }

    public override void OnClickedBackButton() //Handles back button click events.
    {
        base.OnClickedBackButton(); //Override in derived classes to implement specific back button behavior.
    }
}
