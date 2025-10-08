using UnityEngine;
using UnityEngine.EventSystems;

public class CameraTouchController : MonoBehaviour
{
    public Transform cameraTransform;
    public float sensitivity = 0.2f;
    public float verticalClamp = 80f;
    public bool smoothCamera = true;
    public float smoothSpeed = 10f;
    private Vector2 lastTouchPos;
    private bool dragging = false;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float targetXRot = 0f;
    private float targetYRot = 0f;

    void Start()
    {
        Vector3 angles = cameraTransform.eulerAngles;
        xRotation = targetXRot = angles.x;
        yRotation = targetYRot = angles.y;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPos = touch.position;
                    dragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && dragging)
                {
                    Vector2 delta = touch.position - lastTouchPos;
                    lastTouchPos = touch.position;
                    targetXRot -= delta.y * sensitivity;
                    targetYRot += delta.x * sensitivity;
                    targetXRot = Mathf.Clamp(targetXRot, -verticalClamp, verticalClamp);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    dragging = false;
                }
            }
        }

        // Smooth or instant camera
        if (smoothCamera)
        {
            xRotation = Mathf.LerpAngle(xRotation, targetXRot, Time.deltaTime * smoothSpeed);
            yRotation = Mathf.LerpAngle(yRotation, targetYRot, Time.deltaTime * smoothSpeed);
        }
        else
        {
            xRotation = targetXRot;
            yRotation = targetYRot;
        }
        cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
