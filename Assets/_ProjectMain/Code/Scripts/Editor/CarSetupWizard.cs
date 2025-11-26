using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


public enum CarControllerType
{
    Arcade,
    Realistic,
}

public enum CarControlType
{
    None,
    Player,
    ChaseAI,
    AlongPathAI,
}

public class CarSetupWizard : ScriptableWizard
{
    [SerializeField] private GameObject carModelRoot;

    [SerializeField] private string carName = "Car";

    [SerializeField] private float carMass = 1000f;
    [SerializeField] private PhysicsMaterial physicsMaterial;

    [SerializeField] private GameObject carBody;

    [SerializeField] private GameObject[] wheelModels;
    [SerializeField] private GameObject[] steerableWheelModels;

    [SerializeField] private CarControllerType carControllerType = CarControllerType.Realistic;

    [SerializeField] private CarControlType carControlType = CarControlType.Player;

    [SerializeField] private bool attachRoadMaterialDetector = false;

    [Header("Effect")]
    [SerializeField] private WheelSmoke wheelSmokePrefab;
    [SerializeField] private Skidmark skidmarkPrefab;
    [SerializeField] private SkidSound skidSoundPrefab;
    [SerializeField] private bool attackStopSlidingDownSlope = false;

    [Header("Engine")]
    [SerializeField] private EngineSound engineSoundPrefab;
    [SerializeField] private GameObject enginePosition;
    
    [Header("AfterFire")]
    [SerializeField] private AfterFireEffect afterFirePrefab;
    [SerializeField] private GameObject[] afterFirePositions;

    [Header("Light")]
    [SerializeField] private Renderer[] headLightRenderers;
    [SerializeField] private Renderer[] leftBlinkerRenderers;
    [SerializeField] private Renderer[] rightBlinkerRenderers;
    [SerializeField] private Renderer[] brakeLightRenderers;
    [SerializeField] private Renderer[] backLightRenderers;

    [MenuItem("GameObject/FastSetup/Car Setup Wizard")]
    public static void CreateWizard()
    {
        DisplayWizard<CarSetupWizard>("Car Setup Wizard", "Setup", "AutoFind");
    }

    private void OnWizardCreate()
    {
        #region BaseSetup
        if (carModelRoot == null)
        {
            Debug.Log("Car Model Root is not set.");
            return;
        }

        if (carBody == null)
        {
            Debug.Log("Car Body is not set.");
            return;
        }

        Undo.SetCurrentGroupName("Setup Car");
        var group = Undo.GetCurrentGroup();

        var carRoot = new GameObject(carName);
        Undo.RegisterCreatedObjectUndo(carRoot, "");
        Undo.RegisterCompleteObjectUndo(carRoot, "");

        carRoot.transform.position = carModelRoot.transform.position;
        carRoot.transform.rotation = carModelRoot.transform.rotation;

        Undo.SetTransformParent(carModelRoot.transform, carRoot.transform, "");
        carModelRoot.transform.parent = carRoot.transform;

        Undo.RegisterCompleteObjectUndo(carModelRoot, "");
        carModelRoot.transform.localPosition = Vector3.zero;
        carModelRoot.transform.localRotation = Quaternion.identity;

        var rigidbody = Undo.AddComponent<Rigidbody>(carRoot);

        Undo.RegisterCompleteObjectUndo(rigidbody, "");
        rigidbody.mass = carMass;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        var collider = Undo.AddComponent<BoxCollider>(carBody);

        Undo.RegisterCompleteObjectUndo(collider, "");
        collider.material = physicsMaterial;

        CarControllerBase carCon = null;
        switch (carControllerType)
        {
            case CarControllerType.Arcade:
                carCon = Undo.AddComponent<ArcadeCarController>(carRoot);
                break;
            case CarControllerType.Realistic:
                carCon = Undo.AddComponent<RealisticCarController>(carRoot);
                break;
        }
        #endregion
        #region CreateWheel
        var numWheels = wheelModels.Length;
        var wheels = new Wheel[numWheels];
        for (var i = 0; i < numWheels; i++)
        {
            var wheelModel = wheelModels[i];

            var wheelGO = new GameObject($"Wheel{i}");
            Undo.RegisterCreatedObjectUndo(wheelGO, "");

            Undo.SetTransformParent(wheelGO.transform, carRoot.transform, "");

            Undo.RegisterCompleteObjectUndo(wheelGO, "");
            wheelGO.transform.position = wheelModel.transform.position;
            wheelGO.transform.rotation = wheelModel.transform.rotation;

            var wheel = Undo.AddComponent<Wheel>(wheelGO);

            Undo.RegisterCompleteObjectUndo(wheel, "");
            wheel.Model = wheelModel.transform;

            var wheelMR = wheelModel.GetComponentInChildren<MeshRenderer>();
            var wheelR = Mathf.Max(wheelMR.bounds.size.z, wheelMR.bounds.size.y) / 2f;
            var wheelW = wheelMR.bounds.size.x;
            wheel.Radius = wheelR;
            wheel.Width = wheelW;

            wheels[i] = wheel;
        }
        #endregion
        #region SteerableWheels
        var numSteerableWheels = steerableWheelModels.Length;
        var steerableWheels = new Wheel[numSteerableWheels];
        for (var i = 0; i < numSteerableWheels; i++)
        {
            var steerableWheelModel = steerableWheelModels[i];
            for (var j = 0; j < numWheels; j++)
            {
                var wheelModel = wheelModels[j];
                var wheel = wheels[j];
                if (steerableWheelModel == wheelModel)
                {
                    steerableWheels[i] = wheel;
                    break;
                }
            }
        }

        Undo.RegisterCompleteObjectUndo(carCon, "");
        carCon.SteerableWheels = steerableWheels;

        if (wheels.Length > 0)
        {
            carCon.WheelRadius = wheels[0].Radius;
        }
        #endregion
        #region CarController
        switch (carControlType)
        {
            case CarControlType.Player:
                Undo.AddComponent<InputCarController>(carRoot);
                break;
                // case CarControlType.ChaseAI:
                //     Undo.AddComponent<ChaseAI>(carRoot);
                //     break;
                // case CarControlType.AlongPathAI:
                //     Undo.AddComponent<AIPathTracker>(carRoot);
                //     Undo.AddComponent<AlongPathAI>(carRoot);
                //     break;
        }
        #endregion
        #region WheelEffect
        if (attachRoadMaterialDetector)
            {
                foreach (var wheel in wheels)
                {
                    Undo.AddComponent<RoadMaterialDetector>(wheel.gameObject);
                    Undo.AddComponent<WheelSmokeManager>(wheel.gameObject);
                    Undo.AddComponent<SkidmarkManager>(wheel.gameObject);
                    Undo.AddComponent<SkidSoundManager>(wheel.gameObject);
                }

                Undo.AddComponent<FrictionUpdater>(carRoot);
            }
        else
        {
            foreach (var wheel in wheels)
            {
                if (wheelSmokePrefab != null)
                {
                    var wheelSmoke = Instantiate(wheelSmokePrefab, wheel.transform);
                    Undo.RegisterCreatedObjectUndo(wheelSmoke, "");
                }
                if (skidmarkPrefab != null)
                {
                    var skidmark = Instantiate(skidmarkPrefab, wheel.transform);
                    Undo.RegisterCreatedObjectUndo(skidmark, "");
                }
                if (skidSoundPrefab != null)
                {
                    var skidSound = Instantiate(skidSoundPrefab, wheel.transform);
                    Undo.RegisterCreatedObjectUndo(skidSound, "");
                }
            }
        }
        #endregion
        #region Anti-slip
        if (attackStopSlidingDownSlope)
        {
            Undo.AddComponent<StopSlidingDownSlope>(carRoot);
        }
        #endregion
        #region EngineSound
        if (engineSoundPrefab != null && enginePosition != null)
        {
            var engineSound = Instantiate(engineSoundPrefab, carRoot.transform);
            Undo.RegisterCreatedObjectUndo(engineSound, "");

            Undo.RegisterCompleteObjectUndo(engineSound, "");
            engineSound.transform.position = enginePosition.transform.position;
            engineSound.transform.rotation = enginePosition.transform.rotation;
        }
        #endregion
        #region AfterFire
        if ((afterFirePrefab != null && carControllerType == CarControllerType.Realistic) || (afterFirePrefab != null && carControllerType == CarControllerType.Arcade))
        {
            foreach (var afterFirePos in afterFirePositions)
            {
                var afterFire = Instantiate(afterFirePrefab, carRoot.transform);
                Undo.RegisterCreatedObjectUndo(afterFire, "");

                Undo.RegisterCompleteObjectUndo(afterFire, "");
                afterFire.transform.position = afterFirePos.transform.position;
                afterFire.transform.rotation = afterFirePos.transform.rotation;
            }
        }
        #endregion
        #region LightCar
        var lightManager = Undo.AddComponent<CarLightManager>(carRoot);
        Undo.RegisterCompleteObjectUndo(lightManager, "");
        var orange = new Color(1f, 0.5f, 0f);
        if (headLightRenderers.Length > 0)
        {
            lightManager.HeadLight = AddLight(carRoot.transform, "HeadLight", headLightRenderers, false, Color.white);
        }
        if (leftBlinkerRenderers.Length > 0)
        {
            lightManager.LeftBlinker = AddLight(carRoot.transform, "LeftBlinker", leftBlinkerRenderers, true, orange);
        }
        if (rightBlinkerRenderers.Length > 0)
        {
            lightManager.RightBlinker = AddLight(carRoot.transform, "RightBlinker", rightBlinkerRenderers, true, orange);
        }
        if (brakeLightRenderers.Length > 0)
        {
            lightManager.BrakeLight = AddLight(carRoot.transform, "BrakeLight", brakeLightRenderers, false, Color.red);
        }
        if (backLightRenderers.Length > 0)
        {
            lightManager.BackLight = AddLight(carRoot.transform, "BackLight", backLightRenderers, false, Color.white);
        }
        #endregion
        #region Skidmark
        CreateSkidmarMeshIfNotExists();
        #endregion

        #region  CollapseOperations
        Undo.CollapseUndoOperations(group);
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        #endregion
    }

    private void OnWizardOtherButton()
    {
        if (carModelRoot == null)
        {
            Debug.Log("To use Auto Find, you need to set Car Model Root.");
            return;
        }

        var carBodies = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)body", carBodies);
        if (carBodies.Count > 0)
        {
            carBody = carBodies[0];
        }

        var wheelModels = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)wheel", wheelModels);
        this.wheelModels = wheelModels.ToArray();

        var steerableWheelModels = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)wheel.?(?i)f", steerableWheelModels);
        this.steerableWheelModels = steerableWheelModels.ToArray();

        var enginePositions = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)engine.?(?i)position", enginePositions);
        if (enginePositions.Count > 0)
        {
            enginePosition = enginePositions[0];
        }

        var afterFirePositions = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)after.?(?i)fire.?(?i)position", afterFirePositions);
        this.afterFirePositions = afterFirePositions.ToArray();

        var headLights = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(carModelRoot, @"(?i)head.*?((?i)lights?|(?i)lamps?)", headLights);

        var headLightRenderers = new List<Renderer>();
        foreach (var headLight in headLights)
        {
            headLightRenderers.Add(headLight.GetComponent<Renderer>());
        }
        this.headLightRenderers = headLightRenderers.ToArray();

        var blinkers = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(
            carModelRoot,
            @"(?i)indicators?|(?i)blinkers?|(?i)turn.?(?i)signals?",
            blinkers);

        var leftBlinkerRenderers = new List<Renderer>();
        var rightBlinkerRenderers = new List<Renderer>();
        foreach (var blinker in blinkers)
        {
            var renderer = blinker.GetComponent<Renderer>();
            if (Regex.IsMatch(Regex.Replace(blinker.name, @"(?i)blinkers?|(?i)turn.?(?i)signals?", ""), @"(?i)l"))
            {
                leftBlinkerRenderers.Add(renderer);
            }
            else
            {
                rightBlinkerRenderers.Add(renderer);
            }
        }
        this.leftBlinkerRenderers = leftBlinkerRenderers.ToArray();
        this.rightBlinkerRenderers = rightBlinkerRenderers.ToArray();

        var brakeLights = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(
            carModelRoot,
            @"(?i)brake.?((?i)lights?|(?i)lamps?)",
            brakeLights);

        var brakeLightRenderers = new List<Renderer>();
        foreach (var brakeLight in brakeLights)
        {
            brakeLightRenderers.Add(brakeLight.GetComponent<Renderer>());
        }
        this.brakeLightRenderers = brakeLightRenderers.ToArray();

        var backLights = new List<GameObject>();
        AutoFindSetup.FindChildrenByRegex(
            carModelRoot,
            @"(?i)back.?((?i)lights?|(?i)lamps?)",
            backLights);

        var backLightRenderers = new List<Renderer>();
        foreach (var backLight in backLights)
        {
            backLightRenderers.Add(backLight.GetComponent<Renderer>());
        }
        this.backLightRenderers = backLightRenderers.ToArray();

        physicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/UshiSoft/ArcadeCarPhysics/PhysicsMaterials/Car.physicMaterial");
        engineSoundPrefab = AssetDatabase.LoadAssetAtPath<EngineSound>("Assets/UshiSoft/ArcadeCarPhysics/Prefabs/Effect/EngineSound.prefab");
        afterFirePrefab = AssetDatabase.LoadAssetAtPath<AfterFireEffect>("Assets/UshiSoft/ArcadeCarPhysics/Prefabs/Effect/AfterFire.prefab");
        wheelSmokePrefab = AssetDatabase.LoadAssetAtPath<WheelSmoke>("Assets/UshiSoft/ArcadeCarPhysics/Prefabs/Effect/SmokeAsphalt.prefab");
        skidmarkPrefab = AssetDatabase.LoadAssetAtPath<Skidmark>("Assets/UshiSoft/ArcadeCarPhysics/Prefabs/Effect/SkidmarkAsphalt.prefab");
        skidSoundPrefab = AssetDatabase.LoadAssetAtPath<SkidSound>("Assets/UshiSoft/ArcadeCarPhysics/Prefabs/Effect/SkidSoundAsphalt.prefab");
    }

    private CarLight AddLight(Transform carRoot, string name, Renderer[] renderers, bool blink, Color emissionColor)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "");

        Undo.SetTransformParent(go.transform, carRoot, "");

        Undo.RegisterCompleteObjectUndo(go, "");
        go.transform.localPosition = Vector3.zero;

        var light = Undo.AddComponent<CarLight>(go);

        Undo.RegisterCompleteObjectUndo(light, "");
        light.Renderers = renderers;
        light.Blink = blink;
        light.EmissionColor = emissionColor;

        return light;
    }

    private void CreateSkidmarMeshIfNotExists()
    {
        if (FindAnyObjectByType<SkidmarkMesh>() != null)
        {
            return;
        }

        var skidmarkMeshPrefab = AssetDatabase.LoadAssetAtPath<SkidmarkMesh>("Assets/_ProjectMain/Prefabs/Effect/SkidmarkMesh.prefab");
        if (skidmarkMeshPrefab == null)
        {
            return;
        }

        var skidmarkMesh = Instantiate(skidmarkMeshPrefab);
        Undo.RegisterCreatedObjectUndo(skidmarkMesh.gameObject, "");
    }
}


