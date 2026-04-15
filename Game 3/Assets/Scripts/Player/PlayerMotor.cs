using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotor : MonoBehaviour
{
    public PlayerInput inputActions;

    [Header("General")]
    public float groundSpeed = 5.0f;
    public float jumpForce = 1.0f;

    [Header("Surface Checks")]
    public Transform groundCheckTransform;
    public Transform eastWallCheckTransform;
    public Transform westWallCheckTransform;
    public Transform ceilingCheckTransform;
    public float collisionCheckRadius;
    public LayerMask groundLayer;

    [Header("Inertia")]
    [Tooltip("How quickly inertia catches up to the input direction while on a surface. Higher = snappier.")]
    public float groundAcceleration = 20f;
    [Tooltip("How quickly inertia catches up to the input direction while airborne. Keep low to mimic zero gravity.")]
    public float aerialAcceleration = 3f;

    // Various bools to check if player is colliding with solid surfaces
    // isOnWall = isOnWallE || isOnWallW
    // isColliding is true if any of these are true
    private bool isOnGround = false;
    private bool isOnWallE = false;
    private bool isOnWallW = false;
    private bool isOnCeiling = false;
    private bool isOnWall = false;
    private bool isColliding = false;
    // Velocity is input direction
    // Inertia is actual movement direction, smoothed to Velocity over time
    private Vector2 velocity, inertia = Vector2.zero;
    private Rigidbody2D rb;

    void Awake()
    {
        inputActions = new PlayerInput();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += Movement;
        inputActions.Player.Jump.performed += Jump;

        inputActions.Player.Move.canceled += Movement;
        inputActions.Player.Jump.canceled += Jump;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Move.performed -= Movement;
        inputActions.Player.Jump.performed -= Jump;
    }

    void Movement(InputAction.CallbackContext ctx)
    {
        velocity = ctx.ReadValue<Vector2>();
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Jumps write directly into inertia. Because Update only nudges inertia
        // toward input rather than overwriting it, these won't be overridden by
        // WASD input
        if (isOnGround)
        {
            inertia.y = jumpForce;
        }
        else if (isOnCeiling)
        {
            inertia.y = -jumpForce;
        }
        else if (isOnWallW)
        {
            inertia.x = jumpForce;
        }
        else if (isOnWallE)
        {
            inertia.x = -jumpForce;
        }
    }

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        // Check bools
        isOnGround = Physics2D.OverlapCircle(groundCheckTransform.position, collisionCheckRadius, groundLayer);
        isOnCeiling = Physics2D.OverlapCircle(ceilingCheckTransform.position, collisionCheckRadius, groundLayer);
        isOnWallE = Physics2D.OverlapCircle(eastWallCheckTransform.position, collisionCheckRadius, groundLayer);
        isOnWallW = Physics2D.OverlapCircle(westWallCheckTransform.position, collisionCheckRadius, groundLayer);
        isOnWall = (isOnWallE || isOnWallW);
        isColliding = (isOnGround || isOnCeiling || isOnWall);

        // Inertia steering
        // On each surface, only the axis the surface controls is steered toward input
        // The other axis is left alone, so jump impulses on the free axis decay naturally
        // rather than being zeroed out
        if (isOnGround || isOnCeiling)
        {
            // Ground and ceiling - steer inertia.x toward input
            float targetX = velocity.x * groundSpeed;
            inertia.x = Mathf.MoveTowards(inertia.x, targetX, groundAcceleration * Time.deltaTime);
        }
        else if (isOnWall)
        {
            // Walls - steer inertia.y toward input
            float targetY = velocity.y * groundSpeed;
            inertia.y = Mathf.MoveTowards(inertia.y, targetY, groundAcceleration * Time.deltaTime);
        }
        else
        {
            // Airborne - reworked so that velocity is 100% preserved when no input is held
            // When input is held, THEN process inertia, otherwise don't do anything
            if (velocity.SqrMagnitude() >= 0.01f)
            {
                Vector2 target = velocity * groundSpeed;
                inertia = Vector2.MoveTowards(inertia, target, aerialAcceleration * Time.deltaTime);
            }
        }

        rb.linearVelocity = inertia;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(ceilingCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(eastWallCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(westWallCheckTransform.position, collisionCheckRadius);
    }
}
