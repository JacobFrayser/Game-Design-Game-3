using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    // Crosshair child game object to position within ring
    public Transform crosshairTransform;

    // Renderer on crosshair sprite. Used to disable the crosshair visual when on a surface using Default movement style
    public Renderer crosshairRenderer;

    // Renderer on ring line. Used to disable the ring visual when on a surface using Default movement style
    public Renderer ringRenderer;

    // Radius of the ring to be displayed and to confine the crosshair to
    public float ringRadius = 1.75f;

    // Crosshair sensitivity
    public float sensitivity = 0.015f;

    // Angle of crosshair on ring
    private Vector2 crosshairOffset = Vector2.zero;

    private PlayerMotor motor;

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

    private void Start()
    {
        // Get player motor to check whether player is grounded
        motor = GetComponentInChildren<PlayerMotor>();
    }

    void Update()
    {
       if (crosshairTransform == null)
       {
            return;
       }

        GameSettings.MovementStyle style = GetStyle();

        UpdateVisibility(style);

        switch (style)
        {
            case GameSettings.MovementStyle.DEFAULT:
                UpdateDefault();
                break;
            case GameSettings.MovementStyle.PRECISE:
                UpdatePrecise();
                break;
        }

        crosshairTransform.localPosition = crosshairOffset;
    }

    private void UpdateVisibility(GameSettings.MovementStyle style)
    {
        bool visible = true;

        // Using Default mode, hide crosshair and ring while on any surface
        if (style == GameSettings.MovementStyle.DEFAULT && motor != null)
        {
            visible = motor.IsAirborne;
        }

        if (crosshairRenderer != null)
        {
            crosshairRenderer.enabled = visible;
        }
        if (ringRenderer != null)
        {
            ringRenderer.enabled = visible;
        }
    }

    private void UpdatePrecise()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (mouseDelta.sqrMagnitude < 0.001f)
        {
            return;
        }

        // Default sensSetting to 1.0x
        float sensSetting = 1.0f;

        // Grab Sensitivity setting if settings are present
        if (GameSettings.Instance != null)
        {
            sensSetting = GameSettings.Instance.mouseSens;
        }

        // Apply sensSetting as multiplier, process mouse delta in world-space, clamp to within ring
        crosshairOffset += mouseDelta * sensitivity * sensSetting;
        crosshairOffset = Vector2.ClampMagnitude(crosshairOffset, ringRadius);
    }

    private void UpdateDefault()
    {
        // Don't update crosshair position when not airborne
        if (motor != null && !motor.IsAirborne)
        {
            return;
        }

        // Get input keys and add/subtract 1 from respective axis
        float x = 0f, y = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            y += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            y -= 1f;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            x -= 1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            x += 1f;
        }

        Vector2 input = new Vector2(x, y);

        // If no input given, return
        if (input.sqrMagnitude <= 0.01f )
        {
            return;
        }

        // Snap crosshair to ring perimeter
        crosshairOffset = input.normalized * ringRadius;
    }

    private GameSettings.MovementStyle GetStyle()
    {
        if (GameSettings.Instance != null)
        {
            return GameSettings.Instance.CurrentMovementStyle;
        }

        return GameSettings.MovementStyle.DEFAULT;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
