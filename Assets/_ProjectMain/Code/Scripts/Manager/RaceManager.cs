using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;
    public CameraCarController cameraCarController;
    public FullScreenPassRendererFeature screenFeature;

    [Header("RaceSetup")]
    [SerializeField] public CheckPoint[] allCheckPoints;
    [SerializeField] public CarControllerBase playerCar;
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

    [SerializeField] public bool raceCompleted;

    void Awake()
    {
        Instance = this;
    }

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

    // Update is called once per frame
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
            TrackPlayerPosition(); //TrackPos over time
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
                float playerDistance = Vector3.Distance(playerCar.transform.position, allCheckPoints[playerCar.nextCheckPoint].transform.position);
                if (aiCar.currentLap > playerCar.currentLap) // Greater than Lap
                {
                    playerPos++;
                }
                else if (aiCar.currentLap == playerCar.currentLap) //Same lap, but greater than Checkpoint
                {
                    if (aiCar.nextCheckPoint > playerCar.nextCheckPoint)
                    {
                        playerPos++;
                    }
                    else if (aiCar.nextCheckPoint == playerCar.nextCheckPoint)
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
        //Get player Start Position
        playerStartPosition = Random.Range(0, aiNumberToSpawn + 1); //plus 1 because with Int Random.Range not include max value, only min value
        //Get info player car and spawn
        playerCar = Instantiate(playerCar, startPoints[playerStartPosition].position, startPoints[playerStartPosition].rotation, spawnCarParent);
        playerCar.AISetup(false);
        //Remove AI same car with Player, Set up from the bottom to avoid the influence of the top element
        for (int i = carsToSpawn.Count - 1; i >= 0; i--)
        {
            if (carsToSpawn[i] == playerCar)
            {
                Debug.Log(carsToSpawn[i]);
                carsToSpawn.RemoveAt(i);
            }
        }

        //Spawn AI
        for (int i = 0; i < aiNumberToSpawn + 1; i++) //example: 5 mean 5 AI to spawn, not 4 AI with 1 player, so plus 1
        {
            if (i != playerStartPosition)
            {
                int selectedCar = Random.Range(0, carsToSpawn.Count);

                allAICars.Add(Instantiate(carsToSpawn[selectedCar], startPoints[i].position, startPoints[i].rotation, spawnCarParent));

                if (carsToSpawn.Count > aiNumberToSpawn - i)
                {
                    carsToSpawn.RemoveAt(selectedCar);
                }
            }
        }
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
