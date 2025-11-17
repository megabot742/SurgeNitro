using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RaceManager : BaseManager<RaceManager>
{
    public CarControllerBase playerCar;
    public CameraCarController cameraCarController;
    public FullScreenPassRendererFeature screenFeature;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        FindScreenShaderFeature();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void FindScreenShaderFeature()
    {
        var allRendererDatas = Resources.FindObjectsOfTypeAll<UniversalRendererData>();

        foreach (var rendererData in allRendererDatas)
        {
            // rendererData.rendererFeatures là List<ScriptableRendererFeature>
            var features = rendererData.rendererFeatures;
            if (features == null) continue;

            for (int i = 0; i < features.Count; i++)
            {
                var feature = features[i];
                if (feature != null && feature is FullScreenPassRendererFeature fullscreenFeature)
                {
                    screenFeature = fullscreenFeature;
                    Debug.Log($"[SpeedUpShader] Find {screenFeature.name} (from {rendererData.name})");
                    return; // Tìm được → thoát ngay, không loop nữa
                }
            }
        }
    }
}
