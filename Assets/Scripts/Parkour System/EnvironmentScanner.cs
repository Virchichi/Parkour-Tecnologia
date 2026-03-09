using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] float forwardRayLenght = 0.8f;
    [SerializeField] float heightRayLenght = 5f;
    [SerializeField] LayerMask obstacleLayer;
    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        hitData.forwardHitFound = Physics.Raycast(transform.position + forwardRayOffset, transform.forward, 
            out hitData.forwardHit, forwardRayLenght, obstacleLayer);

        Debug.DrawRay(transform.position + forwardRayOffset, 
            transform.forward * forwardRayLenght, hitData.forwardHitFound ? Color.red : Color.green);

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLenght;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                out hitData.heightHit, heightRayLenght, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLenght, hitData.heightHitFound ? Color.red : Color.green);

        }

        return hitData;
    }
  
}
public struct ObstacleHitData 
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}