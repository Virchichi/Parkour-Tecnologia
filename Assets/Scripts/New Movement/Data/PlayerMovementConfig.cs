using UnityEngine;

[CreateAssetMenu(menuName = "Player/Movement Config")]
public class PlayerMovementConfig : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;

    [Header("Acceleration")]
    public float acceleration = 15f;
    public float deceleration = 20f;

    [Header("Rotation")]
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float gravity = -25f;

    [Header("Forgiveness")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
}
