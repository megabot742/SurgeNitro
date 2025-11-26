using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Speed set once we reach the waypoint")]
    [SerializeField] public float maxSpeed = 0;
    
    [Header("This is the waypoint we are going towards, not yet reached")]
    [SerializeField] public float minDistanceToReachWaypoint = 5;
    [SerializeField] public WaypointNode[] nextWaypointNode;
}
