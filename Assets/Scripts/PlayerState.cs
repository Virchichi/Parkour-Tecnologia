using UnityEngine;

public abstract class PlayerState
{
    protected PlayerControler player;

    public PlayerState(PlayerControler player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}
public class IdleState : PlayerState
{
    public IdleState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        player.Animator.CrossFade("Idle", 0.1f);
    }

    public override void Update()
    {
        player.ApplyGravity();

        var moveDir = player.GetMoveDirection();
        player.Move(moveDir);

        if (GlobalControlerData.inAction) return;

        if (player.MoveAmount > 0.1f)
            player.stateMachine.ChangeState(new WalkState(player));

        if (player.JumpPressed())
            player.stateMachine.ChangeState(new JumpState(player));
    }
}
public class WalkState : PlayerState
{
    public WalkState(PlayerControler player) : base(player) { }
    public override void Enter()
    {
        player.Animator.CrossFade("Walk", 0.1f);
    }
    public override void Update()
    {
        player.ApplyGravity();
        var moveDir = player.GetMoveDirection();
        player.Move(moveDir);
        if (player.MoveAmount <= 0.1f)
            player.stateMachine.ChangeState(new IdleState(player));
        if (player.SprintPressed())
            player.stateMachine.ChangeState(new RunState(player));
        if (!player.IsGrounded())
            player.stateMachine.ChangeState(new FallState(player));
        if (player.JumpPressed())
            player.stateMachine.ChangeState(new JumpState(player));
    }
}
public class RunState : PlayerState
{
    public RunState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        player.Animator.CrossFade("Run", 0.1f);
    }

    public override void Update()
    {
        player.ApplyGravity();
        player.Move(player.GetMoveDirection());
        player.UpdateSpeed();

        if (player.MoveAmount <= 1f)
            player.stateMachine.ChangeState(new WalkState(player));

        if (!player.IsGrounded())
            player.stateMachine.ChangeState(new FallState(player));

        if (player.JumpPressed())
            player.stateMachine.ChangeState(new JumpState(player));
        if(player.SlidePressed())
            player.stateMachine.ChangeState(new SlideState(player));
    }
}
public class JumpState : PlayerState
{
    public JumpState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        // Establecer velocidad vertical para el salto
        player.SetYSpeed(player.JumpForce);
        player.Animator.CrossFade("Jump", 0.1f);
    }

    public override void Update()
    {
        player.ApplyGravity();

        player.Move(player.GetMoveDirection());

        if (player.GetComponent<EnvironmentScanner>().WallRunCheck(
            player.GetComponent<EnvironmentScanner>().ObstacleCheck()))
        {
            player.stateMachine.ChangeState(new WallRunState(player));
        }

        if (player.IsGrounded())
            player.stateMachine.ChangeState(new IdleState(player));
    }
}
public class FallState : PlayerState
{
    public FallState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        //player.Animator.CrossFade("Fall", 0.2f);
    }

    public override void Update()
    {
        player.ApplyGravity();
        player.Move(player.Movement());

        if (player.IsGrounded())
            player.stateMachine.ChangeState(new IdleState(player));
    }
}
public class WallRunState : PlayerState
{
    public WallRunState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        //player.Animator.CrossFade("WallRun", 0.2f);
    }

    public override void Update()
    {
        var scanner = player.GetComponent<EnvironmentScanner>();
        var hit = scanner.ObstacleCheck();

        if (!scanner.WallRunCheck(hit))
        {
            player.stateMachine.ChangeState(new FallState(player));
            return;
        }

        Vector3 wallNormal = hit.rightHitFound ? hit.rightHit.normal : hit.leftHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

        if (Vector3.Dot(wallForward, player.transform.forward) < 0)
            wallForward = -wallForward;

        player.SetYSpeed(-2f);
        player.Move(wallForward);

        if (player.JumpPressed())
            player.stateMachine.ChangeState(new JumpState(player));
    }
}
public class SlideState : PlayerState
{
    float timer;

    public SlideState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        //player.Animator.CrossFade("Slide", 0.1f);
        timer = 1f;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        player.Move(player.transform.forward);

        if (timer <= 0)
            player.stateMachine.ChangeState(new IdleState(player));
    }
}
public class ParkourState : PlayerState
{
    ParkourAction action;
    ParkourControler parkour;
    float timer;

    public ParkourState(PlayerControler player, ParkourAction action, ParkourControler parkour) : base(player)
    {
        this.action = action;
        this.parkour = parkour;
    }

    public override void Enter()
    {
        player.SetControl(false);
        player.Animator.CrossFade(action.AnimationName, 0.1f);
        timer = 0f;
    }

    public override void Update()
    {
        // Actualizar por frame (no usar yield en un método void)
        var animState = player.Animator.GetCurrentAnimatorStateInfo(0);
        timer += Time.deltaTime;

        // Si la animación actual no es la esperada y no estamos en transición -> error y volver a Idle
        if (!animState.IsName(action.AnimationName) && !player.Animator.IsInTransition(0))
        {
            Debug.LogError("Animation not found: " + action.AnimationName);
            player.SetControl(true);
            player.stateMachine.ChangeState(new IdleState(player));
            return;
        }

        // Rotar hacia objetivo si procede
        if (action.RotateToObstacle)
        {
            player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, action.TargetRotation, player.RotationSpeed * Time.deltaTime);
        }

        // Match target si está habilitado
        if (action.EneableTargetMatching && parkour != null)
        {
            parkour.MatchTarget(action);
        }

        // Si ha acabado la animación, restaurar control y volver a Idle
        // Nota: animState.length puede ser 0 en algunos casos; este es el comportamiento original simplificado
        if (animState.length > 0f && timer >= animState.length)
        {
            player.SetControl(true);
            player.stateMachine.ChangeState(new IdleState(player));
        }
    }
}

