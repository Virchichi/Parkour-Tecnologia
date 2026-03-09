using UnityEngine;

public class ParkourControler : MonoBehaviour
{
    EnvironmentScanner environmentScanner;
    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();

        if(hitData.forwardHitFound)
        {
                Debug.Log("Obstacle Detected: " + hitData.forwardHit.collider.name);
        }
    }
}
