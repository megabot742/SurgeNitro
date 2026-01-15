using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarShowManager : BaseManager<CarShowManager>
{
    [SerializeField] private Transform modelParent;  // Parent cho models (this.transform nếu attach vào "DisplayCar")
    [SerializeField] private CarDatabaseSO carDatabase;
    private Dictionary<string, GameObject> carModelPool = new Dictionary<string, GameObject>();
    private string currentShownCar;

    protected override void Awake()
    {
        base.Awake();
        if (carDatabase == null)
        {
            Debug.LogError("CarDatabaseSO is not assigned in CarShowManager Inspector!");
            return;
        }
        InitializePool();
    }
    private void InitializePool()
    {
        if (carModelPool.Count > 0) return;  // Đã init rồi thì skip

        //Debug.Log("CarShowManager: Initializing model pool...");

        List<CarParam> allCars = carDatabase.AllCars;
        if (allCars == null || allCars.Count == 0)
        {
            Debug.LogError("No cars in database! Check CarDatabaseSO.");
            return;
        }

        foreach (var car in allCars)
        {
            if (car.carShowModel == null)
            {
                Debug.LogWarning($"Car '{car.carName}' has no carShowModel prefab set!");
                continue;
            }

            GameObject model = Instantiate(car.carShowModel, modelParent);
            model.SetActive(false);  // Inactive ban đầu
            carModelPool.Add(car.carName, model);

            //Debug.Log($"Spawned model for: {car.carName}");
        }

        // Show xe default/lastShown ngay khi init
        string defaultCar = PlayerManager.Instance.GetLastShownCar();
        Debug.Log($"Showing default/last car: {defaultCar}");
        ShowCarModel(defaultCar);
    }

    public void ShowCarModel(string carName)
    {
        // BỔ SUNG: Chỉ cho phép show nếu đang ở scene Garage (an toàn cho multi-scene)
        if (SceneManager.GetActiveScene().name != "Garage")
        {
            Debug.Log("ShowCarModel skipped: Not in Garage scene");
            return;
        }

        if (string.IsNullOrEmpty(carName) && PlayerManager.HasInstance)
        {
            carName = PlayerManager.Instance.defaultCarName;
        }

        if (currentShownCar == carName) return;

        // Deactivate all
        foreach (var model in carModelPool.Values)
        {
            model.SetActive(false);
        }

        if (carModelPool.TryGetValue(carName, out GameObject selectedModel))
        {
            selectedModel.SetActive(true);
            currentShownCar = carName;

            // Update lastShownCar ở PlayerManager
            PlayerManager.Instance.SetLastShownCar(carName);
            //Debug.Log($"Show model success: {carName}");
        }
        else
        {
            if(PlayerManager.HasInstance)
            {
                Debug.LogWarning($"No model for car: {carName} | Pool size: {carModelPool.Count}");
                // Fallback: Show default nếu có
                if (carModelPool.TryGetValue(PlayerManager.Instance.defaultCarName, out var defaultModel))
                {
                    defaultModel.SetActive(true);
                    currentShownCar = PlayerManager.Instance.defaultCarName;
                }
            }
        }
    }
}
