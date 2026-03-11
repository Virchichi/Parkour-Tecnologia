using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Parkour Action", fileName = "New Parkour Action")]

public class ParkourAction : ScriptableObject
{
    [SerializeField] string animationName;

    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;

    [SerializeField] bool rotateToObstacle;

    [Header("Target Matching")]
    [SerializeField] bool eneableTargetMatching = true;
    [SerializeField] AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos { get; set; }

    public bool CanBePerformed(ObstacleHitData hitData , Transform player)
    {
        float height = hitData.heightHit.point.y - player.position.y;
        if(height < minHeight || height > maxHeight)
            return false;
        if(rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(hitData.forwardHit.normal * -1);
        if(eneableTargetMatching)
            MatchPos = hitData.heightHit.point;
        return true;
    }

    public String AnimationName => animationName;
    public bool RotateToObstacle => rotateToObstacle;

    public bool EneableTargetMatching => eneableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
}
