using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    // Crosshair child game object to position within ring
    public Transform crosshairTransform;

    // Radius of the ring to be displayed and to confine the crosshair to
    public float ringRadius = 1.75f;

    // Crosshair sensitivity
    public float sensitivity = 0.015f;

    // Angle of crosshair on ring, in radians, defaults to pointing East (0)
    private Vector2 crosshairOffset = Vector2.zero;

    // Normalized direction from player to crosshair, used to determine pulse gun aim
    // Defaults to straight down when crosshair is perfectly centered
    public Vector2 AimDirection
    {
        get
        {
            if (crosshairOffset.sqrMagnitude > 0.001f)
            {
                return crosshairOffset.normalized;
            }
            else
            {
                return Vector2.down;
            }
        }
    }

    // World-space position of crosshair
    public Vector2 CrosshairWorldPosition => crosshairTransform != null
        ? (Vector2)crosshairTransform.position
        : (Vector2)transform.position;

    private void Awake()
    {
        // Hide/lock cursor (mouse movement still readable)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
       if (crosshairTransform == null)
       {
            return;
       }

        ProcessMouseDelta();
        PositionCrosshair();
    }

    private void ProcessMouseDelta()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (mouseDelta.sqrMagnitude < 0.001f)
        {
            return;
        }

        // Process mouse delta in world-space, clamp to within ring
        crosshairOffset += mouseDelta * sensitivity;
        crosshairOffset = Vector2.ClampMagnitude(crosshairOffset, ringRadius);
    }

    private void PositionCrosshair()
    {
        // Local position keeps crosshair relative to player
        // rather than allowing camera movement to change crosshair position
        // through being a child object of the player
        crosshairTransform.localPosition = crosshairOffset;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
