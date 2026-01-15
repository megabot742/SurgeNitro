using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenRaceSetup : BaseScreen
{
    [Header("Car Select")]
    [SerializeField] CarDatabaseSO carDatabase;
    [SerializeField] TMP_Text carNameTxt;
    [SerializeField] Image classColorImg;
    [SerializeField] TMP_Text classTxt;
    [SerializeField] Image carImg;
    [SerializeField] TMP_Text carRankTxt;

    [Header("Track Select")]
    [SerializeField] TrackDatabaseSO trackDatabase;
    [SerializeField] Image trackImg;
    [SerializeField] TMP_Text trackNameTxt;

    [Header("Setup AI and Lap")]
    [SerializeField] Slider aiSlider;
    [SerializeField] TMP_Text aiText;
    [SerializeField] Slider lapSlider;
    [SerializeField] TMP_Text lapTxt;
    public override void Init()
    {
        base.Init();
        //Slider listerners (Not change)
        if (aiSlider != null)
        {
            aiSlider.onValueChanged.AddListener(OnAISliderChanged);
        }
        if (lapSlider != null)
        {
            lapSlider.onValueChanged.AddListener(OnLapSliderChanged);
        }
    }

    public override void Show(object data)
    {
        base.Show(data);
        // Cập nhật car UI từ currentCarName
        string currentCarName = PlayerManager.Instance.GetCurrentCarName();
        
        CarParam carParam = GetCarParamByName(currentCarName);
        if (carParam != null)
        {
            PlayerCarData playerData = PlayerManager.Instance.GetPlayerCarData(carParam.carName);
            if (playerData != null)
            {
                carParam.carCurrentRank = playerData.currentRank;  //override curent rank after upgrade
            }
            carNameTxt.text = carParam.carName;
            carImg.sprite = carParam.carSprite;
            carRankTxt.text = $"Rank: {carParam.carCurrentRank}/{carParam.carMaxRank}";

            string displayClass = carParam.carClass.ToString().Replace("class", "").ToUpper();
            classTxt.text = displayClass;

            Color classColor = GetClassColor(carParam.carClass);
            classColorImg.color = classColor;
        }
        else
        {
            Debug.LogWarning("Current car not found in database!");
        }
        // Cập nhật track UI từ currentTrack
        string currentTrackId = PlayerManager.Instance.GetCurrentTrack();
        TrackSO track = GetTrackById(currentTrackId);
        if (track != null)
        {
            trackNameTxt.text = track.trackName;
            trackImg.sprite = track.trackImg;
        }
        else
        {
            Debug.LogWarning("Current track not found in database!");
        }

        // Cập nhật sliders và text từ PlayerManager
        if (aiSlider != null && aiText != null)
        {
            int currentAI = PlayerManager.Instance.GetCurrentAISpawn();
            aiSlider.value = currentAI;
            aiText.text = currentAI.ToString();
        }

        if (lapSlider != null && lapTxt != null)
        {
            int currentLap = PlayerManager.Instance.GetCurrentLap();
            lapSlider.value = currentLap;
            lapTxt.text = currentLap.ToString();
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
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

    // Helper: Lấy class color từ database (dùng method sẵn có)
    private Color GetClassColor(CarClass carClass)
    {
        return carDatabase.GetClassColor(carClass);
    }
    private TrackSO GetTrackById(string idTrack)
    {
        if (trackDatabase == null || trackDatabase.TrackCount == 0) return null;

        for (int i = 0; i < trackDatabase.TrackCount; i++)
        {
            TrackSO track = trackDatabase.GetTrackSO(i);
            if (track.idTrack == idTrack)
            {
                return track;
            }
        }
        return null;
    }
    #region Slider Handlers
    private void OnAISliderChanged(float value)
    {
        int aiCount = Mathf.RoundToInt(value);
        if (aiText != null)
        {
            aiText.text = aiCount.ToString();
        }
        PlayerManager.Instance.SetCurrentAISpawn(aiCount);
    }

    private void OnLapSliderChanged(float value)
    {
        int lap = Mathf.RoundToInt(value);
        if (lapTxt != null)
        {
            lapTxt.text = lap.ToString();
        }
        PlayerManager.Instance.SetCurrentLap(lap);
    }
    #endregion
    #region Button Handlers
    public void OnClickGoGarage()
    {
        if (UIEventManager.HasInstance)
        {
            var data = new CarInfoData { Mode = CarInfoMode.SelectForRace };
            UIEventManager.Instance.GarageBtn(data);
        }
        this.Hide();
    }
    public void OnClickSelectTrack()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.ShowScreenWithHistory<ScreenSelectTrack>();
        }
        this.Hide();
    }
    public void OnClickStartGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.RaceBtn();
        }
        this.Hide();
    }
    public void OnClickGoBack()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
    #endregion
}
