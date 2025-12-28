using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RaceManager : BaseManager<RaceManager>
{
    public CameraManager cameraManager;
    public InputActionAsset inputActions;
    public FullScreenPassRendererFeature screenFeature;
    [Header("GetParam")] //All of the variables below are not fixed. When the map is loaded, it will be updated according to the map
    [SerializeField] public CheckPoint[] allCheckPoints;
    [SerializeField] Transform[] startPoints;
    [SerializeField] GameObject spawnCarParent;
    private const string CHECK_POINT_PARENT = "CheckPointParent";
    private const string CHECK_POINT = "CheckPoint";
    private const string START_POINT_PARENT = "StartPointParent";
    private const string START_POINT = "StartPoint";
    private const string SPAWN_POINT_PARENT = "SpawnPointParent";

    [Header("RaceSetup")]
    [SerializeField] public GameObject playerCarPrefab;
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

    [Header("Car Database Integration")]
    [SerializeField] private CarDatabaseSO carDatabase;  //CarDatabaseS Param
    [SerializeField] private bool allowCarResemblePlayer = true;  //True: AI can be like a player; False: AI must be different from player (but AI can be the same)

    [SerializeField] public bool raceCompleted;

    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsRaceScene(scene.name))
        {
            Debug.Log("[RaceManager] Not in a race scene, skip init.");
            ClearRaceReferences();
            return;
        }
        ResetRaceState();

        InitRace();
    }
    private bool IsRaceScene(string sceneName)
    {
        return sceneName == "R&D" || sceneName.StartsWith("Track");
    }
    private void ResetRaceState()
    {
        // Reset countdown to default
        isCountdown = false;  
        countDownCurrent = 3f;  // Reset về 3 (default)
        timeCountdown = timeBetweenStartCount;  // Reset về 1f (default)

        // Reset các vars khác nếu cần (dựa trên code bạn)
        allAICars.Clear();  // Clear list AI cũ để tránh duplicate khi respawn
        raceCompleted = false;  // Reset race end
        playerPos = 0;  // Reset position
        positionCheckCounter = 0;  // Reset counter

        // Nếu có vars khác bị giữ state (ví dụ totalLaps nếu dynamic), reset ở đây
        // totalLaps = defaultValue; 

        Debug.Log("[RaceManager] Reset race state for new/reloaded scene.");
    }
    void InitRace()
    {
        FindCheckPoint();
        FindStartPoint();
        spawnCarParent = GameObject.FindGameObjectWithTag(SPAWN_POINT_PARENT);

        //Loop all checkpoint, add checkPointNumber
        for (int i = 0; i < allCheckPoints.Length; i++)
        {
            allCheckPoints[i].checkPointNumber = i;
        }
        //Start Countdown
        isCountdown = true;
        timeCountdown = timeBetweenStartCount;

        SpawnCarWithStartPoint(); //Spawn Car, Include Player and AI
        TrackPlayerPosition(); //Track Pos when star game
        DisplayCountDown(); //Show Countdown
    }
    void FindCheckPoint()
    {
        //Find CheckPointParent
        GameObject checkPointParent = GameObject.Find(CHECK_POINT_PARENT);
        if (checkPointParent == null)
        {
            //Debug.LogError("GameObject 'AllCheckPoint' was not found in the scene");
            allCheckPoints = new CheckPoint[0];
            return;
        }
        //Get all child in CheckPointParent (not include Parent)
        CheckPoint[] checkPoint = checkPointParent.GetComponentsInChildren<CheckPoint>(true); // true = include inactive
        //Sort array
        System.Array.Sort(checkPoint, (a, b) =>
        {
            // Extract number form name, EX: CheckPoint (5) -> 5
            int indexA = ExtractIndexFromName(a.gameObject.name);
            int indexB = ExtractIndexFromName(b.gameObject.name);
            return indexA.CompareTo(indexB);
        });
        allCheckPoints = checkPoint; //Add to array
    }
    void FindStartPoint()
    {
        //Find StartPointParent
        GameObject startPointParent = GameObject.Find(START_POINT_PARENT);
        if (startPointParent == null)
        {
            //Debug.LogError("GameObject 'AllStartPoint' was not found in the scene");
            startPoints = new Transform[0];
            return;
        }
        //Get all child in StarPointParent (not include Parent)
        Transform[] children = startPointParent.GetComponentsInChildren<Transform>(true);
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform child in children)
        {
            if (child != startPointParent.transform)
            {
                validPoints.Add(child); //Only child
            }
        }
        //Sort name
        validPoints.Sort((a, b) =>
        {
            // Extract number form name, EX: StartPoint (2) -> 2
            int indexA = ExtractIndexFromName(a.gameObject.name);
            int indexB = ExtractIndexFromName(b.gameObject.name);
            return indexA.CompareTo(indexB);
        });
        startPoints = validPoints.ToArray(); //Add to array
    }
    private int ExtractIndexFromName(string objectName)
    {
        //Case: If "StartPoint" or "CheckPoint" ->  index = 0
        if (objectName == START_POINT || objectName == CHECK_POINT)
        {
            return 0;
        }
        //Case: If find in ()
        int startIndex = objectName.IndexOf('(');
        int endIndex = objectName.IndexOf(')');
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string numberStr = objectName.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (int.TryParse(numberStr, out int index))
            {
                return index;
            }
        }
        //If can't parseable -> put last
        return int.MaxValue;
    }
    private void ClearRaceReferences()
    {
        allAICars.Clear();  // Clear list AI
        playerCarController = null;  // Null player refs
        inputCarController = null;
        playerStats = null;
        spawnCarParent = null;  // Null parent nếu cần
        allCheckPoints = null;  // Clear checkpoints
        startPoints = null;

        // Reset flags để skip logic
        isCountdown = false;
        raceCompleted = true;  // Giả end race để skip Update if cần

        Debug.Log("[RaceManager] Cleared race references for non-race scene.");
    }
    void Update()
    {
        if (!IsRaceScene(SceneManager.GetActiveScene().name))
        {
            return;
        }
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
        if (playerCarController == null || allAICars == null)
        {
            return;
        }
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
        playerCarController = playerCarPrefab.GetComponent<CarControllerBase>();
        //playerStats = playerCarPrefab.GetComponent<CarStatsProvider>();
        // inputCarController = playerCarPrefab.GetComponent<InputCarController>();
        #region Spawnplayer
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
        CarParam playerCarParam = allCarsInClass.Find(car => car.carPrefab == playerCarPrefab);
        if (playerCarParam == null)
        {
            Debug.LogError($"CarParam not found for player car prefab {playerCarPrefab.name} in class {playerClass}!");
        }

        //Get player Start Position
        playerStartPosition = Random.Range(0, aiNumberToSpawn + 1); //plus 1 because with Int Random.Range not include max value, only min value

        //Get info player car and spawn
        GameObject playerCarSpawn = Instantiate(playerCarPrefab, startPoints[playerStartPosition].position, startPoints[playerStartPosition].rotation, spawnCarParent.transform);
        playerCarController = playerCarSpawn.GetComponent<CarControllerBase>();
        inputCarController = playerCarSpawn.GetComponent<InputCarController>();
        inputCarController.isAICar = false;
        inputCarController.SetupPlayerInput(true);

        playerCarSpawn.name = "PlayerCar"; //Add name
        playerCarSpawn.tag = "Player"; //Add tag

        //Check PlayerInput
        if (inputCarController != null)
        {
            PlayerInput playerInput = inputCarController.GetComponent<PlayerInput>();
            if (playerInput != null && !playerInput.enabled)
            {
                playerInput.enabled = true;
                Debug.Log("Forced enable PlayerInput for player after spawn.");
            }
        }
        #endregion

        #region SpawnAI
        List<CarParam> availableAICars = new List<CarParam>(allCarsInClass);
        // Remove AI car like player if not allow
        if (!allowCarResemblePlayer && playerCarParam != null)
        {
            availableAICars.Remove(playerCarParam);
            if (availableAICars.Count == 0)
            {
                Debug.LogWarning($"No cars left in class {playerClass} after removing duplicate with player! Allowing resemble as fallback.");
                availableAICars = new List<CarParam>(allCarsInClass);  // Fallback to avoid empty
            }
        }
        //Random car from availableAICars
        for (int i = 0; i < aiNumberToSpawn + 1; i++) // example: 5 mean 5 AI to spawn, not 4 AI with 1 player, so plus 1
        {
            if (i != playerStartPosition)
            {
                if (availableAICars.Count == 0)
                {
                    Debug.LogError("No cars available to spawn AI!");
                    continue;
                }

                // Random CarParam from list (allow duplicate AI with each other, but respect allowCarResemblePlayer)
                int selectedIndex = Random.Range(0, availableAICars.Count);
                CarParam selectedCarParam = availableAICars[selectedIndex];

                // Instantiate from carPrefab, get CarControllerBase
                GameObject aiCarObj = Instantiate(selectedCarParam.carPrefab, startPoints[i].position, startPoints[i].rotation, spawnCarParent.transform);
                CarControllerBase aiController = aiCarObj.GetComponent<CarControllerBase>();
                if (aiController == null)
                {
                    Debug.LogError($"Prefab {selectedCarParam.carPrefab.name} does not have CarControllerBase!");
                    Destroy(aiCarObj);  // Cleanup if error
                    continue;
                }
                InputCarController aiInputController = aiCarObj.GetComponent<InputCarController>();
                aiInputController.isAICar = true;
                //aiInputController.useVirtualPad = false;
                aiInputController.SetupPlayerInput(false);  // Setup AI (disable input)
                aiCarObj.tag = "Untagged"; //default tag

                PlayerInput aiPlayerInput = aiInputController.GetComponent<PlayerInput>();
                if (aiPlayerInput != null && aiPlayerInput.enabled)
                {
                    aiPlayerInput.enabled = false;
                    Debug.Log("Forced disable PlayerInput for AI car: " + aiCarObj.name);
                }

                allAICars.Add(aiController);
            }
        }
        #endregion

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
