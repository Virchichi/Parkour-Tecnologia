using UnityEngine;

public abstract class GroundedState : PlayerState
{
    protected GroundedState(PlayerController player)
        : base(player)
    {
    }

    protected void HandleJump()
    {
        if (
            player.JumpBufferTimer > 0 &&
            player.CoyoteTimer > 0)
        {
            player.motor.Jump();

            player.input.ConsumeJump();

            player.JumpBufferTimer = 0;
        }
    }
}
