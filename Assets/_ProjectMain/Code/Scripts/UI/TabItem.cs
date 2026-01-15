using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public TMP_Text currentLevel;
    [SerializeField] public TMP_Text nextLevel;
    [SerializeField] public GameObject CurrentLevelGroup; //disable when level max
    [SerializeField] public TMP_Text maxLevel; //default disable

    [Header("Car privew stat")]
    [SerializeField] public TMP_Text currentValue;
    [SerializeField] public TMP_Text nextValue;
    [SerializeField] public GameObject CurrentValueGroup; //disable when value max
    [SerializeField] public TMP_Text maxValue; //default disable

    [Header("Buy group")]
    [SerializeField] public GameObject upgradeGroup; 
    [SerializeField] public TMP_Text costCoin;
    [SerializeField] public Button upgradeButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
