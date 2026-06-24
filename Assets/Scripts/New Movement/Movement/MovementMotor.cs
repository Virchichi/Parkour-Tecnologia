using UnityEngine;

public class MovementMotor : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }

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
                Velocity.x,
                0,
                Velocity.z);

        Vector3 desiredVelocity =
            desiredDirection * targetSpeed;

        horizontalVelocity =
            Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                config.acceleration * Time.deltaTime);

        Velocity = new Vector3(
            horizontalVelocity.x,
            Velocity.y,
            horizontalVelocity.z);
    }

    public void ApplyGravity()
    {
        Velocity +=
            Vector3.up *
            config.gravity *
            Time.deltaTime;
    }

    public void Jump()
    {
        Velocity.y = Mathf.Sqrt(
                config.jumpHeight *
                -2f *
                config.gravity);
    }

    public void GroundSnap()
    {
        if (Velocity.y < 0)
            Velocity.y = -2f;
    }

    public void Execute()
    {
        controller.Move(
            Velocity * Time.deltaTime);
    }
}
