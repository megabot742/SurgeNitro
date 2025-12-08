using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Car System/Class Container")]
public class CarClassContainerSO : ScriptableObject
{
    public CarClass carClass;
    public List<CarParam> cars = new List<CarParam>(); //Car list
    private void OnValidate() //Auto sync in Inspector or Load
    {
        SyncCarClassToAllCars();
    }
    private void OnEnable() //Auto sync when load first time
    {
        SyncCarClassToAllCars();
    }
    public void SyncCarClassToAllCars() //Sync class from container to all car
    {
        if (cars == null) return;

        foreach (var car in cars) //Check car in class
        {
            if (car != null)
            {
                car.carClass = this.carClass; //FORCE the correct class of the container
            }
        }
    }
    //Function
    public int Count
    {
        get
        {
            return cars.Count;
        }
    }

    //Get index car
    public CarParam GetCar(int index)
    {
        if (index >= 0 && index < cars.Count)//check car index
        {
            return cars[index];
        }
        return null;
    }

    //Get random
    public CarParam GetRandomCar()
    {
        if (cars.Count > 0) //check car in class
        {
            int randomIndex = Random.Range(0, cars.Count);
            return cars[randomIndex];
        }
        return null;
    }
}
