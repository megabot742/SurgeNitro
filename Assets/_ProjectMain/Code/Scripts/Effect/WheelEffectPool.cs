using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
[RequireComponent(typeof(Wheel))]
public class WheelEffectPool : MonoBehaviour
{
    [SerializeField] private RoadMaterialList roadMaterialList;
    private Wheel wheel;
    private Dictionary<System.Type, Dictionary<RoadMaterial, ObjectPool<WheelEffectBase>>> effectPools = new();
    private Dictionary<System.Type, Dictionary<RoadMaterial, WheelEffectBase>> activeEffects = new();

    private void Awake()
    {
        wheel = GetComponent<Wheel>();
    }

    public void InitializePools()
    {
        GameObject roadMaterial = GameObject.FindWithTag("RoadMaterialList");
        if (roadMaterial != null)
        {
            roadMaterialList = roadMaterial.GetComponent<RoadMaterialList>();
        }
        if (roadMaterialList == null)
        {
            Debug.LogError("RoadMaterialList not assigned in WheelEffectPool!");
            return;
        }

        effectPools.Clear();
        activeEffects.Clear();

        // Initialize pools for each effect type
        var effectTypes = new[] { typeof(WheelSmoke), typeof(Skidmark), typeof(SkidSound) };
        foreach (var effectType in effectTypes)
        {
            effectPools[effectType] = new Dictionary<RoadMaterial, ObjectPool<WheelEffectBase>>();
            activeEffects[effectType] = new Dictionary<RoadMaterial, WheelEffectBase>();
            foreach (var material in roadMaterialList.GetMaterials())
            {
                WheelEffectBase prefab = null;
                if (effectType == typeof(WheelSmoke))
                {
                    prefab = material.GetEffectPrefab<WheelSmoke>();
                }
                else if (effectType == typeof(Skidmark))
                {
                    prefab = material.GetEffectPrefab<Skidmark>();
                }
                else if (effectType == typeof(SkidSound))
                {
                    prefab = material.GetEffectPrefab<SkidSound>();
                }

                if (prefab != null)
                {
                    //Multiplay effectType * material = poolsize
                    effectPools[effectType][material] = new ObjectPool<WheelEffectBase>(
                        createFunc: () =>
                        {
                            var effect = Instantiate(prefab, wheel.transform);
                            effect.Suppression = true; // Start suppressed
                            return effect;
                        },
                        actionOnGet: effect => effect.gameObject.SetActive(true),
                        actionOnRelease: effect => effect.gameObject.SetActive(false),
                        defaultCapacity: 1,
                        maxSize: 1
                    );
                }
            }
        }
    }

    public T GetEffect<T>(RoadMaterial material) where T : WheelEffectBase
    {
        var effectType = typeof(T);
        if (!effectPools.ContainsKey(effectType) || !effectPools[effectType].ContainsKey(material))
        {
            return null;
        }
        var effect = (T)effectPools[effectType][material].Get();
        effect.Suppression = false;
        activeEffects[effectType][material] = effect;
        return effect;
    }

    public void ReleaseEffect<T>(RoadMaterial material) where T : WheelEffectBase
    {
        var effectType = typeof(T);
        if (activeEffects.ContainsKey(effectType) && activeEffects[effectType].ContainsKey(material))
        {
            var effect = activeEffects[effectType][material];
            effect.Suppression = true;
            effectPools[effectType][material].Release(effect);
            activeEffects[effectType].Remove(material);
        }
    }

    public void PreloadEffects()
    {
        foreach (var effectTypeDict in effectPools)
        {
            foreach (var pool in effectTypeDict.Value.Values)
            {
                var effect = pool.Get();
                pool.Release(effect); // Preload and return to pool
            }
        }
    }

    public void ClearPools()
    {
        foreach (var effectTypeDict in effectPools)
        {
            foreach (var pool in effectTypeDict.Value.Values)
            {
                pool.Clear();
            }
        }
        effectPools.Clear();
        activeEffects.Clear();
    }

    public WheelEffectBase GetActiveEffect<T>(RoadMaterial material) where T : WheelEffectBase
    {
        var effectType = typeof(T);
        return activeEffects.ContainsKey(effectType) && activeEffects[effectType].ContainsKey(material)
            ? activeEffects[effectType][material]
            : null;
    }
}
