using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 hitRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] float hitRayLenght = 0.8f;
    [SerializeField] float heightRayLenght = 5f;
    [SerializeField] LayerMask obstacleLayer;
    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();

        hitData.forwardHitFound = Physics.Raycast(transform.position + hitRayOffset, transform.forward, 
            out hitData.forwardHit, hitRayLenght, obstacleLayer);
        hitData.rightHitFound = Physics.Raycast(transform.position + hitRayOffset, transform.right, 
            out hitData.rightHit, hitRayLenght, obstacleLayer);
        hitData.leftHitFound = Physics.Raycast(transform.position + hitRayOffset, -transform.right, 
            out hitData.leftHit, hitRayLenght, obstacleLayer);

        Debug.DrawRay(transform.position + hitRayOffset, 
            transform.forward * hitRayLenght, hitData.forwardHitFound ? Color.red : Color.green);
        Debug.DrawRay(transform.position + hitRayOffset, 
            transform.right * hitRayLenght, hitData.rightHitFound ? Color.red : Color.green);
        Debug.DrawRay(transform.position + hitRayOffset, 
            -transform.right * hitRayLenght, hitData.leftHitFound ? Color.red : Color.green);

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLenght;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                out hitData.heightHit, heightRayLenght, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLenght, hitData.heightHitFound ? Color.red : Color.green);

        }

        return hitData;
    }
    public bool WallRunCheck(ObstacleHitData hitData)
    {
        if(hitData.rightHitFound || hitData.leftHitFound)
            return true;
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + hitRayOffset,
            transform.forward * hitRayLenght);
        Gizmos.DrawRay(transform.position + hitRayOffset,
            transform.right * hitRayLenght);
        Gizmos.DrawRay(transform.position + hitRayOffset,
            -transform.right * hitRayLenght);
    }

}
public struct ObstacleHitData 
{
    public bool forwardHitFound;
    public bool rightHitFound;
    public bool leftHitFound;
    public bool heightHitFound;
    public bool DownHitFound;
    public RaycastHit forwardHit;
    public RaycastHit rightHit;
    public RaycastHit leftHit;
    public RaycastHit heightHit;
    public RaycastHit DownHit;
}