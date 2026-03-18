using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float distance = 5f;


    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = 45;



    [SerializeField] Vector2 fraimingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;


    [SerializeField] float mouseSensitivityX = 1f;
    [SerializeField] float mouseSensitivityY = 1f;

    [Header("FOV")]
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float runFOV = 75f;
    [SerializeField] float wallRunFOV = 78f;
    [SerializeField] float fovSpeed = 8f;

    Camera cam;
    float targetFOV;

    [Header("Tilt")]
    [SerializeField] float tiltAngle = 15f;
    [SerializeField] float tiltSpeed = 5f;

    float currentTilt;
    float targetTilt;


    float rotationY;
    float rotationX;

    float invertXVal;
    float invertYVal;

    private void Start()
    {
        cam = GetComponent<Camera>();
        targetFOV = normalFOV;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        invertXVal = invertX ? -1 : 1;
        invertYVal = invertY ? -1 : 1;
    
        rotationY += Mouse.current.delta.x.ReadValue() * invertYVal * mouseSensitivityY;

        rotationX += Mouse.current.delta.y.ReadValue() * invertXVal * mouseSensitivityX;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        var targetRotation = Quaternion.Euler(rotationX, rotationY, currentTilt);

        var focusPosition = followTarget.position + new Vector3(fraimingOffset.x, fraimingOffset.y);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSpeed * Time.deltaTime);
    }
    public void SetFOVState(bool isRunning, bool isWallRunning)
    {
        if (isWallRunning)
            targetFOV = wallRunFOV;
        else if (isRunning)
            targetFOV = runFOV;
        else
            targetFOV = normalFOV;
    }
    public void SetTilt(bool wallRight, bool wallLeft)
    {
        if (wallRight)
            targetTilt = tiltAngle;
        else if (wallLeft)
            targetTilt = -tiltAngle;
        else
            targetTilt = 0f;
    }
    //Esto es una Propiedad que devuelve una rotacion en el plano horizontal, es decir, solo con la rotacion en Y, para que el player se mueva en esa direccion
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}
