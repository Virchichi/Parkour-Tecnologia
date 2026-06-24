using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;

    [SerializeField] private float radius = 0.3f;

    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            radius,
            groundMask);
    }
}
