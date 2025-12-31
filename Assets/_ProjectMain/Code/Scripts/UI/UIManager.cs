using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager<UIManager>
{
    [Header("EndingRaceTime")]
    public float timeIfNotFinish = 10f; //default 10s
    public bool isCountingDown; //false
    private float endCountDown;

    [Header("Scene Name")]
    public string currentSceneName;

    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        SwitchToScene("Garage");
    }
    void Update()
    {
        if (isCountingDown && endCountDown > 0)
        {
            endCountDown -= Time.deltaTime;
            if (endCountDown <= 0)
            {
                isCountingDown = false;
            }
        }
    }
    public void SetEndCountDown(float value) //reset value
    {
        if (value >= 0)
        {
            endCountDown = value;
            isCountingDown = true;
        }
    }

    public float GetEndCountDown()
    {
        return endCountDown;
    }

    public void StopCountdown()
    {
        isCountingDown = false;
    }
    public void ReloadCurrentScene()
    {
        //check currentSceneName
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            SwitchToScene(currentSceneName);
        }
    }
    public void SwitchToScene(string sceneName)
    {
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;

        // if (BackgroundMusic.HasInstance)
        // {
        //     // Cập nhật nhạc nền cho scene mới
        //     BackgroundMusic.Instance.UpdateMusicForScene(sceneName);
        // }
    }
    public void ChangeUIGameObject(GameObject currentObject = null, GameObject activeObject = null)
    {
        if (currentObject != null)
        {
            currentObject.SetActive(false);
        }
        if (activeObject != null)
        {
            activeObject.SetActive(true);
        }
    }

    #region References
    [Header("UI Container References")]

    public GameObject cScreen, cPopup, cNotify, cOverlap; // Parent GameObject container for all Screen UI elements.

    //public Camera UICamera; //The camera used for rendering UI elements.

    private Dictionary<string, BaseScreen> screens = new Dictionary<string, BaseScreen>(); //Dictionary storing all instantiated screens by their class name.
    private Dictionary<string, BasePopup> popups = new Dictionary<string, BasePopup>(); //Dictionary storing all instantiated popups by their class name.
    private Dictionary<string, BaseNotify> notifies = new Dictionary<string, BaseNotify>(); //Dictionary storing all instantiated notifies by their class name.
    private Dictionary<string, BaseOverlap> overlaps = new Dictionary<string, BaseOverlap>(); //Dictionary storing all instantiated overlaps by their class name.

    public Dictionary<string, BaseScreen> Screens => screens; //Public accessor for all registered screens.
    public Dictionary<string, BasePopup> Popups => popups; //Public accessor for all registered popups
    public Dictionary<string, BaseNotify> Notifies => notifies; //Public accessor for all registered notifies.
    public Dictionary<string, BaseOverlap> Overlaps => overlaps; //Public accessor for all registered overlaps.

    private BaseScreen curScreen; //Reference to the currently active/visible screen.
    private BasePopup curPopup; //Reference to the currently active/visible popup.
    private BaseNotify curNotify; //Reference to the currently active/visible notify.
    private BaseOverlap curOverlap; //Reference to the currently active/visible overlap.

    public BaseScreen CurScreen => curScreen; //Public accessor for the current screen.
    public BasePopup CurPopup => curPopup; //Public accessor for the current popup.
    public BaseNotify CurNotify => curNotify; //Public accessor for the current notify.
    public BaseOverlap CurOverlap => curOverlap; //Public accessor for the current overlap.

    private const string SCREEN_RESOURCES_PATH = "Prefabs/UI/Screen/"; //Resource path for loading screen prefabs from Resources folder.
    private const string POPUP_RESOURCES_PATH = "Prefabs/UI/Popup/"; //Resource path for loading popup prefabs from Resources folder.
    private const string NOTIFY_RESOURCES_PATH = "Prefabs/UI/Notify/"; //Resource path for loading notify prefabs from Resources folder.
    private const string OVERLAP_RESOURCES_PATH = "Prefabs/UI/Overlap/"; //Resource path for loading overlap prefabs from Resources folder.

    #region Screen
    private BaseScreen GetNewScreen<T>() where T : BaseScreen //Instantiates a new screen prefab from Resources and initializes it.
    {
        string nameScreen = typeof(T).Name;
        GameObject pfScreen = GetUIPrefab(UIType.Screen, nameScreen);
        // Validate that the prefab exists and has the required component
        if (pfScreen == null || !pfScreen.GetComponent<BaseScreen>())
        {
            throw new MissingReferenceException("Can not found" + nameScreen + "screen. !!!");
        }
        // Instantiate and setup the screen GameObject
        GameObject ob = Instantiate(pfScreen) as GameObject;
        ob.transform.SetParent(this.cScreen.transform);
        ob.transform.localScale = Vector3.one;
        ob.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
        ob.name = "SCREEN_" + nameScreen;
#endif
        BaseScreen screenScr = ob.GetComponent<BaseScreen>();
        screenScr.Init();
        return screenScr;
    }

    public void HideAllScreens() //// Hides all currently visible screens.
    {
        BaseScreen screenScr = null;

        foreach (KeyValuePair<string, BaseScreen> item in screens)
        {
            screenScr = item.Value;
            // Skip if screen is null or already hidden
            if (screenScr == null || screenScr.GetIsHide)
                continue;
            screenScr.Hide();

            if (screens.Count <= 0)
                break;
        }
    }

    public T GetExistScreen<T>() where T : BaseScreen //Gets an existing screen instance if it has been created before.
    {
        string screenName = typeof(T).Name;
        if (screens.ContainsKey(screenName))
        {
            return screens[screenName] as T;
        }
        return null; //The existing screen instance, or null if not found
    }

    // Shows a screen, creating it if it doesn't exist. Reuses existing instance if available.
    public void ShowScreen<T>(object data = null, bool forceShowData = false) where T : BaseScreen
    {
        string screenName = typeof(T).Name;
        BaseScreen result = null;

        if (curScreen != null)
        {
            var curName = curScreen.GetType().Name;
            if (curName.Equals(screenName))
            {
                result = curScreen;
            }
        }

        if (result == null)
        {
            if (!screens.ContainsKey(screenName))
            {
                BaseScreen screenScr = GetNewScreen<T>();
                if (screenScr != null)
                {
                    screens.Add(screenName, screenScr);
                }
            }

            if (screens.ContainsKey(screenName))
            {
                result = screens[screenName];
            }
        }

        bool isShow = false;
        if (result != null)
        {
            if (forceShowData)
            {
                isShow = true;
            }
            else
            {
                if (result.GetIsHide)
                {
                    isShow = true;
                }
            }
        }

        if (isShow)
        {
            curScreen = result;
            result.transform.SetAsLastSibling();
            result.Show(data);
        }
    }

    #endregion

    #region Popup
    private BasePopup GetNewPopup<T>() where T : BasePopup //Instantiates a new popup prefab from Resources and initializes it.
    {
        string namePopup = typeof(T).Name;
        GameObject pfPopup = GetUIPrefab(UIType.Popup, namePopup);

        // Validate that the prefab exists and has the required component
        if (pfPopup == null || !pfPopup.GetComponent<BasePopup>())
        {
            throw new MissingReferenceException("Can not found" + namePopup + "popup. !!!");
        }

        // Instantiate and setup the popup GameObject
        GameObject ob = Instantiate(pfPopup) as GameObject;
        ob.transform.SetParent(this.cPopup.transform);
        ob.transform.localScale = Vector3.one;
        ob.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
        ob.name = "POPUP_" + namePopup;
#endif
        BasePopup popupScr = ob.GetComponent<BasePopup>();
        popupScr.Init();
        return popupScr;
    }

    public void HideAllPopups() //Hides all currently visible popups.
    {
        BasePopup popupScr = null;

        foreach (KeyValuePair<string, BasePopup> item in popups)
        {
            popupScr = item.Value;
            // Skip if popup is null or already hidden
            if (popupScr == null || popupScr.GetIsHide)
                continue;
            popupScr.Hide();

            if (popups.Count <= 0)
                break;
        }
    }

    public T GetExistPopup<T>() where T : BasePopup //Gets an existing popup instance if it has been created before.
    {
        string popupName = typeof(T).Name;
        if (popups.ContainsKey(popupName))
        {
            return popups[popupName] as T;
        }
        return null;
    }

    public void ShowPopup<T>(object data = null, bool forceShowData = false) where T : BasePopup //Shows a popup, creating it if it doesn't exist. Reuses existing instance if available.
    {
        string popupName = typeof(T).Name;
        BasePopup result = null;

        if (curPopup != null)
        {
            var curName = curPopup.GetType().Name;
            if (curName.Equals(popupName))
            {
                result = curPopup;
            }
        }

        if (result == null)
        {
            if (!popups.ContainsKey(popupName))
            {
                BasePopup popupScr = GetNewPopup<T>();
                if (popupScr != null)
                {
                    popups.Add(popupName, popupScr);
                }
            }

            if (popups.ContainsKey(popupName))
            {
                result = popups[popupName];
            }
        }

        bool isShow = false;
        if (result != null)
        {
            if (forceShowData)
            {
                isShow = true;
            }
            else
            {
                if (result.GetIsHide)
                {
                    isShow = true;
                }
            }
        }

        if (isShow)
        {
            curPopup = result;
            result.transform.SetAsLastSibling();
            result.Show(data);
        }
    }

    #endregion

    #region Notify
    private BaseNotify GetNewNotify<T>() where T : BaseNotify //Instantiates a new notify prefab from Resources and initializes it.
    {
        string nameNotify = typeof(T).Name;
        GameObject pfNotify = GetUIPrefab(UIType.Notify, nameNotify);

        // Validate that the prefab exists and has the required component
        if (pfNotify == null || !pfNotify.GetComponent<BaseNotify>())
        {
            throw new MissingReferenceException("Can not found" + nameNotify + "notify. !!!");
        }

        // Instantiate and setup the notify GameObject
        GameObject ob = Instantiate(pfNotify) as GameObject;
        ob.transform.SetParent(this.cNotify.transform);
        ob.transform.localScale = Vector3.one;
        ob.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
        ob.name = "NOTIFY_" + nameNotify;
#endif
        BaseNotify notifyScr = ob.GetComponent<BaseNotify>();
        notifyScr.Init();
        return notifyScr;
    }

    public void HideAllNotifies() //Hides all currently visible notifies.
    {
        BaseNotify notifyScr = null;

        foreach (KeyValuePair<string, BaseNotify> item in notifies)
        {
            notifyScr = item.Value;
            // Skip if notify is null or already hidden
            if (notifyScr == null || notifyScr.GetIsHide)
                continue;
            notifyScr.Hide();

            if (notifies.Count <= 0)
                break;
        }
    }

    public T GetExistNotify<T>() where T : BaseNotify //Gets an existing notify instance if it has been created before.
    {
        string notifyName = typeof(T).Name;
        if (notifies.ContainsKey(notifyName))
        {
            return notifies[notifyName] as T;
        }
        return null;
    }

    public void ShowNotify<T>(object data = null, bool forceShowData = false) where T : BaseNotify //Shows a notify, creating it if it doesn't exist. Reuses existing instance if available.
    {
        string notifyName = typeof(T).Name;
        BaseNotify result = null;

        if (curNotify != null)
        {
            var curName = curPopup.GetType().Name;
            if (curName.Equals(notifyName))
            {
                result = curNotify;
            }
        }

        if (result == null)
        {
            if (!notifies.ContainsKey(notifyName))
            {
                BaseNotify notifyScr = GetNewNotify<T>();
                if (notifyScr != null)
                {
                    notifies.Add(notifyName, notifyScr);
                }
            }

            if (notifies.ContainsKey(notifyName))
            {
                result = notifies[notifyName];
            }
        }

        bool isShow = false;
        if (result != null)
        {
            if (forceShowData)
            {
                isShow = true;
            }
            else
            {
                if (result.GetIsHide)
                {
                    isShow = true;
                }
            }
        }

        if (isShow)
        {
            curNotify = result;
            result.transform.SetAsLastSibling();
            result.Show(data);
        }
    }

    #endregion

    #region Overlap
    private BaseOverlap GetNewOverLap<T>() where T : BaseOverlap //Instantiates a new overlap prefab from Resources and initializes it.
    {
        string nameOverlap = typeof(T).Name;
        GameObject pfOverlap = GetUIPrefab(UIType.Overlap, nameOverlap);

        // Validate that the prefab exists and has the required component
        if (pfOverlap == null || !pfOverlap.GetComponent<BaseOverlap>())
        {
            throw new MissingReferenceException("Can not found" + nameOverlap + "overlap. !!!");
        }

        // Instantiate and setup the overlap GameObject
        GameObject ob = Instantiate(pfOverlap) as GameObject;
        ob.transform.SetParent(this.cOverlap.transform);
        ob.transform.localScale = Vector3.one;
        ob.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
        ob.name = "OVERLAP_" + nameOverlap;
#endif
        BaseOverlap overlapScr = ob.GetComponent<BaseOverlap>();
        overlapScr.Init();
        return overlapScr;
    }

    public void HideAllOverlaps() //Hides all currently visible overlaps.
    {
        BaseOverlap overlapScr = null;

        foreach (KeyValuePair<string, BaseOverlap> item in overlaps)
        {
            overlapScr = item.Value;
            // Skip if overlap is null or already hidden
            if (overlapScr == null || overlapScr.GetIsHide)
                continue;
            overlapScr.Hide();

            if (overlaps.Count <= 0)
                break;
        }
    }

    public T GetExistOverlap<T>() where T : BaseOverlap //Gets an existing overlap instance if it has been created before.
    {
        string overlapName = typeof(T).Name;
        if (overlaps.ContainsKey(overlapName))
        {
            return overlaps[overlapName] as T;
        }
        return null;
    }

    public void ShowOverlap<T>(object data = null, bool forceShowData = false) where T : BaseOverlap //Shows an overlap, creating it if it doesn't exist. Reuses existing instance if available.
    {
        string overlapName = typeof(T).Name;
        BaseOverlap result = null;

        if (curOverlap != null)
        {
            var curName = curOverlap.GetType().Name;
            if (curName.Equals(overlapName))
            {
                result = curOverlap;
            }
        }

        if (result == null)
        {
            if (!overlaps.ContainsKey(overlapName))
            {
                BaseOverlap overlapScr = GetNewOverLap<T>();
                if (overlapScr != null)
                {
                    overlaps.Add(overlapName, overlapScr);
                }
            }

            if (overlaps.ContainsKey(overlapName))
            {
                result = overlaps[overlapName];
            }
        }

        bool isShow = false;
        if (result != null)
        {
            if (forceShowData)
            {
                isShow = true;
            }
            else
            {
                if (result.GetIsHide)
                {
                    isShow = true;
                }
            }
        }

        if (isShow)
        {
            curOverlap = result;
            result.transform.SetAsLastSibling();
            result.Show(data);
        }
    }

    #endregion

    private GameObject GetUIPrefab(UIType t, string uiName) //Loads a UI prefab from the Resources folder based on UI type and name.
    {
        GameObject result = null;
        var defaultPath = "";
        if (result == null)
        {
            // Determine the resource path based on UI type
            switch (t)
            {
                case UIType.Screen:
                    {
                        defaultPath = SCREEN_RESOURCES_PATH + uiName;
                    }
                    break;
                case UIType.Popup:
                    {
                        defaultPath = POPUP_RESOURCES_PATH + uiName;
                    }
                    break;
                case UIType.Notify:
                    {
                        defaultPath = NOTIFY_RESOURCES_PATH + uiName;
                    }
                    break;
                case UIType.Overlap:
                    {
                        defaultPath = OVERLAP_RESOURCES_PATH + uiName;
                    }
                    break;
            }

            // Load the prefab from Resources folder
            result = Resources.Load(defaultPath) as GameObject;
        }
        return result;
    }

    #endregion
    #region Other
    public void LoadSceneWithLoading(string sceneName)
    {
        // Show notify loading với data là sceneName
        ShowNotify<NotifyLoadingGame>(data: sceneName);
    }
    
    #endregion
}

