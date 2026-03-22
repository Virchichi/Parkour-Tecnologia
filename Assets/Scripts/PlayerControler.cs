using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    float currentSpeed;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [SerializeField] float fallingHeight = 5f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = .2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    [Header("Wall Run")]
    [SerializeField] float wallRunSpeed = 6f;
    [SerializeField] float wallRunVerticalSpeed = 0f;
    [SerializeField] float wallRunStickForce = 4f;
    [SerializeField] float wallRunDuration = 1.5f;
    [SerializeField] float wallJumpForce = 7f;
    [SerializeField] float wallJumpHorizontalForce = 4f;

    [Header("Wall Run Cooldown")]
    [SerializeField] float wallRunCooldown = 0.6f;
    float wallRunCooldownTimer = 0f;

    [Header("Wall Jump Window")]
    [SerializeField] float recentWallJumpWindow = 0.25f;
    bool recentWallJump;
    float recentWallJumpTimer = 0f;

    [Header("Slide")]
    [SerializeField] float slideSpeed = 8f;
    [SerializeField] float slideDuration = 1f;

    [Header("Slide Collider")]
    [SerializeField] float slideHeight = 1f;
    [SerializeField] float normalHeight = 2f;

    Vector3 normalCenter;
    Vector3 slideCenter;

    float moveAmount;

    bool isGrounded;

    bool hasControl = true;

    float ySpeed;

    Quaternion targetRotation;

    float previousSpeed;

    bool isWallRunning;
    Vector3 wallRunNormal;
    Vector3 wallRunForward;
    float wallRunTimer;

    Vector3 externalVelocity;

    private InputSystem_Actions inputActions;

    [SerializeField] CameraController cameraController;

    Animator animator;

    CharacterController characterController;

    public CharacterController CharacterController => characterController;

    public PlayerStateMachine stateMachine;

    public Animator Animator => animator;

    public float SlideHeight => slideHeight;
    public float NormalHeight => normalHeight;

    public Vector3 SlideCenter => slideCenter;

    public Vector3 NormalCenter => normalCenter;
    public float MoveAmount => moveAmount;
    public float JumpForce => jumpSpeed;

    public float RotationSpeed => rotationSpeed;
    public bool IsGrounded() => isGrounded;

    public float YSpeed => ySpeed;

    public float WallRunSpeed => wallRunSpeed;
    public float WallRunVerticalSpeed => wallRunVerticalSpeed;
    public float WallRunStickForce => wallRunStickForce;
    public float WallJumpForce => wallJumpForce;
    public float WallJumpHorizontalForce => wallJumpHorizontalForce;
    public bool IsWallRunning() => isWallRunning;
    public CameraController CameraController => cameraController;

    // Recomiendo usar .triggered para detectar el input de salto
    public bool JumpPressed() => inputActions != null && inputActions.Player.Jump.triggered;
    public bool SprintPressed() => inputActions.Player.Sprint.IsPressed();
    public bool SlidePressed() => inputActions.Player.Crouch.WasPressedThisFrame();

    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Player.Disable();
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        stateMachine = new PlayerStateMachine();

        normalHeight = characterController.height;
        normalCenter = characterController.center;

        slideCenter = new Vector3(normalCenter.x, slideHeight / 2f, normalCenter.z);
    }
    private void Start()
    {
        stateMachine.Initialize(new IdleState(this));
    }
    private void Update()
    {
        // timers
        if (wallRunCooldownTimer > 0f) wallRunCooldownTimer -= Time.deltaTime;
        if (recentWallJumpTimer > 0f)
        {
            recentWallJumpTimer -= Time.deltaTime;
            if (recentWallJumpTimer <= 0f) recentWallJump = false;
        }

        GroundCheck();
        stateMachine.Update();
        UpdateSpeed();
    }
    public void UpdateSpeed()
    {
        if (SprintPressed())
        {
            currentSpeed = runSpeed;
            moveAmount += 0.5f;
        }
        else
            currentSpeed = moveSpeed;
    }

    public bool CanStartWallRun()
    {
        return wallRunCooldownTimer <= 0f && !isGrounded && !recentWallJump;
    }

    public void StartWallRun(Vector3 normal, Vector3 forward)
    {
        previousSpeed = currentSpeed;
        currentSpeed = wallRunSpeed;
        isWallRunning = true;
        wallRunNormal = normal;
        wallRunForward = forward;
        wallRunTimer = 0f;

        targetRotation = Quaternion.LookRotation(wallRunForward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        ySpeed = wallRunVerticalSpeed;
    }

    public void StopWallRun()
    {
        currentSpeed = previousSpeed;
        isWallRunning = false;
    }

    public void ApplyWallJump(Vector3 wallNormal)
    {
        Debug.Log("WALL JUMP TRIGGERED");
        // establecer velocidad vertical de salto y impulso horizontal persistente
        ySpeed = wallJumpForce;

        Vector3 pushDir = wallNormal + Vector3.up * 0.5f;
        pushDir.Normalize();
        externalVelocity = pushDir * wallJumpHorizontalForce;

        isWallRunning = false;

        // activar cooldown y ventana que evita reenganche inmediato
        wallRunCooldownTimer = wallRunCooldown;
        recentWallJump = true;
        recentWallJumpTimer = recentWallJumpWindow;

        Debug.Log($"ApplyWallJump: ySpeed={ySpeed}, externalVelocity={externalVelocity}, cooldown={wallRunCooldownTimer}, recentWindow={recentWallJumpTimer}");
    }

    public Vector3 GetMoveDirection()
    {
        if (cameraController == null)
        {
            Debug.LogError("CAMERA CONTROLLER ES NULL");
        }
        Vector3 moveDir = cameraController.PlanarRotation * Movement();
        return moveDir;
    }
    public Vector3 Movement()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();

        moveAmount = Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.y));

        var moveImput = (new Vector3(move.x, 0, move.y)).normalized;
        return moveImput;
    }
    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }

    public void Move(Vector3 dir)
    {
        var velocity = dir * currentSpeed;

        velocity += externalVelocity;

        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, 8f * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplyGravity()
    {
        if (isWallRunning)
        {
            ySpeed = Mathf.Lerp(ySpeed, wallRunVerticalSpeed, 15f * Time.deltaTime);
            wallRunTimer += Time.deltaTime;
            if (wallRunTimer >= wallRunDuration)
            {
                isWallRunning = false;
            }
            return;
        }

        if (isGrounded && ySpeed < 0)
            ySpeed = -2f;
        else
            ySpeed += Physics.gravity.y * Time.deltaTime;
    }

    public void SetYSpeed(float value)
    {
        ySpeed = value;
        Debug.Log("YSpeed set to: " + ySpeed);
    }

    // Exponer la bandera para que JumpState la consulte
    public bool RecentWallJump() => recentWallJump;

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
