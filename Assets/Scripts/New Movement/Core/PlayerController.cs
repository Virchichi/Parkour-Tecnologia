using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;

    public Transform cameraTransform;

    public InputReader input;

    public GroundDetector groundDetector;

    public MovementMotor motor;

    public PlayerMovementConfig config;

    public StateMachine StateMachine { get; private set; }

    public IdleState IdleState { get; private set; }

    public MoveState MoveState { get; private set; }

    public SprintState SprintState { get; private set; }

    public float CoyoteTimer { get; set; }

    public float JumpBufferTimer { get; set; }

    private void Awake()
    {
        StateMachine = new StateMachine();

        motor.Initialize(controller, config);

        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        SprintState = new SprintState(this);
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        HandleTimers();

        StateMachine.Update();

        motor.ApplyGravity();

        if (groundDetector.IsGrounded())
            motor.GroundSnap();

        motor.Execute();
    }

    void HandleTimers()
    {
        if (groundDetector.IsGrounded())
            CoyoteTimer = config.coyoteTime;
        else
            CoyoteTimer -= Time.deltaTime;

        if (input.JumpPressed)
            JumpBufferTimer = config.jumpBufferTime;
        else
            JumpBufferTimer -= Time.deltaTime;
    }

    public Vector3 GetCameraRelativeDirection()
    {
        Vector2 move = input.MoveInput;

        Vector3 forward =
            cameraTransform.forward;

        Vector3 right =
            cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (
            forward * move.y +
            right * move.x).normalized;
    }

    public void RotateTowards(
        Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion target =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                target,
                config.rotationSpeed *
                Time.deltaTime);
    }
}