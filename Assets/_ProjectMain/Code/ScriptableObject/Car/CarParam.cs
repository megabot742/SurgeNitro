using UnityEngine;

[System.Serializable]
public class CarParam
{
    [Header("Car Setting")]
    public CarClass carClass;
    public int carRank;
    public string carName;
    public GameObject carPrefab;

    [Header("Car Parameter")]
    public float topSpeed; //KPH
    public float acceleration; //Second
    public float handling; //Power
    public float nitro; //Second

}
