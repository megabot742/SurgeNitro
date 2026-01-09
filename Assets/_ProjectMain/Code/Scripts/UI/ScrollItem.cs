using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text carNameTxt;
    [SerializeField] private Image classColorImg;
    [SerializeField] private TMP_Text classTxt;
    [SerializeField] private TMP_Text rankTxt;
    [SerializeField] private Image carImg;
    
    private CarParam carData; //Data car
    private CarDatabaseSO database; //Data get class color

    public void SetData(CarParam car, CarDatabaseSO db)
    {
        if (car == null) return;

        // Step 1: Save data.
        carData = car;
        database = db;
        // Step 2: Update UI
        if (carNameTxt != null) carNameTxt.text = car.carName; //Name car
        if (classColorImg != null)  //Color Class
        {
            Color classColor = GetClassColor(car.carClass);
            classColorImg.color = classColor;
        }
        if (classTxt != null) //Class car
        {
            string displayClass = car.carClass.ToString().Replace("class", "").ToUpper();
            classTxt.text = displayClass;
        }
        if (rankTxt != null) rankTxt.text = $"Rank: {car.carCurrentRank}/{car.carMaxRank}"; //Rank car
        if (carImg != null) carImg.sprite = car.carSprite; //Img car
    }
    //------Get color from Car class in database-----
    private Color GetClassColor(CarClass carClass)
    {
        if (database == null) return Color.white;
        return database.GetClassColor(carClass);
    }

    //-----Reset when reuse form pool-----
    public void ResetItem()
    {
        carData = null;
        if (carNameTxt != null) carNameTxt.text = "";
        if (classTxt != null) classTxt.text = "";
        if (rankTxt != null) rankTxt.text = "";
        if (carImg != null) carImg.sprite = null;
        if (classColorImg != null) classColorImg.color = Color.white;
    }
}
