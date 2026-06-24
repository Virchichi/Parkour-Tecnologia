using UnityEngine;

public class MoveState : GroundedState
{
    public MoveState(PlayerController player)
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
            player.config.walkSpeed);

        player.RotateTowards(moveDir);

        if (moveDir.magnitude < 0.1f)
        {
            player.StateMachine.ChangeState(
                player.IdleState);
        }

        if (player.input.SprintHeld)
        {
            player.StateMachine.ChangeState(
                player.SprintState);
        }
    }
}
