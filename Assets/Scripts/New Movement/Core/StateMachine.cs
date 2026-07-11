public class StateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState state)
    {
        CurrentState = state;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();

        CurrentState = newState;

        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}
