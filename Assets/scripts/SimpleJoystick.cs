using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 1f;
    public float deadZone = 0.1f;
    public Vector2 Direction { get; private set; }

    private Vector2 input = Vector2.zero;

    // Static flag to indicate if any joystick is being touched
    public static bool IsJoystickActive = false;

    public void OnDrag(PointerEventData eventData)
    {
    IsJoystickActive = true;
    Vector2 pos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);
    pos = pos / background.sizeDelta * 2f;
    input = Vector2.ClampMagnitude(pos, 1f);
    handle.anchoredPosition = input * (background.sizeDelta.x / 2f) * handleRange;
    Direction = (input.magnitude > deadZone) ? input : Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    IsJoystickActive = true;
    OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    input = Vector2.zero;
    handle.anchoredPosition = Vector2.zero;
    Direction = Vector2.zero;
    IsJoystickActive = false;
    }
}
