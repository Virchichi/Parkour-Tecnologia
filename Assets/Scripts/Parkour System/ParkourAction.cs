using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Parkour Action", fileName = "New Parkour Action")]

public class ParkourAction : ScriptableObject
{
    [SerializeField] string animationName;
    [SerializeField] string obstacleTag;

    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;

    [SerializeField] bool rotateToObstacle;
    [SerializeField] float posActionDelay;

    [SerializeField] bool isRuningAction;
    [SerializeField] bool isInAir;

    [Header("Target Matching")]
    [SerializeField] bool eneableTargetMatching = true;
    [SerializeField] AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPosWeigth = new Vector3(0, 1, 0);
    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos { get; set; }

    public bool CanBePerformed(ObstacleHitData hitData , Transform player)
    {
        // Check if the forward raycast hit an obstacle and the height raycast hit a valid point
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)
        {
            GlobalControlerData.canPerformParkour = false;
            return false;
        }
            

        // Check if the height of the obstacle is within the specified range
        float height = hitData.heightHit.point.y - player.position.y;
        if(height < minHeight || height > maxHeight)
        {
            GlobalControlerData.canPerformParkour = false;
            return false;
        }

        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(hitData.forwardHit.normal * -1);
        if(eneableTargetMatching)
            MatchPos = hitData.heightHit.point;

        GlobalControlerData.canPerformParkour = true;
        return true;
    }

    public String AnimationName => animationName;
    public bool RotateToObstacle => rotateToObstacle;

    public bool EneableTargetMatching => eneableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;

    public Vector3 MatchPosWeigth => matchPosWeigth;

    public float PosActionDelay => posActionDelay;
}
