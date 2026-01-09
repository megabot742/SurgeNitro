using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen Garage: List car in ScrollView, Filter car with Class, handler CarInfo item
/// </summary>
public class ScreenGarage : BaseScreen
{
    [Header("References")]
    [SerializeField] private CarDatabaseSO carDatabase; //Car database
    [SerializeField] private Transform content; // Parent cho items (Content form ScrollView)
    [SerializeField] private GameObject itemPrefab; // Prefab OwnedItem

    [Header("Filter Visual")]
    [SerializeField] private Image currentClassBg;
    [SerializeField] private Color classColor;
    [SerializeField] private TMP_Text currentClassTxt;

    [Header("Toggle Group")]
    [SerializeField] private ToggleButtonGroup filterToggleGroup;

    private CarInfoData garageData;
    private List<ScrollItem> itemPool = new List<ScrollItem>(); // Pool
    private int poolSize = 10;
    //private int currentFilterIndex = 0; //default 
    public override void Init()
    {
        base.Init();
        poolSize = carDatabase.TotalCarCount + 10; //Calculate pool size with carData
        InitializePool(); //Create items in the pool in advance.
    }

    public override void Show(object data)
    {
        base.Show(data);
        garageData = data as CarInfoData; //Save data
        //Show last filter index from screenCarInfo go back screenGarage
        if (filterToggleGroup != null && UIEventManager.HasInstance)
        {
            filterToggleGroup.SelectSilent(UIEventManager.Instance.currentFilterIndexGarage); // Khôi phục toggle state/colors mà không trigger event
        }
       //ShowAllCars(); //Show item in garage
        OnFilterButtonSelected(UIEventManager.Instance.currentFilterIndexGarage);
        GameEvent.OnFilterButtonSelected += OnFilterButtonSelected; //Subscribe events

    }

    public override void Hide()
    {
        base.Hide();
        DeactivateAllItems(); //Hide item in garage
        //Reset filter button
        
        GameEvent.OnFilterButtonSelected -= OnFilterButtonSelected; //Unsubscribe to avoid memory leak

    }
    
    #region Object Pooling 
    public void ShowAllCars()
    {
        //Step 1: Get all car in class
        List<CarParam> allCars = new List<CarParam>();
        foreach (CarClass carClass in carDatabase.GetAvailableClasses())
        {
            allCars.AddRange(carDatabase.GetCarsInClass(carClass));
        }
        // Step 2: Activate item with car list
        ActivateAllItems(allCars);
        // Step 3: Update visual filter
        UpdateFilterVisual("All", classColor); //default all filter
    }
    private void InitializePool()
    {
        if (itemPool.Count > 0) return; //Check pool

        //Loop and creat item with pool size
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(itemPrefab, content); // Instantiate to parent Content
            obj.SetActive(false); // Inactive first
            ScrollItem item = obj.GetComponent<ScrollItem>();
            if (item != null)
            {
                itemPool.Add(item);
            }
        }
    }
    #endregion
    #region Active & Deactive Item
    private void ActivateAllItems(List<CarParam> carsToShow)
    {
        DeactivateAllItems(); //Clear first

        // Step 1: Loop through the vehicle list and activate the corresponding item.
        int index = 0;
        foreach (var car in carsToShow)
        {
            if (index >= itemPool.Count) break; //Check safe without out pool

            ScrollItem item = itemPool[index];
            item.gameObject.SetActive(true);
            item.SetData(car, carDatabase); //Set data car for item.

            // Step 2: Attach listener for the item's button.
            Button itemButton = item.GetComponent<Button>();
            if (itemButton != null)
            {
                item.GetComponent<Button>().onClick.RemoveAllListeners(); //Clear old listener, avoid duplicate
                CarParam localCar = car; // Capture to avoid closure issue in loop
                item.GetComponent<Button>().onClick.AddListener(() => OnClickItemCarInfo(localCar)); // Attach listener calls OnItemSelected with the specified car
            }

            // Step 3: increase the next value
            index++;
        }
    }

    private void DeactivateAllItems()
    {
        //Loop pool and inactive each item.
        foreach (ScrollItem item in itemPool)
        {
            if(item == null) return;
            item.gameObject.SetActive(false);
            item.ResetItem(); //reset data for reuse
        }
    }
    #endregion
    #region Handler Filter
    //-----Text and color show current Filter Class-----
    private void UpdateFilterVisual(string displayText, Color bgColor)
    {
        if (currentClassTxt != null)
        {
            if (displayText == "All")
                currentClassTxt.text = "All Cars";
            else
                currentClassTxt.text = "Class " + displayText;
        }

        if (currentClassBg != null)
        {
            currentClassBg.color = bgColor;
        }
    }
    //----- Get color from Car class in  database -----
    private Color GetClassColor(CarClass carClass)
    {
        return carDatabase.GetClassColor(carClass);
    }
    //Update class car list & update visual
    private void FilterAndUpdate(CarClass carClass, string displayText)
    {
        //Step 1: Get the list of vehicles by class.
        var carsInClass = carDatabase.GetCarsInClass(carClass);
        // Step 2: Activate items with the list.
        ActivateAllItems(carsInClass);
        // Step 3: Get color and update visual.
        Color classColor = GetClassColor(carClass);
        UpdateFilterVisual(displayText, classColor);
    }
    //Handling when the filter button is selected from toggle button 
    private void OnFilterButtonSelected(int selectedIndex)
    {
        if(UIEventManager.HasInstance)
        {
            UIEventManager.Instance.currentFilterIndexGarage = selectedIndex;
        }
        // Button index: 0 = All, 1 = D, 2 = C, 3 = B, 4 = A, 5 = S , Total 6
        switch (selectedIndex)
        {
            case 0: // All
                ShowAllCars();
                UpdateFilterVisual("All", classColor);
                break;

            case 1: // D
                FilterAndUpdate(CarClass.classD, "D");
                break;

            case 2: // C
                FilterAndUpdate(CarClass.classC, "C");
                break;

            case 3: // B
                FilterAndUpdate(CarClass.classB, "B");
                break;

            case 4: // A
                FilterAndUpdate(CarClass.classA, "A");
                break;

            case 5: // S
                FilterAndUpdate(CarClass.classS, "S");
                break;
        }
    }
    //-----Button item fuction-----
    public void OnClickFilterByClass(string classFilter)
    {
        CarClass selected;

        // Step 1: Handle fallback if "All" or empty.
        if (classFilter == "All" || string.IsNullOrEmpty(classFilter))
        {
            ShowAllCars();
            UpdateFilterVisual("All", Color.white); // Default white
            return;
        }
        // Step 2: Map string to CarClass.
        else if (classFilter == "D")
            selected = CarClass.classD;
        else if (classFilter == "C")
            selected = CarClass.classC;
        else if (classFilter == "B")
            selected = CarClass.classB;
        else if (classFilter == "A")
            selected = CarClass.classA;
        else if (classFilter == "S")
            selected = CarClass.classS;
        else
        {
            ShowAllCars(); // fallback all car
            return;
        }

        // Step 3: Filter and update.
        var carsInClass = carDatabase.GetCarsInClass(selected);
        ActivateAllItems(carsInClass);

        string displayText = classFilter; // "D", "C",...
        Color classColor = GetClassColor(selected);
        UpdateFilterVisual(displayText, classColor);
    }
    #endregion
    #region OtherButton
    //Create detailed data and transfer to ScreenCarInfo.
    private void OnClickItemCarInfo(CarParam selectedCar)
    {
        if (selectedCar == null) return;

        CarInfoMode mode = CarInfoMode.View; //Create variables (View = Default)
        if (garageData != null) //Check data
        {
            mode = garageData.Mode; //Get mode data for change screen
        }

        CarInfoData infoData = new CarInfoData
        {
            Car = selectedCar,
            Mode = mode //Set mode data for data transmission
        };
        //Change screen
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.CarInfoBtn(infoData);
        }
        //Hide this screen
        this.Hide();
    }
    public void OnClickGoBack()
    {
        //Go back ScreenHome
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        if (filterToggleGroup != null && UIEventManager.HasInstance)
        {
            UIEventManager.Instance.currentFilterIndexGarage = 0;//reset to default
            filterToggleGroup.SelectSilent( UIEventManager.Instance.currentFilterIndexGarage);
        }
        //ShowAllCars(); //reset all car screenGarage
        //Hide this screen
        this.Hide();
    }
    #endregion
}
