using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkourControler : MonoBehaviour
{
    bool inAction;

    EnvironmentScanner environmentScanner;

    Animator animator;
    
    private InputSystem_Actions inputActions;
    PlayerControler playerControler;

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
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerControler = GetComponent<PlayerControler>();
    }
    private void Update()
    {
        if (inputActions.Player.Jump.IsPressed() && !inAction)
        {
            var hitData = environmentScanner.ObstacleCheck();

            if (hitData.forwardHitFound)
            {
                StartCoroutine(DoParkourAction());
            }
        }
    }
    IEnumerator DoParkourAction()
    {
        inAction = true;
        playerControler.SetControl(false);
        animator.CrossFade("StepUp", .2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);

        yield return new WaitForSeconds(animState.length);
        
        playerControler.SetControl(true);

        inAction = false;
    }
}
