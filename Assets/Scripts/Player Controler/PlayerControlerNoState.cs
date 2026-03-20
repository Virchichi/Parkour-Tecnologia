using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Analytics.IAnalytic;

public class PlayerControlerNoState : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    float currentSpeed;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [SerializeField] float fallingHeight = 5f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = .2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    [Header("Wall Run")]
    [SerializeField] float wallRunSpeed = 6f;
    [SerializeField] float wallRunGravity = -2f;
    [SerializeField] float wallJumpForce = 7f;

    bool isWallRunning;
    Vector3 wallNormal;

    [Header("Slide")]
    [SerializeField] float slideSpeed = 8f;
    [SerializeField] float slideDuration = 1f;

    [Header("Slide Collider")]
    [SerializeField] float slideHeight = 1f;
    [SerializeField] float normalHeight = 2f;

    Vector3 normalCenter;
    Vector3 slideCenter;

    bool isSliding;

    float moveAmount;

    bool isGrounded;

    bool hasControl = true;

    float ySpeed;

    Quaternion targetRotation;

    //Gameobject

    private InputSystem_Actions inputActions;

    CameraController cameraController;

    Animator animator;

    CharacterController characterController;

    EnvironmentScanner environmentScanner;

    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        if (inputActions == null)
            inputActions.Player.Disable();
    }
    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();

        normalHeight = characterController.height;
        normalCenter = characterController.center;

        // Centro más bajo para el slide
        slideCenter = new Vector3(normalCenter.x, slideHeight / 2f, normalCenter.z);
    }



    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();
        // WALL RUN
        WallRunning(hitData);
        cameraController.SetTilt(isWallRunning && hitData.rightHitFound, isWallRunning && hitData.leftHitFound
);

        // SLIDE INPUT
        if (inputActions.Player.Crouch.WasPressedThisFrame() && moveAmount > 0.5f && isGrounded && !isSliding)
        {
            StartCoroutine(Slide());
        }

        GroundCheck();
        GravityCheck();

        //TE DA LA DIRECCION EN EL PLANO HORIZONTAL DE LA CAMARA,
        //PARA QUE EL PLAYER SE MUEVA EN ESA DIRECCION, ES DECIR,
        //SI LA CAMARA ESTA GIRADA HACIA LA DERECHA,
        //EL PLAYER SE MOVERA HACIA LA DERECHA CUANDO PRESIONES W

        var moveDir = cameraController.PlanarRotation * Movement();

        if (!hasControl || isSliding)
            return;

        cameraController.SetFOVState(inputActions.Player.Sprint.IsPressed() && moveAmount > 0.1f, isWallRunning);

        if (inputActions.Player.Jump.WasPressedThisFrame() && !GlobalControlerData.canPerformParkour)
        {
            moveDir = JumpAction(hitData);
        }
        if (inputActions.Player.Sprint.IsPressed())
        {
            moveAmount += 0.5f;
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        var velocity = moveDir * currentSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
    }
    Vector3 Movement()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();

        moveAmount = Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.y)); //for Animator imput

        var moveImput = (new Vector3(move.x, 0, move.y)).normalized;
        return moveImput;
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }
    public Vector3 JumpAction(ObstacleHitData hitData)
    {
        Vector3 jumpDir = Vector3.zero;
        // WallRuning Jump
        if (isWallRunning)
        {
            isWallRunning = false;

            Vector3 dir = wallNormal + Vector3.up;
            jumpDir = dir.normalized;

            ySpeed = wallJumpForce;

            //StartCoroutine(DoAnimationAction("WallJump", true));

            return jumpDir;
        }
        //else if (!isGrounded)
        //{
        //    if (hitData.forwardHitFound)
        //    {
        //        jumpDir += new Vector3(hitData.forwardHit.normal.x, 0, hitData.forwardHit.normal.z).normalized;
        //        ySpeed = jumpSpeed;
        //        StartCoroutine(DoAnimationAction("FrontWallJump", true));
        //    }
        //    if (hitData.rightHitFound)
        //    {
        //        jumpDir += new Vector3(hitData.rightHit.normal.x, 0, hitData.rightHit.normal.z).normalized;
        //        ySpeed = jumpSpeed;
        //        StartCoroutine(DoAnimationAction("RightWallJump", true));
        //    }
        //    if (hitData.leftHitFound)
        //    {
        //        jumpDir += new Vector3(hitData.leftHit.normal.x, 0, hitData.leftHit.normal.z).normalized;
        //        ySpeed = jumpSpeed;
        //        StartCoroutine(DoAnimationAction("LeftWallJump", true));
        //    }
        //}
        else
        {
            ySpeed = jumpSpeed;
            StartCoroutine(DoAnimationAction("Jump", false));
        }

        return jumpDir;
    }
    void WallRunning(ObstacleHitData hitData)
    {
        if (!isGrounded && environmentScanner.WallRunCheck(hitData) && moveAmount > 0.1f)
        {
            isWallRunning = true;

            if (hitData.rightHitFound)
            {
                wallNormal = hitData.rightHit.normal;
            }
            else if (hitData.leftHitFound)
            {
                wallNormal = hitData.leftHit.normal;
            }

            // Dirección del wallrun (paralela a la pared)
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

            // Ajustar dirección para que siga hacia adelante
            if (Vector3.Dot(wallForward, transform.forward) < 0)
                wallForward = -wallForward;

            ySpeed = wallRunGravity;

            Vector3 velocity = wallForward * wallRunSpeed;
            velocity.y = ySpeed;

            characterController.Move(velocity * Time.deltaTime);

            // Rotación hacia dirección del wallrun
            targetRotation = Quaternion.LookRotation(wallForward);
        }
        else
        {
            isWallRunning = false;
        }
    }
    IEnumerator Slide()
    {
        isSliding = true;

        //Reducir collider
        characterController.height = slideHeight;
        characterController.center = slideCenter;

        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;

            Vector3 slideDir = transform.forward;
            Vector3 velocity = slideDir * slideSpeed;
            velocity.y = ySpeed;

            characterController.Move(velocity * Time.deltaTime);

            yield return null;
        }

        //Restaurar collider
        characterController.height = normalHeight;
        characterController.center = normalCenter;

        isSliding = false;
    }
    void GravityCheck()
    {
        if (isGrounded && ySpeed < 0)
        {
            ySpeed = -2f;
        }
        else if (!isWallRunning)
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
    }
    bool CheckFallingLandig(ObstacleHitData hitData)
    {
        if (!isGrounded && ySpeed < 0 && hitData.DownHit.point.y >= fallingHeight)
        {
            //Aqui puedes agregar una animacion de caida
            return true;
        }
        return false;
    }
    IEnumerator DoAnimationAction(string animationName, bool isInAir)
    {
        GlobalControlerData.inAir = isInAir;

        if (isInAir)
        {
            SetControl(false);
        }
        animator.CrossFade(animationName, .2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animationName))
        {
            Debug.LogError("Animation not found: " + animationName);
            yield break;
        }
        float timer = 0f;
        while (timer < animState.length)
        {
            timer += Time.deltaTime;

            if (animator.IsInTransition(0) && timer >= animState.length /** parkourAction.MatchTargetTime*/)
            {
                break;
            }
            yield return null;
        }
        //yield return new WaitForSeconds(parkourAction.PosActionDelay);

        SetControl(true);

        if (isInAir)
            GlobalControlerData.inAir = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}
