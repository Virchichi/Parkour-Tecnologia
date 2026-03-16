using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkourControler : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    
    //bool inAction;

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
        var hitData = environmentScanner.ObstacleCheck();

        if (hitData.forwardHitFound)
        {
            foreach (var parkourAction in parkourActions)
            {
                if (parkourAction.CanBePerformed(hitData, transform) && inputActions.Player.Jump.IsPressed() && !GlobalControlerData.inAction)
                {
                    //playerControler.parkourEnable = true;
                    StartCoroutine(DoParkourAction(parkourAction));
                    break;
                }
            }

        }
    }
    IEnumerator DoParkourAction(ParkourAction parkourAction)
    {
        GlobalControlerData.inAction = true;
        playerControler.SetControl(false);

        animator.CrossFade(parkourAction.AnimationName, .2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(parkourAction.AnimationName))
        {
            Debug.LogError("Animation not found: " + parkourAction.AnimationName);
            yield break;
        }
        float timer = 0f;
        while (timer < animState.length)
        {
            timer += Time.deltaTime;

            //Rotate the player towards the forward hit normal during the parkour action
            if (parkourAction.RotateToObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, parkourAction.TargetRotation, playerControler.RotationSpeed * Time.deltaTime);
            }
            if(parkourAction.EneableTargetMatching)
            {
                MatchTarget(parkourAction);
            }

            if (animator.IsInTransition(0) && timer >= animState.length * parkourAction.MatchTargetTime)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(parkourAction.PosActionDelay);

        playerControler.SetControl(true);

        //playerControler.parkourEnable = false; 
        GlobalControlerData.inAction = false;
    }
    void MatchTarget(ParkourAction parkourAction)
    {
        if(animator.isMatchingTarget) return;
        animator.MatchTarget(parkourAction.MatchPos, transform.rotation, 
            parkourAction.MatchBodyPart, new MatchTargetWeightMask(parkourAction.MatchPosWeigth, 0), 
            parkourAction.MatchStartTime, parkourAction.MatchTargetTime);
    }
}
