using UnityEngine;
using UnityEngine.UI;

public class ScreenSetting : BaseScreen
{
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    public override void Init()
    {
        base.Init();

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();  // Tránh duplicate
            sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        }
    }
    public override void Show(object data)
    {
        base.Show(data);
        if (PlayerManager.HasInstance)
        {
            float sfxVol = PlayerManager.Instance.GetSFXVolume();
            float musicVol = PlayerManager.Instance.GetMusicVolume();
            
            if (sfxSlider != null) sfxSlider.value = sfxVol;
            if (musicSlider != null) musicSlider.value = musicVol;
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
    public void OnClickClearData()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowOverlap<OverlapRestartGame>();
        }
    }
    public void OnClickGoBack()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<ScreenHome>();
        }
        this.Hide();
    }
    #region Slider
    public void OnSFXSliderValueChanged(float value)
    {
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.SetSFXVolume(value);
            Debug.Log($"SFX changed to {value}");
        }
    }
    public void OnMusicSliderValueChanged(float value)
    {
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.SetMusicVolume(value);
            Debug.Log($"Music changed to {value}");
        }
    }
    #endregion
}
