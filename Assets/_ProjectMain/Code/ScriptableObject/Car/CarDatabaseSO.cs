using UnityEngine;

[CreateAssetMenu]
public class CarDatabaseSO : ScriptableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CarSO[] carsSO;

    public int CarCount
    {
        get
        {
            return carsSO.Length;
        }
    }

    public CarSO GetCarSO(int index)
    {
        return carsSO[index];
    }
}
