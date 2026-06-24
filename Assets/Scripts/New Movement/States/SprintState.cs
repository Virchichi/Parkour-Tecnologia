using UnityEngine;

public class SprintState : GroundedState
{
    public SprintState(PlayerController player)
        : base(player)
    {
    }

    public override void Update()
    {
        HandleJump();

        Vector3 moveDir =
            player.GetCameraRelativeDirection();

        player.motor.Move(
            moveDir,
            player.config.sprintSpeed);

        player.RotateTowards(moveDir);

        if (!player.input.SprintHeld)
        {
            player.StateMachine.ChangeState(
                player.MoveState);
        }

        if (moveDir.magnitude < 0.1f)
        {
            player.StateMachine.ChangeState(
                player.IdleState);
        }
    }
}
