using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Speed set once we reach the waypoint")]
    [SerializeField, Range(0.1f, 1f)] public float maxSpeedMultiplier = 1; //default 1 = 100% speed
    [SerializeField, Range(0.1f, 1f)] private float classDMultiplier = 1f; // Class D: default 1 = 100%
    [SerializeField, Range(0.1f, 1f)] private float classCMultiplier = 1f; // Class C
    [SerializeField, Range(0.1f, 1f)] private float classBMultiplier = 1f; // Class B
    [SerializeField, Range(0.1f, 1f)] private float classAMultiplier = 1f; // Class A
    [SerializeField, Range(0.1f, 1f)] private float classSMultiplier = 1f; // Class S
    
    [Header("This is the waypoint we are going towards, not yet reached")]
    [SerializeField, Min(0f)] public float minDistanceToReachWaypoint = 10; //default 10

    [SerializeField] public WaypointNode[] nextWaypointNode;
    public float GetMaxSpeedMultiplierForClass(CarClass carClass)
    {
        // Switch theo enum để trả về multiplier tương ứng
        switch (carClass)
        {
            case CarClass.classD:
                return classDMultiplier;
            case CarClass.classC:
                return classCMultiplier;
            case CarClass.classB:
                return classBMultiplier;
            case CarClass.classA:
                return classAMultiplier;
            case CarClass.classS:
                return classSMultiplier;
            default:
                return 1f; // defaullt hoặc không biết → 100%
        }
    }
    private void OnDrawGizmos()
    {
        if(minDistanceToReachWaypoint > 0)
        {
            Gizmos.color = Color.yellow; // Nếu radius=0, vẽ nhỏ với minDistance
            Gizmos.DrawWireSphere(transform.position, minDistanceToReachWaypoint);
        }
    }
}
