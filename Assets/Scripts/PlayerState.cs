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
        // Si hemos hecho un wall-jump recientemente, NO sobrescribimos la velocidad vertical
        if (player.RecentWallJump())
        {
            Debug.Log("JumpState.Enter: recent wall jump detected, no override of ySpeed");
        }
        else
        {
            // Si venimos en wallrun y no hubo reciente wall-jump, aplicar wall-jump special
            if (player.IsWallRunning())
            {
                player.SetYSpeed(player.WallJumpForce);
            }
            else
            {
                player.SetYSpeed(player.JumpForce);
            }
        }

        player.Animator.CrossFade("Jump", 0.1f);
    }

    public override void Update()
    {
        player.ApplyGravity();
        player.Move(player.GetMoveDirection());

        // Log para depuración: ver por frame si estamos en suelo / ySpeed / recent flag
        Debug.Log($"JumpState: IsGrounded={player.IsGrounded()} YSpeed={player.YSpeed} RecentWallJump={player.RecentWallJump()} MoveAmount={player.MoveAmount}");

        // 1) Primero prioridad a aterrizar: si estás en suelo y ya no subes, ir a Idle
        if (player.IsGrounded() && player.YSpeed <= 0f)
        {
            player.stateMachine.ChangeState(new IdleState(player));
            return;
        }

        // 2) Sólo si no estamos en suelo y no acabamos de wall-jumpear, intentar wallrun.
        //    Añadimos comprobación explícita !IsGrounded() para seguridad.
        if (!player.RecentWallJump()
            && !player.IsGrounded()
            && player.GetComponent<EnvironmentScanner>().WallRunCheck(player.GetComponent<EnvironmentScanner>().ObstacleCheck())
            && player.CanStartWallRun())
        {
            Debug.Log("Wall Run Available from JumpState");
            player.stateMachine.ChangeState(new WallRunState(player));
            return;
        }
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
        player.Move(player.GetMoveDirection());

        if (player.IsGrounded())
            player.stateMachine.ChangeState(new IdleState(player));
    }
}
public class WallRunState : PlayerState
{
    bool wallRight;
    bool wallLeft;
    Vector3 wallNormal;
    Vector3 wallForward;

    public WallRunState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered WallRun State");

        var scanner = player.GetComponent<EnvironmentScanner>();
        var hit = scanner.ObstacleCheck();

        if (!scanner.WallRunCheck(hit))
        {
            player.stateMachine.ChangeState(new FallState(player));
            return;
        }

        // Evitar iniciar wallrun si estamos en suelo o en cooldown tras un wall-jump
        if (player.IsGrounded() || !player.CanStartWallRun())
        {
            Debug.Log("WallRun blocked at Enter (grounded or cooldown)");
            player.stateMachine.ChangeState(new FallState(player));
            return;
        }

        wallRight = hit.rightHitFound;
        wallLeft = hit.leftHitFound;
        wallNormal = wallRight ? hit.rightHit.normal : hit.leftHit.normal;

        // calculamos la dirección de avance por la pared
        wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, player.transform.forward) < 0)
            wallForward = -wallForward;

        // iniciar wallrun en el player (guarda normal/forward y ajusta velocidad)
        player.StartWallRun(wallNormal, wallForward);

        // pequeña corrección posicional para "pegar" al jugador a la pared (evita separaciones)
        player.CharacterController.Move(-wallNormal * (player.WallRunStickForce * 0.02f));

        // ajustes cámara si existe
        if (player.CameraController != null)
            player.CameraController.SetFOVState(false, true);
    }

    public override void Update()
    {
        var scanner = player.GetComponent<EnvironmentScanner>();
        var hit = scanner.ObstacleCheck();

        // si la pared se pierde, salimos a caer
        if (!scanner.WallRunCheck(hit) || !player.IsWallRunning())
        {
            player.stateMachine.ChangeState(new FallState(player));
            return;
        }
        if(player.IsGrounded())
        {
            player.stateMachine.ChangeState(new IdleState(player));
            return;
        }

        // actualizar normal/forward por si cambia mientras avanzamos
        wallRight = hit.rightHitFound;
        wallLeft = hit.leftHitFound;
        wallNormal = wallRight ? hit.rightHit.normal : hit.leftHit.normal;
        wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, player.transform.forward) < 0)
            wallForward = -wallForward;

        // empujoncito lateral continuo para pegar al jugador
        player.CharacterController.Move(-wallNormal * player.WallRunStickForce * Time.deltaTime);

        // fijar/ajustar velocidad vertical objetivo (ApplyGravity lo suaviza)
        player.SetYSpeed(player.WallRunVerticalSpeed);

        // mover al jugador a lo largo de la pared
        player.Move(wallForward);

        // ajustar tilt de camara
        if (player.CameraController != null)
            player.CameraController.SetTilt(wallRight, wallLeft);

        // salto desde la pared
        if (player.JumpPressed())
        {
            // aplicamos wall-jump: empuje lateral + vertical
            player.ApplyWallJump(wallNormal);
            player.stateMachine.ChangeState(new JumpState(player));
            return;
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting WallRun State");
        player.StopWallRun();
        if (player.CameraController != null)
        {
            player.CameraController.SetFOVState(false, false);
            player.CameraController.SetTilt(false, false);
        }
    }
}
public class SlideState : PlayerState
{
    float timer;

    public SlideState(PlayerControler player) : base(player) { }

    public override void Enter()
    {
        player.Animator.CrossFade("Slide", 0.1f);
        timer = 1f;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        player.Move(player.transform.forward);
        player.CharacterController.height = player.SlideHeight;
        player.CharacterController.center = player.SlideCenter;

        if (timer <= 0)
        {
            player.CharacterController.height = player.NormalHeight;
            player.CharacterController.center= player.NormalCenter;
            player.stateMachine.ChangeState(new IdleState(player));
        }

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

