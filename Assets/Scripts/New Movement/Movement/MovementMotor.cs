using UnityEngine;

public class MovementMotor : MonoBehaviour
{
    private Vector3 _velocity;
    public Vector3 velocity
    {
        get => _velocity;
        private set => _velocity = value;
    }

    private CharacterController controller;

    private PlayerMovementConfig config;

    public void Initialize(
        CharacterController cc,
        PlayerMovementConfig cfg)
    {
        controller = cc;
        config = cfg;
    }

    public void Move(
        Vector3 desiredDirection,
        float targetSpeed)
    {
        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0,
                velocity.z);

        Vector3 desiredVelocity =
            desiredDirection * targetSpeed;

        horizontalVelocity =
            Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                config.acceleration * Time.deltaTime);

        velocity = new Vector3(
            horizontalVelocity.x,
            velocity.y,
            horizontalVelocity.z);
    }

    public void ApplyGravity()
    {
        velocity +=
            Vector3.up *
            config.gravity *
            Time.deltaTime;
    }

    public void Jump()
    {
        _velocity.y =
            Mathf.Sqrt(
                config.jumpHeight *
                -2f *
                config.gravity);
    }

    public void GroundSnap()
    {
        if (_velocity.y < 0)
            _velocity.y = -2f;
    }

    public void Execute()
    {
        controller.Move(
            velocity * Time.deltaTime);
    }
}