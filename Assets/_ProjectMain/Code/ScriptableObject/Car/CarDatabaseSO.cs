using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car System/Car Database")]
public class CarDatabaseSO : ScriptableObject
{
    [Header("Class Containers")]
    [SerializeField] private List<CarClassContainerSO> classContainersList = new List<CarClassContainerSO>();

    // Cache CarClass
    private Dictionary<CarClass, CarClassContainerSO> classContainers;
    // Cache allcar
    private List<CarParam> allCarsCache = null;
    private bool cacheBuilt = false;

    // Gọi khi ScriptableObject được load (trong Editor và khi chơi game)
    private void OnEnable()
    {
        BuildContainerClass();
        BuildAllCarsCache(); // đảm bảo AllCars luôn sẵn sàng
    }

    // Tạo dictionary để lấy nhanh container theo class
    private void BuildContainerClass()
    {
        classContainers = new Dictionary<CarClass, CarClassContainerSO>();

        foreach (var container in classContainersList)
        {
            if (container != null && container.carClass != 0) // tránh lỗi enum = 0
            {
                // Nếu trùng class → ghi đè (giữ cái cuối cùng)
                classContainers[container.carClass] = container;
            }
        }
    }
    private void BuildAllCarsCache()
    {
        if (cacheBuilt) return;

        allCarsCache = new List<CarParam>();

        foreach (var container in classContainers.Values)
        {
            if (container != null && container.cars != null)
            {
                allCarsCache.AddRange(container.cars);
            }
        }

        cacheBuilt = true;
    }
    public int CarCount
    {
        get
        {
            return allCarsCache.Count;
        }
    }
    public CarParam GetCarParam(int index)
    {
        if (index >= 0 && index < allCarsCache.Count)
        {
            return allCarsCache[index];
        }
        return null;
    }

    //Get container class
    public CarClassContainerSO GetContainer(CarClass carClass)
    {
        classContainers.TryGetValue(carClass, out var container);
        return container;
    }

    //Get car list in class
    public List<CarParam> GetCarsInClass(CarClass carClass)
    {
        var container = GetContainer(carClass);
        if (container != null && container.cars != null)//check container & car
        {
            return container.cars;
        }
        return new List<CarParam>(); //null list
    }

    //Get car number in class
    public int GetCarCountInClass(CarClass carClass)
    {
        var container = GetContainer(carClass);
        if (container != null)
        {
            return container.Count;
        }

        return 0;
    }

    //Get random car in class
    public CarParam GetRandomCarInClass(CarClass carClass)
    {
        var container = GetContainer(carClass);
        if (container != null && container.Count > 0)//Check container & check class car
        {
            int randomIndex = Random.Range(0, container.Count); //Get random in range
            return container.GetCar(randomIndex); //Get car with index random
        }
        return null;
    }

    //Get car with best Rank in Class
    public CarParam GetBestCarInClass(CarClass carClass)
    {
        var cars = GetCarsInClass(carClass);
        if (cars.Count == 0) return null;

        CarParam best = cars[0];
        foreach (var car in cars)
        {
            if (car.carRank > best.carRank)
                best = car;
        }
        return best;
    }
    //Get class with available car
    public List<CarClass> GetAvailableClasses()
    {
        return new List<CarClass>(classContainers.Keys);
    }
}
