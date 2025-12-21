using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;
    public CameraCarController cameraCarController;
    public FullScreenPassRendererFeature screenFeature;

    [Header("RaceSetup")]
    [SerializeField] public CheckPoint[] allCheckPoints;
    [SerializeField] public GameObject playerCarObject;
    [SerializeField] public CarControllerBase playerCarController;
    [SerializeField] public InputCarController inputCarController;
    [SerializeField] public CarStatsProvider playerStats;
    [SerializeField] public List<CarControllerBase> allAICars = new List<CarControllerBase>();
    [SerializeField] public int totalLaps;
    [SerializeField] int playerPos;
    [SerializeField] float timeBetweenPosCheck = 0.2f;
    private float positionCheckCounter = 0;

    [Header("Race Start Countdown")]
    [SerializeField] public bool isCountdown; //Check countdown time 
    [SerializeField] float timeBetweenStartCount = 1f; //Default 1 mean, countdown -1 every second
    [SerializeField] float countDownCurrent = 3; //Default 3, game will start after 3s 
    private float timeCountdown;

    [Header("StartPosition")]
    [SerializeField] int playerStartPosition;
    [SerializeField] int aiNumberToSpawn;
    [SerializeField] Transform[] startPoints;
    [SerializeField] Transform spawnCarParent;
    [SerializeField] List<CarControllerBase> carsToSpawn = new List<CarControllerBase>();

    [Header("Car Database Integration")]
    [SerializeField] private CarDatabaseSO carDatabase;  //CarDatabaseS Param
    [SerializeField] private bool allowDuplicateWithPlayer = true;  //True: AI can be like a player; False: AI must be different from player (but AI can be the same)

    [SerializeField] public bool raceCompleted;

    void Awake()
    {
        Instance = this;
        playerCarController = playerCarObject.GetComponent<CarControllerBase>();
        playerStats = playerCarObject.GetComponent<CarStatsProvider>();
        inputCarController = playerCarObject.GetComponent<InputCarController>()
 ;   }
    void Start()
    {
        //Loop all checkpoint, add checkPointNumber
        for (int i = 0; i < allCheckPoints.Length; i++)
        {
            allCheckPoints[i].checkPointNumber = i;
        }
        //Start Countdown
        isCountdown = true;
        timeCountdown = timeBetweenStartCount;

        SpawnCarWithStartPoint(); //Spawn Car, include Player and AI
        TrackPlayerPosition(); //Track Pos when star game
        DisplayCountDown(); //Show Countdown
    }
    void Update()
    {
        if (isCountdown)
        {
            timeCountdown -= Time.deltaTime;
            if (timeCountdown <= 0)
            {
                countDownCurrent--;
                timeCountdown = timeBetweenStartCount;
                DisplayCountDown();

                if (countDownCurrent == 0)
                {
                    isCountdown = false;
                    if (UIManager.HasInstance)
                    {
                        UIManager.Instance.hUDPanel.countDownNum.gameObject.SetActive(false);
                        UIManager.Instance.hUDPanel.countDownTxt.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            TrackPlayerPosition(); //Track Pos over time
        }

    }
    void TrackPlayerPosition()
    {
        positionCheckCounter -= Time.deltaTime;
        if (positionCheckCounter <= 0)
        {
            playerPos = 1;

            foreach (CarControllerBase aiCar in allAICars)
            {
                float aiDistance = Vector3.Distance(aiCar.transform.position, allCheckPoints[aiCar.nextCheckPoint].transform.position);
                float playerDistance = Vector3.Distance(playerCarController.transform.position, allCheckPoints[playerCarController.nextCheckPoint].transform.position);
                if (aiCar.currentLap > playerCarController.currentLap) // Greater than Lap
                {
                    playerPos++;
                }
                else if (aiCar.currentLap == playerCarController.currentLap) //Same lap, but greater than Checkpoint
                {
                    if (aiCar.nextCheckPoint > playerCarController.nextCheckPoint)
                    {
                        playerPos++;
                    }
                    else if (aiCar.nextCheckPoint == playerCarController.nextCheckPoint)
                    {
                        if (aiDistance < playerDistance) //Same lap, same check point, lessthan Distance
                        {
                            playerPos++;
                        }
                    }

                }
            }
            positionCheckCounter = timeBetweenPosCheck;
        }
        if (UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.posTxt.text = playerPos + "/" + (allAICars.Count + 1);
        }

    }

    void DisplayCountDown()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.countDownNum.text = countDownCurrent + "!";
        }
    }

    void SpawnCarWithStartPoint()
    {
        //Check player Car Class 
        CarClass playerClass = playerCarController.CarClass;
        // Get container car class like car class player
        CarClassContainerSO container = carDatabase.GetContainer(playerClass);
        if (container == null) //Check
        {
            Debug.LogError($"No container found for class {playerClass}!!");
            return;
        }
        // Get a list of all CarParams in the class
        List<CarParam> allCarsInClass = new List<CarParam>(container.cars);  // Copy to not modify origin

        //Find the CarParam corresponding to the player car (based on the matching prefab)
        CarParam playerCarParam = allCarsInClass.Find(car => car.carPrefab == playerCarObject);
        if (playerCarParam == null)
        {
            Debug.LogError($"CarParam not found for player car prefab {playerCarObject.name} in class {playerClass}!");
        }


        //Get player Start Position
        playerStartPosition = Random.Range(0, aiNumberToSpawn + 1); //plus 1 because with Int Random.Range not include max value, only min value
        //Get info player car and spawn
        playerCarController = Instantiate(playerCarController, startPoints[playerStartPosition].position, startPoints[playerStartPosition].rotation, spawnCarParent);
        //playerCarController.AISetup(false); //Set car for player
        inputCarController.isAICar = false;
        inputCarController.SetupPlayerInput(true);
        

        List<CarParam> availableAICars = new List<CarParam>(allCarsInClass);
        //Remove AI car like player
        if (!allowDuplicateWithPlayer && playerCarParam != null)
        {
            availableAICars.Remove(playerCarParam);
            if (availableAICars.Count == 0)
            {
                Debug.LogWarning($"Không còn car nào trong class {playerClass} sau khi remove duplicate với player! Cho phép duplicate AI.");
                availableAICars = new List<CarParam>(allCarsInClass);  // Fallback để tránh empty
            }
        }
        //Random car from availableAICars
        for (int i = 0; i < aiNumberToSpawn + 1; i++) //example: 5 mean 5 AI to spawn, not 4 AI with 1 player, so plus 1
        {
            if (i != playerStartPosition)
            {
                if (availableAICars.Count == 0)
                {
                    Debug.LogError("Không có car nào để spawn AI!");
                    continue;
                }

                // Random CarParam từ list (cho phép duplicate vì không remove sau random)
                int selectedIndex = Random.Range(0, availableAICars.Count);
                CarParam selectedCarParam = availableAICars[selectedIndex];

                // Instantiate từ carPrefab, get CarControllerBase
                GameObject aiCarObj = Instantiate(selectedCarParam.carPrefab, startPoints[i].position, startPoints[i].rotation, spawnCarParent);
                CarControllerBase aiController = aiCarObj.GetComponent<CarControllerBase>();
                if (aiController == null)
                {
                    Debug.LogError($"Prefab {selectedCarParam.carPrefab.name} không có CarControllerBase!");
                    Destroy(aiCarObj);  // Cleanup nếu lỗi
                    continue;
                }
                InputCarController aiInputController = aiCarObj.GetComponent<InputCarController>();
                aiInputController.isAICar = true;
                aiInputController.SetupPlayerInput(false);
                //aiController.AISetup(true);  // Setup là AI
                allAICars.Add(aiController);
            }
        }
        // //Remove AI same car with Player, Set up from the bottom to avoid the influence of the top element
        // for (int i = carsToSpawn.Count - 1; i >= 0; i--)
        // {
        //     if (carsToSpawn[i] == playerCarController)
        //     {
        //         Debug.Log(carsToSpawn[i]);
        //         carsToSpawn.RemoveAt(i);
        //     }
        // }

        // //Spawn AI
        // for (int i = 0; i < aiNumberToSpawn + 1; i++) //example: 5 mean 5 AI to spawn, not 4 AI with 1 player, so plus 1
        // {
        //     if (i != playerStartPosition)
        //     {
        //         int selectedCar = Random.Range(0, carsToSpawn.Count);

        //         allAICars.Add(Instantiate(carsToSpawn[selectedCar], startPoints[i].position, startPoints[i].rotation, spawnCarParent));

        //         if (carsToSpawn.Count > aiNumberToSpawn - i)
        //         {
        //             carsToSpawn.RemoveAt(selectedCar);
        //         }
        //     }
        // }
    }
    public void FinishRace()
    {
        if (!raceCompleted)
        {
            raceCompleted = true;
            if (UIManager.HasInstance) //RaceInfoManager.HasInstance
            {
                UIManager.Instance.StopCountdown(); // Stop countdown when race ends
                UIManager.Instance.resultPanel.gameObject.SetActive(true);
                string posTxt = GetOrdinalText(playerPos);
                UIManager.Instance.resultPanel.posNumberTxt.text = posTxt;


                inputCarController.SetupPlayerInput(false);
                //Unlock new track
                // if (playerPosition < 4 && !string.IsNullOrEmpty(RaceInfoManager.Instance.raceToUnlock))
                // {
                //     // if (!PlayerPrefs.HasKey(RaceInfoManager.Instance.raceToUnlock + "_unlocked"))
                //     // {
                //     //     PlayerPrefs.SetInt(RaceInfoManager.Instance.raceToUnlock + "_unlocked", 1);
                //     //     PlayerPrefs.Save();
                //     //UIManager.Instance.hUDPanel.unlockRaceText.gameObject.SetActive(true);
                //     // }
                // }
            }
        }

    }

    string GetOrdinalText(int number)
    {
        if (number <= 0) return number.ToString();

        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }

        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }

}
