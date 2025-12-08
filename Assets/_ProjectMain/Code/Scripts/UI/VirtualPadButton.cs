using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = Color.red;

    private Image image;

    protected bool pressed;

    public bool Pressed => pressed;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        image.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        image.color = normalColor;
    }
}
