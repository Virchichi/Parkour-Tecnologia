using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;


    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = .2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;

    bool hasControl = true;

    float ySpeed;

    Quaternion targetRotation;

    private InputSystem_Actions inputActions;

    CameraController cameraController;
    
    Animator animator;

    CharacterController characterController;

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
    }
    private void Update()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();

        float moveAmount =Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.y));

        var moveImput = (new Vector3(move.x, 0, move.y)).normalized;
        //TE DA LA DIRECCION EN EL PLANO HORIZONTAL DE LA CAMARA,
        //PARA QUE EL PLAYER SE MUEVA EN ESA DIRECCION, ES DECIR,
        //SI LA CAMARA ESTA GIRADA HACIA LA DERECHA,
        //EL PLAYER SE MOVERA HACIA LA DERECHA CUANDO PRESIONES W
        var moveDir = cameraController.PlanarRotation * moveImput;
        if (!hasControl)
            return;

        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f; // Para que el personaje se mantenga pegado al suelo, si es 0 puede haber problemas de colision con el suelo
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move( velocity * Time.deltaTime );


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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}
