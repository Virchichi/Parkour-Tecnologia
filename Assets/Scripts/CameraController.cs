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


    float rotationY;
    float rotationX;

    float invertXVal;
    float invertYVal;

    private void Start()
    {
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

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        var focusPosition = followTarget.position + new Vector3(fraimingOffset.x, fraimingOffset.y);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }
    //Esto es una Propiedad que devuelve una rotacion en el plano horizontal, es decir, solo con la rotacion en Y, para que el player se mueva en esa direccion
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}
