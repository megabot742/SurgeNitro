using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class AfterFireController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _minDuration = 0.05f;
    [SerializeField, Min(0f)] private float _maxDuration = 0.1f;
    [SerializeField, ReadOnly] List<AfterFireEffect> effects = new List<AfterFireEffect>(); // Cache các con
    private CarControllerBase carController;
    private float endTime;
    private bool isNitroActiveCached; // Cache để tránh check nitro mỗi frame

    private void Awake()
    {
        // Tìm CarController và đăng ký listener (chỉ một lần)
        carController = GetComponentInParent<CarControllerBase>();
        if (carController != null)
        {
            if (carController is RealisticCarController r)
                r.Engine.OnAfterFire.AddListener(OnAfterFire);
            else if (carController is ArcadeCarController a)
                a.OnAfterFire.AddListener(OnAfterFire);
        }

        // Tự động tìm tất cả con (tối ưu: chỉ Awake một lần)
        effects.AddRange(GetComponentsInChildren<AfterFireEffect>());
        foreach (var effect in effects)
        {
            effect.Initialize(); // Gọi init cho con nếu cần
            effect.StopEffect(); // Đảm bảo stop ban đầu
        }
    }

    private void Update()
    {
        if (carController == null || effects.Count == 0) return;

        bool isNitroActive = IsNitroActive();
        if (isNitroActive != isNitroActiveCached)
        {
            isNitroActiveCached = isNitroActive; // Cache để giảm checks
            if (isNitroActive)
            {
                TriggerAllEffects(true); // Play loop cho nitro
            }
            else
            {
                // Chỉ stop nếu hết time (như hiện tại)
                if (Time.time >= endTime)
                {
                    TriggerAllEffects(false);
                }
            }
        }
        else if (!isNitroActive && Time.time >= endTime)
        {
            TriggerAllEffects(false); // Stop nếu hết time
        }
    }

    private void OnAfterFire()
    {
        if (IsNitroActive()) return; // Không trigger ngắn nếu nitro on

        TriggerAllEffects(true);
        endTime = Time.time + Random.Range(_minDuration, _maxDuration);
    }

    private bool IsNitroActive()
    {
        if (carController is RealisticCarController r) return r.Engine.NOS.Injection;
        if (carController is ArcadeCarController a) return a.NOS.Injection;
        return false;
    }

    private void TriggerAllEffects(bool play)
    {
        foreach (var effect in effects)
        {
            if (play) effect.PlayEffect();
            else effect.StopEffect();
        }
    }

    private void OnDestroy()
    {
        // Hủy listener
        if (carController != null)
        {
            if (carController is RealisticCarController r)
                r.Engine.OnAfterFire.RemoveListener(OnAfterFire);
            else if (carController is ArcadeCarController a)
                a.OnAfterFire.RemoveListener(OnAfterFire);
        }
    }
}