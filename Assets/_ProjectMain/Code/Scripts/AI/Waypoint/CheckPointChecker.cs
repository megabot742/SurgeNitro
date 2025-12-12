using UnityEngine;

public class CheckPointChecker : MonoBehaviour
{
    public CarControllerBase CarController;
    void Awake()
    {
        CarController = GetComponent<CarControllerBase>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            //Debug.Log(other.GetComponent<CheckPoint>().checkPointNumber);
            CarController.CheckPointHit(other.GetComponent<CheckPoint>().checkPointNumber);
        }
    }
}
