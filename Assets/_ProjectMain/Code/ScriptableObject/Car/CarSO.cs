using UnityEngine;

[System.Serializable]
public class CarSO
{
    [Header("Car Setting")]
    public string carName;
    public GameObject carPrefab;
    public int carRank;

    [Header("Car Parameter")]
    public float topSpeed; //KPH
    public float acceleration; //Second
    public float handling; //Power
    public float nitro; //Second

}
