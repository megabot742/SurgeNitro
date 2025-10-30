using UnityEngine;
[DisallowMultipleComponent]
public class ChaseAI : DriverBase
{
    [SerializeField] CarControllerBase targetCar;

    [SerializeField, Min(0f)] private float _minSpeedKPH = 30f;
    [SerializeField, Min(0f)] private float _maxSpeedKPH = 120f;
    [SerializeField, Min(1f)] private float _transitionKPH = 1f;

    public CarControllerBase TargetCar
    {
        get => targetCar;
        set => targetCar = value;
    }

    protected override void Drive()
    {
        var minSpeed = _minSpeedKPH * CarMath.KPHToMPS;
        var maxSpeed = _maxSpeedKPH * CarMath.KPHToMPS;
        var transition = _transitionKPH * CarMath.KPHToMPS;

        if (targetCar == null)
        {
            carController.SteerInput = 0f;
            carController.ThrottleInput = 0f;
            carController.BrakeInput = 1f;
            return;
        }

        var targetPos = targetCar.transform.position;
        var localTargetPos = transform.InverseTransformPoint(targetPos);
        localTargetPos.y = 0f;

        var targetAngle = Vector3.SignedAngle(Vector3.forward, localTargetPos, Vector3.up);

        var steerInput = Mathf.Clamp(targetAngle / carController.MaxSteerAngle, -1f, 1f);

        var targetSpeed = Mathf.Lerp(maxSpeed, minSpeed, Mathf.Abs(steerInput));

        var throttleInput = Mathf.Max((targetSpeed - carController.Speed) / transition, 0f);

        var brakeInput = Mathf.Max((carController.Speed - targetSpeed) / transition, 0f);

        carController.SteerInput = steerInput;
        carController.ThrottleInput = throttleInput;
        carController.BrakeInput = brakeInput;
    }
}
