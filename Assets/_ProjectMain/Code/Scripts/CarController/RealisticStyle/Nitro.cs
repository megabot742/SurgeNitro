using UnityEngine;
[System.Serializable]
public class Nitro
{
    [SerializeField] private bool install = false;

    [SerializeField, Min(1f)] private float engineTorqueCoef = 1.5f;

    [SerializeField, Min(0f)] private float maxTankCapacity = 30f;

    [SerializeField] private float remainTankCapacity;
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

    public float EngineTorqueCoef => injection ? engineTorqueCoef : 1f;

    public float RemainTankCapacity
    {
        get => remainTankCapacity;
        set => remainTankCapacity = value;
    }
    public float MaxTankCapacity
    {
        get => maxTankCapacity;
    }

    public void Init()
    {
        if (!install)
        {
            return;
        }

        remainTankCapacity = maxTankCapacity;
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
    }
}
