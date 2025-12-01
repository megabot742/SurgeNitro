using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Speed set once we reach the waypoint")]
    [SerializeField, Range(0.1f, 1f)] public float maxSpeedMultiplier = 1; //default 1 = 100% speed
    
    [Header("This is the waypoint we are going towards, not yet reached")]
    [SerializeField, Min(0f)] public float minDistanceToReachWaypoint = 10; //default 10

    [SerializeField] public WaypointNode[] nextWaypointNode;

    private void OnDrawGizmos()
    {
        if(minDistanceToReachWaypoint > 0)
        {
            Gizmos.color = Color.yellow; // Nếu radius=0, vẽ nhỏ với minDistance
            Gizmos.DrawWireSphere(transform.position, minDistanceToReachWaypoint);
        }
    }
}
