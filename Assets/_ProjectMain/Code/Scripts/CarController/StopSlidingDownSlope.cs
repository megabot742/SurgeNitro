using UnityEngine;
[RequireComponent(typeof(CarControllerBase))]
[DisallowMultipleComponent]
public class StopSlidingDownSlope : MonoBehaviour
{
    [SerializeField, Min(0f)] private float maxSlopeAngle = 30f;
    [SerializeField, Min(0f)] private float maxSpeedKPH = 1f;

    private Rigidbody carRB;
    private CarControllerBase carControllerBase;
    private Vector3 gravityVector;

    private void Awake()
    {
        carRB = GetComponent<Rigidbody>();
        carControllerBase = GetComponent<CarControllerBase>();

        carRB.useGravity = false;
    }

    private void FixedUpdate()
    {
        UpdateGravity();
        AddGravity();
        AddGravity2();
    }

    private void UpdateGravity()
    {
        gravityVector = GetNewGravity();
    }

    private Vector3 GetNewGravity()
    {
        if (carControllerBase.SpeedKPH > maxSpeedKPH)
        {
            return UnityEngine.Physics.gravity;
        }

        var groundNormal = Vector3.zero;
        var count = 0;
        foreach (var wheel in carControllerBase.Wheels)
        {
            if (wheel.Grounded)
            {
                groundNormal += wheel.HitInfo.normal;
                count++;
            }
        }

        if (count == 0)
        {
            return UnityEngine.Physics.gravity;
        }

        groundNormal /= count;
        groundNormal.Normalize();
        var slopeAngle = Vector3.Angle(Vector3.up, groundNormal);

        if (slopeAngle > maxSlopeAngle)
        {
            return UnityEngine.Physics.gravity;
        }

        return -groundNormal * UnityEngine.Physics.gravity.magnitude;
    }

    private void AddGravity()
    {
        var force = gravityVector * carRB.mass;
        carRB.AddForce(force);
    }

    private void AddGravity2()
    {
        if (gravityVector == UnityEngine.Physics.gravity)
        {
            return;
        }

        if (carControllerBase.BrakeInput != 0f)
        {
            return;
        }

        var dir = Vector3.ProjectOnPlane(transform.forward, -gravityVector.normalized).normalized;
        var mg = carRB.mass * gravityVector.magnitude;
        var force = dir * mg * Mathf.Sin(transform.localEulerAngles.x * Mathf.Deg2Rad);
        carRB.AddForce(force);
    }
}
