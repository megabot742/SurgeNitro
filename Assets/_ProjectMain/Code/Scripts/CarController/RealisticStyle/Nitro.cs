using UnityEngine;
[System.Serializable]
public class Nitro
{
    [SerializeField] private bool install = false;

    [SerializeField, Min(1f)] private float _engineTorqueCoef = 1.5f;

    [SerializeField, Min(0f)] private float _maxTankCapacity = 30f;

    private float remainTankCapacity;
    private bool injection;

    public bool Install
    {
        get => install;
        set => install = value;
    }

    public bool Injection
    {
        get => injection;
        set => injection = value;
    }

    public float EngineTorqueCoef => injection ? _engineTorqueCoef : 1f;

    public float RemainTankCapacity
    {
        get => remainTankCapacity;
        set => remainTankCapacity = value;
    }

    public void Init()
    {
        if (!install)
        {
            return;
        }

        remainTankCapacity = _maxTankCapacity;
    }

    public void Update(bool nosInput)
    {   
;        if (!install)
        {
            injection = false;
            return;
        }

        if (nosInput)
        {
            remainTankCapacity = Mathf.Max(remainTankCapacity - Time.deltaTime, 0f);
            injection = remainTankCapacity > 0f;
            
        }
        else
        {
            injection = false;
        }
        DisplayNitro();
    }
    private void DisplayNitro()
    {
        if(UIManager.HasInstance)
        {
            // UIManager.Instance.hUDPanel.nitroSlider.maxValue = _maxTankCapacity;
            // UIManager.Instance.hUDPanel.nitroSlider.value = remainTankCapacity;

        }
    }
}
