using UnityEngine;

public class IdleState : GroundedState
{
    public IdleState(PlayerController player)
        : base(player)
    {
    }

    public override void Update()
    {
        HandleJump();

        if (player.input.MoveInput.magnitude > 0.1f)
        {
            player.StateMachine.ChangeState(
                player.MoveState);
        }
    }
}
