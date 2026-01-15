#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarClassContainerSO))]
public class CarClassContainerSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CarClassContainerSO container = (CarClassContainerSO)target;

        //Check car with class
        bool hasClassError = false;
        if (container.cars != null)
        {
            foreach (var car in container.cars)
            {
                if (car != null && car.carClass != container.carClass)
                {
                    hasClassError = true;
                    break;
                }
            }
        }
        //Check car with rank
        bool hasRankError = false;
        if (container.cars != null)
        {
            foreach (var car in container.cars)
            {
                if (car != null)
                {
                    // Check nếu currentRank != baseRank (giả sử level=0)
                    if (car.carCurrentRank < car.carBaseRank)
                    {
                        hasRankError = true;
                        break;
                    }
                    // Check maxRank != base + 200
                    if (car.carMaxRank != car.carBaseRank + 200)
                    {
                        hasRankError = true;
                        break;
                    }
                }
            }
        }

        if (hasClassError)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Some vehicles have a different carClass than the Container\nClick for fix", MessageType.Warning);
        }
        if (hasRankError)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Some vehicles have incorrect curentranks\nClick to fix", MessageType.Warning);
        }

        //Check rank and class
        EditorGUILayout.Space();
        GUI.backgroundColor = hasClassError || hasRankError ? Color.red : Color.green;
        if (GUILayout.Button(hasClassError || hasRankError ? "Some car data has changed." : "Force sync rest all to base", GUILayout.Height(40)))
        {
            container.SyncCarClassToAllCars();
            ResetAllToBase(container); //Function
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RESET ALL] Cars in {container.name}: Reset to base values, levels cleared.");
        }
        GUI.backgroundColor = Color.white;

        //Check Upgrade value
        EditorGUILayout.Space();
        if (GUILayout.Button("Force Update All Values & Ranks (Preview)", GUILayout.Height(40)))
        {
            UpdateAllValuesAndRanks(container);
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PREVIEW UPDATE] Updated current values & ranks for all cars in {container.name}");
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Sync Levels from PlayerPrefs (Load Runtime Data)", GUILayout.Height(40)))
        {
            SyncLevelsFromPlayerPrefs(container);
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SYNC RUNTIME] Levels synced from PlayerPrefs to Editor for {container.name}");
        }
    }
    #region Reset
    private void ResetAllToBase(CarClassContainerSO container)
    {
        if (container.cars == null) return;

        foreach (var car in container.cars)
        {
            if (car != null)
            {
                // Sync class
                car.carClass = container.carClass;

                // Reset rank
                car.carCurrentRank = car.carBaseRank;

                // Reset all stats levels to 0
                if (car.stats != null)
                {
                    foreach (var stat in car.stats)
                    {
                        if (stat != null)
                        {
                            stat.CurrentLevel = 0;
                            stat.OnValidate();  // Force update currentValue về base
                        }
                    }
                }
            }
        }
    }
    #endregion
    private void UpdateAllValuesAndRanks(CarClassContainerSO container)
    {
        if (container.cars == null) return;

        foreach (var car in container.cars)
        {
            if (car != null && car.stats != null)
            {
                int totalUpgrades = 0;
                foreach (var stat in car.stats)
                {
                    if (stat != null)
                    {
                        // Force tính currentValue
                        stat.OnValidate();  // Gọi thủ công để update
                        totalUpgrades += stat.CurrentLevel;
                    }
                }
                // Preview rank: base + 5 * total levels (chỉ Editor preview, không lưu nếu không muốn)
                car.carCurrentRank = car.carBaseRank + (totalUpgrades * 5);
            }
        }
    }
    #region Sync from PlayerPrefs
    //Runetime
    private void SyncLevelsFromPlayerPrefs(CarClassContainerSO container)
    {
        if (!PlayerManager.HasInstance)
        {
            Debug.LogWarning("PlayerManager not found! Cannot sync from PlayerPrefs.");
            return;
        }

        if (container.cars == null) return;

        foreach (var car in container.cars)
        {
            if (car != null && car.stats != null)
            {
                PlayerCarData playerData = PlayerManager.Instance.GetPlayerCarData(car.carName);
                if (playerData != null && playerData.stats != null)
                {
                    // Sync levels từ runtime vào Editor stats
                    for (int i = 0; i < car.stats.Length && i < playerData.stats.Length; i++)
                    {
                        if (car.stats[i] != null && playerData.stats[i] != null &&
                            car.stats[i].statType == playerData.stats[i].statType)
                        {
                            car.stats[i].CurrentLevel = playerData.stats[i].CurrentLevel;
                            car.stats[i].GoldUpgrade = playerData.stats[i].GoldUpgrade;
                            car.stats[i].OnValidate();  // Update currentValue
                        }
                    }

                    // Update preview rank
                    int totalUpgrades = playerData.GetTotalUpgradesDone();
                    car.carCurrentRank = car.carBaseRank + (totalUpgrades * 5);
                }
            }
        }
    }
    #endregion
}
#endif