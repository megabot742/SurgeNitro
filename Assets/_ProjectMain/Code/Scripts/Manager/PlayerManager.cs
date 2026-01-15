using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PlayerManager : BaseManager<PlayerManager>
{
    [Header("List car owner")]
    [SerializeField] private List<PlayerCarData> ownedCars = new List<PlayerCarData>();
    [SerializeField] private CarDatabaseSO carDatabase;

    [Header("Current Car for Race")]
    [SerializeField] private string currentCarName; // Lưu vào prefs
    [SerializeField] private GameObject currentCarPrefab;

    [Header("Map data")]
    [SerializeField] private TrackDatabaseSO trackDatabase;
    [SerializeField] private string currentTrack; // Track đang chọn
    [SerializeField] private string baseTrack; // Track default (từ database)
    [Header("Race Settings")]
    [SerializeField] private int currentLap; // Số lap hiện tại
    [SerializeField] private int currentAISpawn; // Số AI spawn hiện tại
    private int defaultValue = 1; // Default cho lap và AI khi reset

    [Header("Sound data")]
    [SerializeField] private float SFXVolume = 1f;  // Âm lượng sound
    [SerializeField] private float musicVolume = 1f;  // Âm lượng music

    // Thêm ngay dưới đây:
    [SerializeField] public AudioMixer audioMixer;         // AudioMixer để điều chỉnh volume
    [SerializeField] private AudioSource musicPlayer;       // AudioSource để phát nhạc nền (tạo GameObject con với AudioSource và kéo vào đây)
    [SerializeField] private AudioClip[] menuAudioClips;    // Array nhạc cho Menu/Garage/Shop
    [SerializeField] private AudioClip[] raceAudioClips;    // Array nhạc cho Race/Track
    private AudioClip[] currentClipList;                    // Danh sách clip hiện tại theo scene
    private int currentClipIndex;
    private const float MIN_DB = -80f;


    [Header("Currency")]
    [SerializeField] private long coin = 1000000;

    [Header("Default Settings")]
    [SerializeField] public string defaultCarName = "Supernova";

    [Header("Last Shown Car")]
    [SerializeField] private string lastShownCar = "";
    [Header("Debug Options")]
    [SerializeField] private bool disableSaving = false;

    private const string PLAYER_KEY = "Player";
    private const int DATA_VERSION = 1;
    protected override void Awake()
    {
        base.Awake();

        if (trackDatabase != null && trackDatabase.TrackCount > 0)
        {
            baseTrack = trackDatabase.GetTrackSO(0).idTrack;
        }
        else
        {
            Debug.LogWarning("No track data");
        }
        LoadPlayerPrefs();
        //Get volume
        GetSFXVolume();
        GetMusicVolume();
        ApplyVolumesToMixer();
        

        currentCarPrefab = LoadCarPrefabFromDatabase(currentCarName);
        if (currentCarPrefab == null)
        {
            currentCarName = defaultCarName;
            currentCarPrefab = LoadCarPrefabFromDatabase(defaultCarName);
            SavePlayerPrefs();
        }

    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnPlayerSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnPlayerSceneLoaded;
    }

    private void OnPlayerSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
    }
    void Start()
    {
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetPlayerPrefs();
        }
        //Check music
        // Thêm ngay dưới đây: Check nếu nhạc hết thật sự (không phải pause) thì next track
        if (musicPlayer != null && !musicPlayer.isPlaying && currentClipList != null && currentClipList.Length > 0)
        {
            PlayNextMusicClip();  // Loop khi hết
        }
    }
    public bool IsOwned(string carName)
    {
        return ownedCars.Exists(c => c.carName == carName);  // Kiểm tra tồn tại = owned
    }
    #region PlayePrefs
    //Load  Player Prefs
    private void LoadPlayerPrefs()
    {
        string playerJson = PlayerPrefs.GetString(PLAYER_KEY, "");
        SaveDataWrapper wrapper = new SaveDataWrapper();

        if (!string.IsNullOrEmpty(playerJson))
        {
            wrapper = JsonUtility.FromJson<SaveDataWrapper>(playerJson);
        }

        if (wrapper.version != DATA_VERSION)
        {
            ResetPlayerPrefs();
            return;
        }

        // Load owned cars
        ownedCars.Clear();
        foreach (var savedCar in wrapper.ownedCars)
        {
            PlayerCarData playerData = new PlayerCarData
            {
                carName = savedCar.carName,
                currentRank = savedCar.currentRank,
                stats = new UpgradeStat[4]
            };

            CarParam originalCar = carDatabase.GetCarByName(savedCar.carName);
            if (originalCar == null) continue;

            for (int i = 0; i < 4; i++)
            {
                CarStatType type = (CarStatType)i;
                playerData.stats[i] = new UpgradeStat
                {
                    statType = type,
                    baseValue = originalCar.GetBaseValue(type),
                    maxValue = originalCar.GetMaxValue(type),
                    baseGold = originalCar.stats[i].baseGold
                };
                playerData.stats[i].CurrentLevel = savedCar.statLevels[i];  // Load level từ save
                playerData.stats[i].OnValidate();  // Tính currentValue và goldUpgrade
            }

            ownedCars.Add(playerData);
        }
        //Car name
        currentCarName = string.IsNullOrEmpty(wrapper.currentCarName) ? defaultCarName : wrapper.currentCarName;
        // Load unlocked maps
        currentTrack = string.IsNullOrEmpty(wrapper.currentTrack) ? baseTrack : wrapper.currentTrack;

        // // Load sound volumes
        // soundVolume = wrapper.soundVolume;
        // musicVolume = wrapper.musicVolume;

        // Load coin
        coin = wrapper.coin;

        // Load lastShownCar từ wrapper (fallback nếu null hoặc rỗng)
        lastShownCar = string.IsNullOrEmpty(wrapper.lastShownCar) ? defaultCarName : wrapper.lastShownCar;

        // Load race settings (fallback về defaultValue nếu <=0 hoặc invalid)
        currentLap = wrapper.currentLap > 0 ? wrapper.currentLap : defaultValue;
        currentAISpawn = wrapper.currentAISpawn >= 0 ? wrapper.currentAISpawn : defaultValue;

        // Load sound volumes with fallback
        SFXVolume = wrapper.SFXVolume >= 0.001f ? wrapper.SFXVolume : 1f;
        musicVolume = wrapper.musicVolume >= 0.001f ? wrapper.musicVolume : 1f;
        
        if(ownedCars == null) //When fist time play game
        {
            InitCarData(defaultCarName);
        }
        //Debug.Log($"Loaded volumes from prefs: SFX={SFXVolume}, Music={musicVolume}");
    }
    private void SavePlayerPrefs()
    {
        if (disableSaving)
        {
            Debug.Log("Saving disabled for testing!");
            return;
        }

        var wrapper = new SaveDataWrapper
        {
            version = DATA_VERSION,
            // SFXVolume = SFXVolume,
            // musicVolume = musicVolume,
            coin = coin,
            lastShownCar = lastShownCar ?? defaultCarName,
            currentTrack = currentTrack ?? baseTrack,
            currentCarName = currentCarName ?? defaultCarName,
            currentLap = currentLap,
            currentAISpawn = currentAISpawn
        };


        foreach (var playerCar in ownedCars)
        {
            var saveCar = new CarSaveData
            {
                carName = playerCar.carName,
                currentRank = playerCar.currentRank,
                statLevels = new int[4]
            };

            for (int i = 0; i < 4; i++)
            {
                saveCar.statLevels[i] = playerCar.stats[i]?.CurrentLevel ?? 0;
            }

            wrapper.ownedCars.Add(saveCar);
        }
        //wrapper.lastShownCar = lastShownCar;
        wrapper.SFXVolume = SFXVolume;
        wrapper.musicVolume = musicVolume;
        Debug.Log($"Saving volumes: SFX={wrapper.SFXVolume}, Music={wrapper.musicVolume}");

        string json = JsonUtility.ToJson(wrapper, true);  // pretty print để dễ debug
        PlayerPrefs.SetString(PLAYER_KEY, json);
        PlayerPrefs.Save();
    }
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(PLAYER_KEY);
        ownedCars.Clear();
        SFXVolume = 1f;  // Default values
        musicVolume = 1f;
        lastShownCar = defaultCarName; //reset default
        currentTrack = baseTrack;
        currentCarName = defaultCarName;
        currentCarPrefab = LoadCarPrefabFromDatabase(defaultCarName);
        currentLap = defaultValue;
        currentAISpawn = defaultValue;
        SetSFXVolume(1f);
        SetMusicVolume(1f);

        InitDefaultData();  // Tạo lại default
        Debug.Log("Player data reset!");
    }
    private void InitDefaultData()
    {
        coin = 1000000;
        InitCarData(defaultCarName);
        currentTrack = baseTrack; //default
        DefaultSound();
        //Save 
        SavePlayerPrefs();
    }
    #endregion
    #region Car Data
    public PlayerCarData GetPlayerCarData(string carName)
    {
        return ownedCars.Find(c => c.carName == carName);
    }
    public void InitCarData(string carName)
    {
        // Tìm CarParam từ database
        CarParam originalCar = carDatabase.GetCarByName(carName);
        if (originalCar == null)
        {
            Debug.LogError($"Car '{carName}' not found in database!");
            return;
        }

        // Check đã sở hữu chưa
        if (ownedCars.Exists(c => c.carName == carName))
        {
            Debug.LogWarning($"Car '{carName}' already exists!. Skip.");
            return;
        }

        // Tạo mới PlayerCarData
        PlayerCarData playerData = new PlayerCarData
        {
            carName = carName,
            currentRank = originalCar.carBaseRank,
            stats = new UpgradeStat[4]
        };

        // Đồng bộ stats từ SO
        for (int i = 0; i < 4; i++)
        {
            CarStatType type = (CarStatType)i;
            UpgradeStat originalStat = originalCar.stats[i];  // Lấy stat sẵn từ CarParam (đã set trong Inspector)

            if (originalStat != null)
            {
                playerData.stats[i] = new UpgradeStat
                {
                    statType = type,
                    baseValue = originalStat.baseValue,  // Copy từ original
                    maxValue = originalStat.maxValue,
                    baseGold = originalStat.baseGold,
                    CurrentLevel = 0 //(mặc định)
                };

                // BỔ SUNG: Gọi OnValidate để tính và set currentValue = baseValue, goldUpgrade = next cost ngay lập tức
                playerData.stats[i].OnValidate();
            }
            else
            {
                Debug.LogWarning("Original stat null for type: " + type);
            }
        }

        ownedCars.Add(playerData);
        SavePlayerPrefs();
        GameEvent.CarPurchased(carName);
    }
    public bool UpgradeCar(string carName, CarStatType type)
    {
        PlayerCarData playerData = GetPlayerCarData(carName);
        if (playerData == null)
        {
            Debug.LogWarning($"Car '{carName}' not owned! Cannot upgrade.");
            return false;
        }

        UpgradeStat stat = playerData.stats[(int)type];
        if (stat == null || !stat.CanUpgrade())
        {
            Debug.LogWarning($"Cannot upgrade {type} for {carName}: Max level or invalid stat.");
            return false;
        }

        int cost = stat.GetNextUpgradeCost();
        if (SpendCoin(cost))
        {
            playerData.ApplyUpgrade(type);  // Tăng level + rank
            SavePlayerPrefs();
            GameEvent.CarUpgraded(carName);
            Debug.Log($"Upgraded {type} for {carName} to level {stat.CurrentLevel}. Cost: {cost}");
            return true;
        }

        return false;
    }
    #region Show Car
    public void SetLastShownCar(string carName)
    {
        if (string.IsNullOrEmpty(carName))
        {
            lastShownCar = defaultCarName;
        }
        else
        {
            lastShownCar = carName;
        }
        SavePlayerPrefs();  // Save ngay để sync prefs
    }

    public string GetLastShownCar()
    {
        return string.IsNullOrEmpty(lastShownCar) ? defaultCarName : lastShownCar;
    }
    #region Car for Race methods
    public string GetCurrentCarName()
    {
        return currentCarName;
    }

    public GameObject GetCurrentCarPrefab()
    {
        return currentCarPrefab;
    }

    public void SetCurrentCar(string carName)
    {
        // Validate tồn tại trong database (loop thủ công)
        CarParam carParam = GetCarParamByName(carName);
        if (carParam != null)
        {
            currentCarName = carName;
            currentCarPrefab = carParam.carPrefab; // Lấy prefab từ CarParam
            SavePlayerPrefs();
        }
        else
        {
            Debug.LogWarning($"Car name không tồn tại: {carName}. Giữ nguyên car cũ.");
        }
    }

    // Helper: Tìm CarParam theo name mà không dùng Linq (tương tự GetTrackById)
    private CarParam GetCarParamByName(string carName)
    {
        if (carDatabase == null || carDatabase.TotalCarCount == 0) return null;

        for (int i = 0; i < carDatabase.TotalCarCount; i++)
        {
            CarParam car = carDatabase.GetCarParam(i);
            if (car != null && car.carName == carName)
            {
                return car;
            }
        }
        return null;
    }

    // Helper: Load prefab từ database dựa trên name
    private GameObject LoadCarPrefabFromDatabase(string carName)
    {
        CarParam carParam = GetCarParamByName(carName);
        return carParam != null ? carParam.carPrefab : null;
    }
    #endregion
    #endregion
    #endregion
    #region Map data
    public string GetCurrentTrack()
    {
        return currentTrack;
    }

    public void SetCurrentTrack(string idTrack)
    {
        // Validate tồn tại trong database
        if (IsTrackExists(idTrack))
        {
            currentTrack = idTrack;
            SavePlayerPrefs();
        }
        else
        {
            Debug.LogWarning($"Track ID không tồn tại: {idTrack}. Giữ nguyên track cũ.");
        }
    }
    private bool IsTrackExists(string idTrack)
    {
        if (trackDatabase == null || trackDatabase.TrackCount == 0) return false;

        for (int i = 0; i < trackDatabase.TrackCount; i++)
        {
            if (trackDatabase.GetTrackSO(i).idTrack == idTrack)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Race Settings
    public int GetCurrentLap()
    {
        return currentLap;
    }

    public void SetCurrentLap(int lap)
    {
        if (lap > 0)
        {
            currentLap = lap;
            SavePlayerPrefs();
        }
        else
        {
            Debug.LogWarning($"Invalid lap value: {lap}. Must be > 0.");
        }
    }

    public int GetCurrentAISpawn()
    {
        return currentAISpawn;
    }

    public void SetCurrentAISpawn(int aiCount)
    {
        if (aiCount >= 0)
        {
            currentAISpawn = aiCount;
            SavePlayerPrefs();
        }
        else
        {
            Debug.LogWarning($"Invalid AI spawn value: {aiCount}. Must be >= 0.");
        }
    }
    #endregion
    #region Sound Data
    private void DefaultSound()
    {
        SetSFXVolume(1f);
        SetMusicVolume(1f);
    }
    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp(value, 0.001f, 1f);  // Min 0.001 để tránh mute hoàn toàn nếu muốn
        ApplyVolumesToMixer();
        SavePlayerPrefs();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, 0.001f, 1f);
        ApplyVolumesToMixer();
        SavePlayerPrefs();
    }

    public float GetSFXVolume() => SFXVolume;
    public float GetMusicVolume() => musicVolume;

    // Thêm PRIVATE method apply Mixer (gọi sau load/change):
    private void ApplyVolumesToMixer()
    {
        if (audioMixer == null) return;

        float sfxDB = SFXVolume > 0f ? 20f * Mathf.Log10(SFXVolume) : MIN_DB;
        float musicDB = musicVolume > 0f ? 20f * Mathf.Log10(musicVolume) : MIN_DB;

        audioMixer.SetFloat("SFX", sfxDB);
        audioMixer.SetFloat("Music", musicDB);
    }

    // Thêm music play logic (dựa trên scene):
    public void UpdateMusicForScene(string sceneName)
    {
        bool isRaceScene = sceneName.StartsWith("Track") || sceneName == "R&D";  // Adjust nếu cần
        currentClipList = isRaceScene ? raceAudioClips : menuAudioClips;

        if (musicPlayer != null && currentClipList != null && currentClipList.Length > 0)
        {
            // Set volume linear cho source (nếu không route full qua Mixer, hoặc set=1f ở Inspector)
            musicPlayer.volume = musicVolume;

            if (musicPlayer.isPlaying)
            {
                musicPlayer.Stop();  // Simple switch (có thể fade sau)
            }
            PlayNextMusicClip();
        }
    }

    private void PlayNextMusicClip()
    {
        if (currentClipList == null || currentClipList.Length == 0) return;

        // Sequential hoặc Random: Uncomment dòng Random nếu muốn random
        currentClipIndex = (currentClipIndex + 1) % currentClipList.Length;
        // currentClipIndex = Random.Range(0, currentClipList.Length);

        musicPlayer.clip = currentClipList[currentClipIndex];
        musicPlayer.Play();
    }
    //Check scene


    #endregion
    #region Coin
    public long GetCoin() => coin;

    public void ReceiveCoin(long amount)
    {
        if (amount > 0)
        {
            coin += amount;
            SavePlayerPrefs();
            GameEvent.CoinChanged(coin);
        }
    }

    public bool SpendCoin(long amount)
    {
        if (amount > 0 && coin >= amount)
        {
            coin -= amount;
            SavePlayerPrefs();
            GameEvent.CoinChanged(coin);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coin! Current: " + coin + " | Needed: " + amount);
            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowOverlap<OverlapWarningOutCoin>();
            }
            return false;
        }
    }
    #endregion

    [System.Serializable]
    private class SaveDataWrapper
    {
        public int version = 1;
        public List<CarSaveData> ownedCars = new List<CarSaveData>();
        public List<string> unlockedMaps = new List<string>();
        public float SFXVolume;
        public float musicVolume;
        public long coin;
        public string lastShownCar;
        public string currentTrack;
        public string currentCarName;
        public int currentLap;
        public int currentAISpawn;
    }
    [System.Serializable]
    private class CarSaveData
    {
        public string carName;
        public int currentRank;
        public int[] statLevels = new int[4];  // Chỉ lưu level của 4 stats (0-10)
    }
}
