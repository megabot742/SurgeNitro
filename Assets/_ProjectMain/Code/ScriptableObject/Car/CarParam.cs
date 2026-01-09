using UnityEngine;

[System.Serializable]
public class CarParam
{
    [Header("Car Setting")]
    public CarClass carClass;
    public int carCurrentRank;
    public int carMaxRank;
    public string carName;
    public GameObject carPrefab;
    public GameObject carShowModel;
    public Sprite carSprite;

    [Header("Car Parameter")]
    public float topSpeed; //KPH, default 100
    public float acceleration; //Second, default 5
    public float handling; //Power, default 0.2
    public float nitro; //Second, default 2

}
