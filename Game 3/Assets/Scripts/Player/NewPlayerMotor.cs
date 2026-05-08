using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMotor : MonoBehaviour
{
    public PlayerInput inputActions;

    [Header("General")]
    public float groundSpeed = 5.0f;
    public float jumpForce = 1.0f;
    private float jumpGraceTimer = 0f;
    private const float JumpGraceDuration = 0.1f;

    [Header("Physics Movement")]
    public float maxGroundSpeed = 6f;
    public float maxAirSpeed = 6f;
    public float groundAccelerationForce = 1f;
    public float airAccelerationForce = 10f;
    public float groundDecelerationForce = 50f;
    public float airDecelerationForce = 2f;
    public float maxVelocity = 10f;

    [Header("Debug")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float totalSpeed;

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

    [Header("Pulse Gun")]
    [Tooltip("Strength of impulse applied to player when firing the Pulse Gun")]
    public float pulseForce = 8f;

    [Header("Wall Slide")]
    // Max downward speed while sliding with gravity (if we ever get around to adding itfd)
    //public float wallSlideSpeed = 2.5f;
    public float wallSlideDeceleration = 3f;
    private CrosshairController crosshair;

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(6f, 10f);
    public float wallJumpGraceDistance = 0.1f; // Optional “near wall” buffer

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
    private bool isAgainstWallE = false;
    private bool isAgainstWallW = false;
    private bool isAgainstCeiling = false;
    private bool isAgainstWall = false;
    // Context for which surface was touched most recently
    private bool wasOnGround = false;
    private bool wasAgainstCeiling = false;
    private bool wasAgainstWallW = false;
    private bool wasAgainstWallE = false;

    // Velocity is input direction
    // Inertia is actual movement direction, smoothed to Velocity over time
    private Vector2 velocity;
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

        Vector2 speed = rb.linearVelocity;

        if (CanWallJump())
        {
            float direction = isAgainstWallE ? -1f : 1f;

            speed.x = direction * wallJumpForce.x;
            speed.y = wallJumpForce.y;

            rb.linearVelocity = speed;

            jumpGraceTimer = JumpGraceDuration;
            return;
        }
        else if (isOnGround)
        {
            speed.y = jumpForce;
            rb.linearVelocity = speed;

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
        if (isOnGround && isAgainstCeiling)
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

        // Overwrite inertia with AimDirection from crosshair so there's an immediate movement change
        // AimDirection already points from player to crosshair, so just invert it
        rb.AddForce(
            -crosshair.AimDirection.normalized * pulseForce,
            ForceMode2D.Impulse
        );
        hasPulseGunCharge = false;
        jumpGraceTimer = JumpGraceDuration;
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

        // Debug speed tracking
        horizontalSpeed = rb.linearVelocity.x;
        verticalSpeed = rb.linearVelocity.y;
        totalSpeed = rb.linearVelocity.magnitude;
    }

    void FixedUpdate()
    {
        ApplyPhysicsMovement();
    }

    private void UpdateCollisionState()
    {
        // Last frame's state is saved first
        wasOnGround = isOnGround;
        wasAgainstCeiling = isAgainstCeiling;
        wasAgainstWallE = isAgainstWallE;
        wasAgainstWallW = isAgainstWallW;

        // Check collisions
        isOnGround = (CheckCollision(groundCheckTransform) || CheckCollision(groundCheckL) || CheckCollision(groundCheckR));
        isAgainstCeiling = (CheckCollision(ceilingCheckTransform) || CheckCollision(ceilingCheckL) || CheckCollision(ceilingCheckR));
        isAgainstWallE = (CheckCollision(eastWallCheckTransform) || CheckCollision(eastWallCheckU) || CheckCollision(eastWallCheckD));
        isAgainstWallW = (CheckCollision(westWallCheckTransform) || CheckCollision(westWallCheckU) || CheckCollision(westWallCheckD));

        isAgainstWall = isAgainstWallE || isAgainstWallW;

        // Pulse Gun Charge refresh
        if (isOnGround)
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
    
    private bool IsPressingIntoWall()
    {
        // Pushing into right wall
        if (isAgainstWallE && velocity.x > 0) return true;
        // Pushing into left wall
        if (isAgainstWallW && velocity.x < 0) return true;
        return false;
    }

    private bool CanWallJump()
    {
        if (!isAgainstWall) return false;
        if (isOnGround) return false;
        return true;
    }

    private void UpdateActiveSurface()
    {
        // Detect colliding with new surface type
        bool newGround = isOnGround && !wasOnGround;

        // Transition to new surface
        // If colliding with a new wall and ground/ceiling at the same time,
        // ground/ceiling take priority
        if (newGround)
        {
            activeSurface = Surface.GROUND;
        }

        if (!isOnGround)
        {
            activeSurface = Surface.AIRBORNE;
        }

        // If an active surface is left, fall back to a surface that is
        // still being touched or airborne if none
        if (activeSurface == Surface.GROUND && !isOnGround)
        {
            FallbackSurface();
        }
        if (activeSurface == Surface.CEILING && !isAgainstCeiling)
        {
            FallbackSurface();
        }
        if (activeSurface == Surface.WALL && !isAgainstWall)
        {
            FallbackSurface();
        }
    
    }

    private void FallbackSurface()
    {
        if (isOnGround)
        {
            activeSurface = Surface.GROUND;
        }
        else
        {
            activeSurface = Surface.AIRBORNE;
        }
    }

    private void ApplyPhysicsMovement()
    {
        // Ensures vertical input is ignored for horizontalmovement speed
        float velocityX;

        if(velocity.x != 0)
        {
            float xSign = Mathf.Sign(velocity.x);
            velocityX = xSign;
        }
        else
        {
            velocityX = 0;
        }

        Vector2 currentVelocity = rb.linearVelocity;
        bool grounded = activeSurface == Surface.GROUND;

        bool isWallSliding = !grounded && isAgainstWall && IsPressingIntoWall();
        if (isWallSliding)
        {
            // Clamp fall speed (classic wall slide behavior)
            //if (currentVelocity.y < -wallSlideSpeed)
            //    currentVelocity.y = -wallSlideSpeed;

            // Gradually reduce vertical velocity toward 0 if moving upward or barely falling
            currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, 0f, wallSlideDeceleration * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentVelocity.y);
        }

        float forceX;

        // Grounded movement
        if (grounded)
        {
            float targetX = velocityX * maxGroundSpeed;
            float deltaX = targetX - currentVelocity.x;

            bool hasHorizontalInput = Mathf.Abs(velocity.x) > 0.01f;

            float forceAmount = hasHorizontalInput ? groundAccelerationForce : groundDecelerationForce;

            forceX = deltaX * forceAmount;

            rb.AddForce(new Vector2(forceX, 0f));

            // Prevent tiny drifting on ground
            if (!hasHorizontalInput && Mathf.Abs(rb.linearVelocity.x) < 0.05f)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        // Airborne movement
        else
        {
            float targetX = velocityX * maxAirSpeed;
            float deltaX = targetX - currentVelocity.x;

            bool hasHorizontalInput = Mathf.Abs(velocity.x) > 0.01f;

            float forceAmount = hasHorizontalInput ? airAccelerationForce : airDecelerationForce;

            forceX = deltaX * forceAmount;

            rb.AddForce(new Vector2(forceX, 0f));
        }
    }

    private void OnDrawGizmosSelected()
    {
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
