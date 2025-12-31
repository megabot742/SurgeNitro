using UnityEngine;

public class BaseUIElement : MonoBehaviour
{
    protected CanvasGroup canvasGroup; // CanvasGroup -> controlling visibility and raycast blocking.
    protected UIType uiType = UIType.Unknown; // Type of UI element -> Screen, Popup, Notify, or Overlap
    protected bool isHide; //Flag check -> UI Hidden
    private bool isInited; //Flag check -> UI Initialized.

    #region Public property to get
    public bool GetIsHide { get => isHide; }
    public CanvasGroup GetCanvasGroup { get => canvasGroup; }
    public bool GetIsInited { get => isInited; }
    public UIType GetUIType { get => uiType; }
    #endregion
    

    #region Function
    public virtual void Init() //Create CanvasGroup if need, activates GameObject, and hides it. Must be called before using UI element
    {
        this.isInited = true;
        if (!this.gameObject.GetComponent<CanvasGroup>()) // Add CanvasGroup component if it doesn't exist
        {
            this.gameObject.AddComponent<CanvasGroup>();
        }
        this.canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        this.gameObject.SetActive(true);
        // Start hidden
        Hide();
    }

    public virtual void Show(object data) //Shows the UI element by activating it and making it visible.
    {
        this.gameObject.SetActive(true);
        this.isHide = false;
        SetActiveGroupCanvas(true);
    }

    public virtual void Hide() //Hides the UI element by making it invisible and non-interactive.
    {
        Clear();
        this.isHide = true;
        SetActiveGroupCanvas(false);
    }

    public virtual void Clear() //Virtual method for clearing/resetting UI element state.
    {
        //Override in derived classes to implement cleanup logic.
    }

    private void SetActiveGroupCanvas(bool isAct) // Sets the CanvasGroup's alpha and raycast blocking based on active state.
    {
        if (GetCanvasGroup != null)
        {
            GetCanvasGroup.blocksRaycasts = isAct;
            GetCanvasGroup.alpha = isAct ? 1 : 0; //True to make visible and interactive, false to hide and disable interaction
        }
    }

    public virtual void OnClickedBackButton() //Virtual method for handling back button clicks.
    {
        Debug.Log("Click back");
    }
    #endregion
}
