using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSelectTrack : BaseScreen
{
    [Header("References")]
    [SerializeField] TMP_Text trackName;
    [SerializeField] Image trackImg;
    [SerializeField] Button LeftBtn;
    [SerializeField] Button RightBtn;
    [SerializeField] Button SelectBtn;
    [SerializeField] TrackDatabaseSO trackDatabase;
    private int currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init()
    {
        base.Init();
        LeftBtn.onClick.AddListener(OnLeftButton);
        RightBtn.onClick.AddListener(OnRightButton);
        SelectBtn.onClick.AddListener(OnSelectButton);
    }

    public override void Show(object data)
    {
        base.Show(data);
        string currentTrackId = PlayerManager.Instance.GetCurrentTrack();
        currentIndex = 0; // Default
        for (int i = 0; i < trackDatabase.TrackCount; i++)
        {
            if (trackDatabase.GetTrackSO(i).idTrack == currentTrackId)
            {
                currentIndex = i;
                break;
            }
        }
        UpdateUI();
    }

    public override void Hide()
    {
        base.Hide();
    }
    private void UpdateUI()
    {
        if (trackDatabase.TrackCount == 0) return;

        TrackSO currentTrackSO = trackDatabase.GetTrackSO(currentIndex);
        trackName.text = currentTrackSO.trackName;
        trackImg.sprite = currentTrackSO.trackImg;
    }
    private void OnLeftButton()
    {
        currentIndex = (currentIndex - 1 + trackDatabase.TrackCount) % trackDatabase.TrackCount;
        UpdateUI();
    }
    private void OnRightButton()
    {
        currentIndex = (currentIndex + 1) % trackDatabase.TrackCount;
        UpdateUI();
    }
    private void OnSelectButton()
    {
        TrackSO selectedTrack = trackDatabase.GetTrackSO(currentIndex);
        PlayerManager.Instance.SetCurrentTrack(selectedTrack.idTrack);

        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
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
}
