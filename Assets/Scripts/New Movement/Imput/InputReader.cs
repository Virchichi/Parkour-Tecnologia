using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }

    public bool SprintHeld { get; private set; }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            JumpPressed = true;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintHeld = context.ReadValueAsButton();
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }
}
