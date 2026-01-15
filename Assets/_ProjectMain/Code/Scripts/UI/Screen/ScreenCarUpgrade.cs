using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCarUpgrade : BaseCarInfoDisplayScreen
{
    [Header("CanvaGroupTab")]
    [SerializeField] private CanvasGroup topSpeedTab;
    [SerializeField] private CanvasGroup accelerationTab;
    [SerializeField] private CanvasGroup handlingTab;
    [SerializeField] private CanvasGroup nitroTab;

    [Header("Tab Items")]
    [SerializeField] private TabItem topSpeedItem;  // Drag TabItem từ topSpeedTab
    [SerializeField] private TabItem accelerationItem;
    [SerializeField] private TabItem handlingItem;
    [SerializeField] private TabItem nitroItem;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button topSpeedBtn;
    [SerializeField] private Button accelerationBtn;
    [SerializeField] private Button handlingBtn;
    [SerializeField] private Button nitroBtn;
    private UpgradePayload upgradePayload;  // Data nhận từ ScreenCarInfo
    private CarInfoData infoData;
    private PlayerCarData playerData;
    private CanvasGroup[] tabs;
    private TabItem[] tabItems;
    public override void Init()
    {
        base.Init();
        tabs = new CanvasGroup[4] { topSpeedTab, accelerationTab, handlingTab, nitroTab };
        tabItems = new TabItem[4] { topSpeedItem, accelerationItem, handlingItem, nitroItem };
    }

    public override void Show(object data)
    {
        base.Show(data);

        upgradePayload = data as UpgradePayload;

        if (upgradePayload != null && upgradePayload.InfoData?.Car != null)
        {
            infoData = upgradePayload.InfoData;

            // Lấy PlayerCarData lần đầu
            playerData = PlayerManager.Instance.GetPlayerCarData(infoData.Car.carName);
            if (playerData == null)
            {
                Debug.LogError($"Xe {infoData.Car.carName} chưa sở hữu, không thể upgrade!");
                return;
            }

            LoadCarData(infoData.Car, playerData);  // Load sliders chung

            AddListenerTabButtons();
            OpenTab(upgradePayload.InitialStatType);
        }
        else
        {
            Debug.LogWarning("No valid UpgradePayload! Opening default tab (TopSpeed)");
            OpenTab(CarStatType.TopSpeed);
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
    private void OpenTab(CarStatType type)
    {
        int index = (int)type;  // Chuyển enum → index (0-3)

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null)
            {
                bool isActive = (i == index);
                tabs[i].alpha = isActive ? 1f : 0f;
                tabs[i].blocksRaycasts = isActive;
                tabs[i].interactable = isActive;  // Thêm nếu cần tương tác con trong panel
            }
        }

        // Cập nhật UI tab (nếu cần, ví dụ text/slider dựa upgradeData.Car.stats[index])
        UpdateTabUI(type);
    }
    private void RefreshPlayerData()
    {
        if (infoData?.Car != null)
        {
            playerData = PlayerManager.Instance.GetPlayerCarData(infoData.Car.carName);
            if (playerData == null)
            {
                Debug.LogError("PlayerData lost after upgrade!");
            }
        }
    }
    private void UpdateTabUI(CarStatType type)
    {
        int index = (int)type;
        TabItem item = tabItems[index];
        if (item == null) return;

        PlayerCarData playerData = PlayerManager.Instance.GetPlayerCarData(infoData.Car.carName);
        UpgradeStat stat = playerData.stats[index];

        // Level
        item.currentLevel.text = stat.CurrentLevel.ToString();
        item.nextLevel.text = (stat.CurrentLevel + 1).ToString();

        // Value
        item.currentValue.text = stat.CurrentValue.ToString("F2");
        item.nextValue.text = stat.GetPreviewNextValue().ToString("F2");

        // Cost
        int cost = stat.GetNextUpgradeCost();
        item.costCoin.text = cost.ToString("N0");

        bool isMax = !stat.CanUpgrade();  // Level == 10
        item.CurrentLevelGroup.SetActive(!isMax);
        item.CurrentValueGroup.SetActive(!isMax);
        item.upgradeGroup.SetActive(!isMax);
        item.maxLevel.gameObject.SetActive(isMax);  // Show maxLevel text
        item.maxValue.gameObject.SetActive(isMax);  // Show maxValue text

        if (isMax)
        {
            item.maxLevel.text = "Max";
            item.maxValue.text = stat.maxValue.ToString("F2");
        }

        // Listener cho upgradeButton (remove cũ trước)
        item.upgradeButton.onClick.RemoveAllListeners();
        if (!isMax)
        {
            item.upgradeButton.onClick.AddListener(() =>
            {
                if (PlayerManager.Instance.UpgradeCar(infoData.Car.carName, type))
                {
                    // Success → Update UI
                    RefreshPlayerData();
                    LoadCarData(infoData.Car, playerData);  // Update sliders chung
                    UpdateTabUI(type);  // Update tab này
                }
            });
        }
    }
    #region Button
    private void AddListenerTabButtons()
    {
        if (topSpeedBtn != null) topSpeedBtn.onClick.AddListener(() => OpenTab(CarStatType.TopSpeed));
        if (accelerationBtn != null) accelerationBtn.onClick.AddListener(() => OpenTab(CarStatType.Acceleration));
        if (handlingBtn != null) handlingBtn.onClick.AddListener(() => OpenTab(CarStatType.Handling));
        if (nitroBtn != null) nitroBtn.onClick.AddListener(() => OpenTab(CarStatType.Nitro));
    }
    private void RemoveListenerTabButtons()
    {
        if (topSpeedBtn != null) topSpeedBtn.onClick.RemoveAllListeners();
        if (accelerationBtn != null) accelerationBtn.onClick.RemoveAllListeners();
        if (handlingBtn != null) handlingBtn.onClick.RemoveAllListeners();
        if (nitroBtn != null) nitroBtn.onClick.RemoveAllListeners();
    }
    public void OnClickGoBack()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.GoBack();
        }
        this.Hide();
    }
    #endregion
}
