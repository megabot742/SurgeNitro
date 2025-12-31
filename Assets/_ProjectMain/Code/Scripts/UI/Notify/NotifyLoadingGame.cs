using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotifyLoadingGame : BaseNotify
{
    [Header("UI References")]
    [SerializeField] private TMP_Text txtLoading;
    [SerializeField] private Slider sldLoading; //value range 0 - 1 mean 0% -> 100%
    [SerializeField] string sceneName;

    public override void Init() //Initializes the loading notify.
    {
        base.Init();
    }

    public override void Show(object data) //Shows the loading notify and starts the scene loading coroutine.
    {
        base.Show(data);

        string targetScene = data as string;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("NotifyLoadingGame: No scene name provided in data!");
            this.Hide();
            return;
        }

        // Cập nhật currentSceneName trong UIManager để các chỗ khác dùng (nếu cần)
        if (UIManager.HasInstance)
        {
            UIManager.Instance.currentSceneName = targetScene;
        }

        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Bắt đầu load nhưng KHÔNG cho activate ngay
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false; // Quan trọng: chặn activation

        // Phase 1: Load đến 90% (Unity progress max 0.9 khi chưa activate)
        while (asyncOperation.progress < 0.9f)
        {
            float progress = asyncOperation.progress / 0.9f; // Scale 0 -> 1
            sldLoading.value = progress;
            txtLoading.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            yield return null;
        }

        // Đã load xong tài nguyên (progress = 0.9), giờ giả vờ "đang xử lý cuối cùng"
        sldLoading.value = 0.9f; // Giữ ở 90%
        txtLoading.text = "Loading... 90%";

        // Delay random 1 - 1.5s để người chơi cảm nhận loading
        float fakeDelay = Random.Range(1.0f, 1.5f);
        float timer = 0f;
        while (timer < fakeDelay)
        {
            timer += Time.unscaledDeltaTime; // Dùng unscaled để delay chính xác dù Time.timeScale = 0

            // Optional: Animate nhẹ ở 90-99% để sống động hơn
            float fakeProgress = 0.9f + (timer / fakeDelay) * 0.09f; // Từ 90% → 99%
            sldLoading.value = fakeProgress;
            txtLoading.text = $"Loading... {Mathf.RoundToInt(fakeProgress * 100)}%";

            yield return null;
        }

        // Delay xong → cho phép activate scene
        sldLoading.value = 1f;
        txtLoading.text = "Loading... 100%";

        asyncOperation.allowSceneActivation = true; // Bây giờ scene mới thực sự chạy

        // Chờ scene activate hoàn tất (thường rất nhanh)
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // Hide loading screen
        this.Hide();
    }

    public override void Hide()
    {
        base.Hide();
        // Optional: Stop coroutine nếu đang chạy
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
