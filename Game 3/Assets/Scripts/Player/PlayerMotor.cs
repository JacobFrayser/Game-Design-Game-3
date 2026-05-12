using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotor : MonoBehaviour
{
    public PlayerInput inputActions;

    [Header("General")]
    public float groundSpeed = 5.0f;
    public float jumpForce = 6.5f;
    private float jumpGraceTimer = 0f;
    private const float JumpGraceDuration = 0.1f;

    [Header("Surface Checks")]
    public Transform groundCheckTransform;
    public Transform eastWallCheckTransform;
    public Transform westWallCheckTransform;
    public Transform ceilingCheckTransform;
    // Corner checks prevent getting stuck on corner geometry
    public Transform groundCheckL;
    public Transform groundCheckR;
    public Transform ceilingCheckL;
    public Transform ceilingCheckR;
    public Transform eastWallCheckU;
    public Transform eastWallCheckD;
    public Transform westWallCheckU;
    public Transform westWallCheckD;
    public float collisionCheckRadius = 0.035f;
    public LayerMask groundLayer;
    public bool IsAirborne => activeSurface == Surface.AIRBORNE;

    [Header("Inertia")]
    [Tooltip("How quickly inertia catches up to the input direction while on a surface. Higher = snappier.")]
    public float groundAcceleration = 20f;
    [Tooltip("How quickly inertia catches up to the input direction while airborne. Keep low to mimic zero gravity.")]
    public float aerialAcceleration = 3f;

    [Header("Pulse Gun")]
    [Tooltip("Strength of impulse applied to player when firing the Pulse Gun")]
    public float pulseForce = 8f;
    private CrosshairController crosshair;

    private enum Surface
    {
        AIRBORNE,
        GROUND,
        CEILING,
        WALL
    }
    // Current surface the player is snapped to
    private Surface activeSurface = Surface.AIRBORNE;

    // Various bools to check if player is colliding with solid surfaces
    // isOnWall = isOnWallE || isOnWallW
    // isColliding is true if any of these are true
    private bool isOnGround = false;
    private bool isOnWallE = false;
    private bool isOnWallW = false;
    private bool isOnCeiling = false;
    private bool isOnWall = false;
    private bool isColliding = false;
    // Context for which surface was touched most recently
    private bool wasOnGround = false;
    private bool wasOnCeiling = false;
    private bool wasOnWallW = false;
    private bool wasOnWallE = false;

    // Velocity is input direction
    // Inertia is actual movement direction, smoothed to Velocity over time
    private Vector2 velocity, inertia = Vector2.zero;
    private Rigidbody2D rb;

    // Whether player has a Pulse Gun charge
    private bool hasPulseGunCharge = true;

    // Main camera reference for world-space mouse position
    private Camera mainCamera;

    void Awake()
    {
        inputActions = new PlayerInput();
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += Movement;
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.Fire.performed += Fire;

        inputActions.Player.Move.canceled += Movement;
        inputActions.Player.Jump.canceled += Jump;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Move.performed -= Movement;
        inputActions.Player.Jump.performed -= Jump;
        inputActions.Player.Fire.performed -= Fire;
    }

    void Movement(InputAction.CallbackContext ctx)
    {
        velocity = ctx.ReadValue<Vector2>();
        Debug.Log($"<Player Motor> Movement callback fired: {velocity}");
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
            jumpGraceTimer = JumpGraceDuration;
        }
        else if (isOnCeiling)
        {
            inertia.y = -jumpForce;
            jumpGraceTimer = JumpGraceDuration;
        }
        else if (isOnWallW)
        {
            inertia.x = jumpForce;
            jumpGraceTimer = JumpGraceDuration;
        }
        else if (isOnWallE)
        {
            inertia.x = -jumpForce;
            jumpGraceTimer = JumpGraceDuration;
        }
    }

    void Fire(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        // If no pulse charge, return
        if (!hasPulseGunCharge)
        {
            return;
        }

        // If sandwiched between two surfaces, don't fire
        if ((isOnGround && isOnCeiling) || (isOnWallW && isOnWallE))
        {
            return;
        }

        // If on a surface and DEFAULT style is active, return
        if (!IsAirborne && GameSettings.Instance.CurrentMovementStyle == GameSettings.MovementStyle.DEFAULT)
        {
            return;
        }

        if (crosshair == null)
        {
            Debug.LogError("<Player Motor> No CrosshairController found on parent player!");
            return;
        }

        // Return if using Space under PRECISE style
        if (ctx.control.path.Contains("space") && GameSettings.Instance?.CurrentMovementStyle == GameSettings.MovementStyle.PRECISE)
        {
            return;
        }

        // Return if using Left Click under DEFAULT style
        if (ctx.control.path.Contains("leftButton") && GameSettings.Instance?.CurrentMovementStyle == GameSettings.MovementStyle.DEFAULT)
        {
            return;
        }

        // Overwrite inertia with AimDirection from crosshair so there's an immediate movement change
        // AimDirection already points from player to crosshair, so just invert it
        inertia = -crosshair.AimDirection * pulseForce;
        hasPulseGunCharge = false;
        jumpGraceTimer = JumpGraceDuration;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        velocity = newVelocity;
        inertia = newVelocity;
    }

    public void RefreshPulseCharge()
    {
        hasPulseGunCharge = true;
    }

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        crosshair = GetComponentInParent<CrosshairController>();
    }

    void Update()
    {
        // Tick down grace period for jump
        if (jumpGraceTimer > 0f)
        {
            jumpGraceTimer -= Time.deltaTime;
        }

        // Check bools
        UpdateCollisionState();
        UpdateActiveSurface();
        ApplyInertia();

        rb.linearVelocity = inertia;
    }

    private void UpdateCollisionState()
    {
        // Last frame's state is saved first
        wasOnGround = isOnGround;
        wasOnCeiling = isOnCeiling;
        wasOnWallE = isOnWallE;
        wasOnWallW = isOnWallW;

        // Check collisions
        isOnGround = (CheckCollision(groundCheckTransform) || CheckCollision(groundCheckL) || CheckCollision(groundCheckR));
        isOnCeiling = (CheckCollision(ceilingCheckTransform) || CheckCollision(ceilingCheckL) || CheckCollision(ceilingCheckR));
        isOnWallE = (CheckCollision(eastWallCheckTransform) || CheckCollision(eastWallCheckU) || CheckCollision(eastWallCheckD));
        isOnWallW = (CheckCollision(westWallCheckTransform) || CheckCollision(westWallCheckU) || CheckCollision(westWallCheckD));

        isOnWall = isOnWallE || isOnWallW;
        isColliding = isOnWall || isOnGround || isOnCeiling;

        // Pulse Gun Charge refresh
        if (isColliding)
        {
            hasPulseGunCharge = true;
        }
    }

    private bool CheckCollision(Transform t)
    {
        if (t == null)
        {
            return false;
        }
        return Physics2D.OverlapCircle(t.position, collisionCheckRadius, groundLayer);
    }

    private void UpdateActiveSurface()
    {
        // Detect colliding with new surface type
        bool newGround = isOnGround && !wasOnGround;
        bool newCeiling = isOnCeiling && !wasOnCeiling;
        bool newWallE = isOnWallE && !wasOnWallE;
        bool newWallW = isOnWallW && !wasOnWallW;
        bool newWall = newWallE || newWallW;

        // Transition to new surface
        // If colliding with a new wall and ground/ceiling at the same time,
        // ground/ceiling take priority
        if (newGround)
        {
            activeSurface = Surface.GROUND;
        }
        else if (newCeiling)
        {
            activeSurface = Surface.CEILING;
        }
        else if (newWall)
        {
            activeSurface = Surface.WALL;
        }

        if (!isColliding)
        {
            activeSurface = Surface.AIRBORNE;
        }

        // If an active surface is left, fall back to a surface that is
        // still being touched or airborne if none
        if (activeSurface == Surface.GROUND && !isOnGround)
        {
            FallbackSurface();
        }
        if (activeSurface == Surface.CEILING && !isOnCeiling)
        {
            FallbackSurface();
        }
        if (activeSurface == Surface.WALL && !isOnWall)
        {
            FallbackSurface();
        }
    }

    private void FallbackSurface()
    {
        // Used to fix bugs with surfaces being touched but not registered
        if (isOnGround)
        {
            activeSurface = Surface.GROUND;
        }
        else if (isOnCeiling)
        {
            activeSurface = Surface.CEILING;
        }
        else if (isOnWall)
        {
            activeSurface = Surface.WALL;
        }
        else
        {
            activeSurface = Surface.AIRBORNE;
        }
    }

    private void ApplyInertia()
    {
        switch (activeSurface)
        {
            case Surface.GROUND:
            case Surface.CEILING:
                // Horizontal
                inertia.x = Mathf.MoveTowards(inertia.x, velocity.x * groundSpeed, groundAcceleration * Time.deltaTime);
                if (jumpGraceTimer <= 0f) inertia.y = 0f;
                break;

            case Surface.WALL:
                // Vertical
                inertia.y = Mathf.MoveTowards(inertia.y, velocity.y * groundSpeed, groundAcceleration * Time.deltaTime);
                if (jumpGraceTimer <= 0f) inertia.x = 0f;
                break;

            case Surface.AIRBORNE:
                bool preciseMode = (GameSettings.Instance.CurrentMovementStyle == GameSettings.MovementStyle.PRECISE);
                // Any axis, only move if input given
                if (preciseMode && velocity.sqrMagnitude >= 0.01f)
                {
                    Vector2 target = velocity * groundSpeed;
                    inertia = Vector2.MoveTowards(inertia, velocity * groundSpeed, aerialAcceleration * Time.deltaTime);
                }
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Purely to display the small collision circles on player object
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(ceilingCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(eastWallCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(westWallCheckTransform.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(groundCheckL.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(groundCheckR.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(ceilingCheckL.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(ceilingCheckR.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(eastWallCheckU.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(eastWallCheckD.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(westWallCheckU.position, collisionCheckRadius);
        Gizmos.DrawWireSphere(westWallCheckD.position, collisionCheckRadius);
    }
}
