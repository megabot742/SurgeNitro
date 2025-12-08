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
        bool hasError = false;
        if (container.cars != null)
        {
            foreach (var car in container.cars)
            {
                if (car != null && car.carClass != container.carClass)
                {
                    hasError = true;
                    break;
                }
            }
        }

        if (hasError)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Some vehicles have a different carClass than the Container\nClick for fix", MessageType.Warning);
        }

        EditorGUILayout.Space();
        GUI.backgroundColor = hasError ? Color.red : Color.green;
        if (GUILayout.Button(hasError ? "Sync all car class to this class container" : "Force Sync All Cars", GUILayout.Height(40)))
        {
            container.SyncCarClassToAllCars();
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AUTO SYNE] Car from {container.cars.Count} to {container.carClass}");
        }
        GUI.backgroundColor = Color.white;
    }
}
#endif