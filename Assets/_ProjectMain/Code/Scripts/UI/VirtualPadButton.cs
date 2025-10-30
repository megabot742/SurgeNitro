using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _pressedColor = Color.red;

        private Image _image;

        protected bool _pressed;

        public bool Pressed => _pressed;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = _normalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            _image.color = _pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            _image.color = _normalColor;
        }
}
