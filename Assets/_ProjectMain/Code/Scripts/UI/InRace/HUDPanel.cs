using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{   
    [Header("Lap&Pos")]
    public TMP_Text lapTxt;
    public TMP_Text posTxt;

    [Header("Time")]
    public TMP_Text lapTimeTxt;
    public TMP_Text bestLapTimeTxt;

    [Header("Speed&Nitro")]
    public TMP_Text speedDometerText;
    public Slider nitroSlider;

    [Header("Timer")]
    public TMP_Text countDownNum;
    public TMP_Text countDownTxt;
    public TMP_Text countDownTimeLeft;


    void Start()
    {
        
    }
    void Update()
    {
        DisplayCountDown();
    }
    public void DisplayCountDown()
    {
        if (UIManager.HasInstance && UIManager.Instance.isCountingDown) // True
        {
            int timeLeft = Mathf.RoundToInt(UIManager.Instance.GetEndCountDown());
            UIManager.Instance.hUDPanel.countDownTimeLeft.gameObject.SetActive(true);
            UIManager.Instance.hUDPanel.countDownTimeLeft.text = "Timeleft: " + timeLeft.ToString();
        }
        else
        {
            UIManager.Instance.hUDPanel.countDownTimeLeft.gameObject.SetActive(false);
        }
    }
    void OnDisable()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.hUDPanel.countDownNum.gameObject.SetActive(true); //Enable count down for new race
            UIManager.Instance.hUDPanel.countDownTimeLeft.gameObject.SetActive(false); //Disable time left counter
            UIManager.Instance.resultPanel.unclockTrackTxt.gameObject.SetActive(false); //Make sure text unclock track in result disable for new race
        }
    }
    //PausePanel
    public void OnClickPause()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PauseGame();
        }
    }
}
