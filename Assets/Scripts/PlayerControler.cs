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
    [SerializeField] float wallRunGravity = -2f;
    [SerializeField] float wallJumpForce = 7f;

    [Header("Slide")]
    [SerializeField] float slideSpeed = 8f;
    [SerializeField] float slideDuration = 1f;

    [Header("Slide Collider")]
    [SerializeField] float slideHeight = 1f;
    [SerializeField] float normalHeight = 2f;

    float moveAmount;

    bool isGrounded;

    bool hasControl = true;

    float ySpeed;

    Quaternion targetRotation;

    //Gameobject

    private InputSystem_Actions inputActions;

    [SerializeField] CameraController cameraController;

    Animator animator;

    CharacterController characterController;

    public PlayerStateMachine stateMachine;

    public Animator Animator => animator;
    public float MoveAmount => moveAmount;
    public float JumpForce => jumpSpeed;

    public float RotationSpeed => rotationSpeed;
    public bool IsGrounded() => isGrounded;

    public bool JumpPressed() => inputActions.Player.Jump.WasPressedThisFrame();
    public bool SprintPressed() => inputActions.Player.Sprint.IsPressed();

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
    }
    private void Start()
    {
        stateMachine.Initialize(new IdleState(this));
    }
    private void Update()
    {
        GroundCheck();
        
        stateMachine.Update();
        UpdateSpeed();
    }
    public void UpdateSpeed()
    {
        if (SprintPressed())
            currentSpeed = runSpeed;
        else
            currentSpeed = moveSpeed;
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

        moveAmount = Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.y)); //for Animator imput

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
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplyGravity()
    {
        if (isGrounded && ySpeed < 0)
            ySpeed = -2f;
        else
            ySpeed += Physics.gravity.y * Time.deltaTime;
    }

    public void SetYSpeed(float value)
    {
        ySpeed = value;
    }
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
