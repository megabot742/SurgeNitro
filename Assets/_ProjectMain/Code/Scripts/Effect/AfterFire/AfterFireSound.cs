using UnityEngine;

[DisallowMultipleComponent]
public class AfterFireSound : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _minDuration = 0.05f;
    [SerializeField, Min(0f)] private float _maxDuration = 0.1f;

    private AudioSource audioSource;
    private CarControllerBase carController;

    private float endTime;

    private void Awake()
    {
        // Tự động lấy AudioSource – nếu không có thì không làm gì (không lỗi)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            enabled = false;
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;        // Quan trọng: loop khi Nitro
        audioSource.Stop();

        // Tìm CarController và đăng ký sự kiện
        carController = GetComponentInParent<CarControllerBase>();
        if (carController != null)
        {
            if (carController is RealisticCarController r)
                r.Engine.OnAfterFire.AddListener(OnAfterFire);
            else if (carController is ArcadeCarController a)
                a.OnAfterFire.AddListener(OnAfterFire);
        }
    }

    private void Update()
    {
        if (carController == null || audioSource == null) return;

        bool isNitroActive = IsNitroActive();

        if (isNitroActive)
        {
            // Nitro bật → bật âm thanh liên tục (loop)
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            // Nitro tắt → chỉ chạy khi có AfterFire, hết thời gian thì tắt
            if (audioSource.isPlaying && Time.time >= endTime)
            {
                audioSource.Stop();
            }
        }
    }

    private void OnAfterFire()
    {
        if (IsNitroActive()) return; // Đang Nitro thì không cần bật ngắn

        if (!audioSource.isPlaying)
            audioSource.Play();

        endTime = Time.time + Random.Range(_minDuration, _maxDuration);
    }

    private bool IsNitroActive()
    {
        if (carController is RealisticCarController r) return r.Engine.NOS.Injection;
        if (carController is ArcadeCarController a) return a.NOS.Injection;
        return false;
    }

    private void OnDestroy()
    {
        if (carController == null) return;

        if (carController is RealisticCarController r)
            r.Engine.OnAfterFire.RemoveListener(OnAfterFire);
        else if (carController is ArcadeCarController a)
            a.OnAfterFire.RemoveListener(OnAfterFire);
    }
}
