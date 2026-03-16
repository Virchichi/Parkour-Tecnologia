using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Analytics.IAnalytic;

public class PlayerControler : MonoBehaviour
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

    bool isGrounded;
    bool isJumping;

    bool hasControl = true;

    float ySpeed;

    Quaternion targetRotation;

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
    }



    private void Update()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();

        float moveAmount = Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.y)); //for Animator imput

        var moveImput = (new Vector3(move.x, 0, move.y)).normalized;

        //TE DA LA DIRECCION EN EL PLANO HORIZONTAL DE LA CAMARA,
        //PARA QUE EL PLAYER SE MUEVA EN ESA DIRECCION, ES DECIR,
        //SI LA CAMARA ESTA GIRADA HACIA LA DERECHA,
        //EL PLAYER SE MOVERA HACIA LA DERECHA CUANDO PRESIONES W

        var moveDir = cameraController.PlanarRotation * moveImput;

        if (!hasControl)
            return;

        var hitData = environmentScanner.ObstacleCheck();

        GroundCheck();

        GravityCheck();

        if (inputActions.Player.Jump.WasPressedThisFrame() && !GlobalControlerData.canPerformParkour && !isJumping)
        {
            moveDir = JumpAction(hitData);
            Debug.Log("ySpeed" + ySpeed);
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
        if(ySpeed > 0)
            Debug.Log("ySpeed 2º segunda vez" + ySpeed);
        var velocity = moveDir * currentSpeed;
        velocity.y = ySpeed;
        
        //Debug.Log("Velocity" + velocity.y);
        //if(velocity.y > 0) {
        //    animator.CrossFade("isJumping", .2f);
        //}
        //else if (isGrounded)
        //{
        //    animator.SetBool("isJumping", false);
        //}
        //Debug.Log("MoveDir: " + moveDir + " Velocity: " + velocity.y);
        characterController.Move(velocity * Time.deltaTime);

        if (ySpeed > 0)
            Debug.Log("ySpeed 3º segunda vez" + velocity.y);
        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
            rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
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
        isJumping = true;
        Vector3 jumpDir = new Vector3();
        if (!isGrounded)
        {
            if (hitData.forwardHitFound)
            {
                jumpDir += new Vector3(hitData.forwardHit.normal.x, 0, hitData.forwardHit.normal.z).normalized;
                ySpeed = jumpSpeed;
                StartCoroutine(DoAnimationAction("FrontWallJump", true));
            }
            if (hitData.rightHitFound)
            {
                jumpDir += new Vector3(hitData.rightHit.normal.x, 0, hitData.rightHit.normal.z).normalized;
                ySpeed = jumpSpeed;
                StartCoroutine(DoAnimationAction("RightWallJump", true));
            }
            if (hitData.leftHitFound)
            {
                jumpDir += new Vector3(hitData.leftHit.normal.x, 0, hitData.leftHit.normal.z).normalized;
                ySpeed = jumpSpeed;
                StartCoroutine(DoAnimationAction("LeftWallJump", true));
            }
        }
        else
        {
            ySpeed = jumpSpeed;
            Debug.Log("ySpeed" + ySpeed + "JumpSpeed" + jumpSpeed );
            StartCoroutine(DoAnimationAction("Jump", false));
        }
        isJumping = false;
        return jumpDir;
    }
    void WallRuning(ObstacleHitData hitData)
    {
        if (!isGrounded && environmentScanner.WallRunCheck(hitData))
        {
           if (hitData.rightHitFound)
           {
                //Wallrun a la derecha
           }
           else if (hitData.leftHitFound)
           {
                //Wallrun a la izquierda
           }
        }
    }
    void GravityCheck()
    {
        if (isGrounded)
        {
            ySpeed = -0.5f; // Para que el personaje se mantenga pegado al suelo, si es 0 puede haber problemas de colision con el suelo 
        }
        else if(!isJumping)
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

        if(isInAir)
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

        if(isInAir)
            GlobalControlerData.inAir = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}
