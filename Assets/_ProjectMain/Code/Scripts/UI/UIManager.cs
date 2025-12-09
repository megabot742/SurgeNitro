using UnityEditor;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{
    [Header("InMenu")]
    public HomeMenuPanel homeMenuPanel;

    [Header("InRace")]
    public HUDPanel hUDPanel;
    public PausePanel pausePanel;
    public ResultPanel resultPanel;

    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void ChangeUIGameObject(GameObject currentObject = null, GameObject activeObject = null)
    {
        if(currentObject != null)
        {
            currentObject.SetActive(false);
        }
        if(activeObject != null)
        {
            activeObject.SetActive(false);
        }
    }
    private void UpdateUIForScene(string sceneName)
    {
        //Disable all panels first
        ChangeUIGameObject(homeMenuPanel.gameObject);

        ChangeUIGameObject(hUDPanel.gameObject);
        ChangeUIGameObject(pausePanel.gameObject);
        ChangeUIGameObject(resultPanel.gameObject);

        switch (sceneName)
        {
            case "Garage":
                ChangeUIGameObject(null, homeMenuPanel.gameObject);
                break;
            case "Track1":
            case "Track2":
            case "Track3":
                ChangeUIGameObject(null, hUDPanel.gameObject);
                break;
        }
    }
}
