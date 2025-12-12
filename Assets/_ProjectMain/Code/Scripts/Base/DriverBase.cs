using UnityEngine;

[RequireComponent(typeof(CarControllerBase))]
public class DriverBase : MonoBehaviour
{
    protected CarControllerBase carController;

    private bool stopping;

    public CarControllerBase CarController => carController;

    public bool Stopping
    {
        get => stopping;
        set => stopping = value;
    }

    protected virtual void Awake()
    {
        carController = GetComponent<CarControllerBase>();
    }

    private void FixedUpdate()
    {
        if (stopping)
        {
            Stop();
        }
        else
        {
            Drive();
        }
    }

    protected virtual void Drive()
    {

    }

    protected virtual void Stop()
    {
        carController.SteerInput = 0f;
        carController.ThrottleInput = 0f;
        carController.BrakeInput = 1f;
    }
}
