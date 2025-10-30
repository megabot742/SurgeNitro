using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Wheel))]
[DisallowMultipleComponent]
public class RoadMaterialDetector : MonoBehaviour
{
   private Wheel wheel;
    private RoadMaterialList roadMaterialList;
    private RoadMaterial roadMaterial;

    [HideInInspector] public UnityEvent<RoadMaterial> OnMaterialChanged = new UnityEvent<RoadMaterial>();

    public Wheel Wheel => wheel;
    public RoadMaterial Material => roadMaterial;

    private void Awake()
    {
        wheel = GetComponent<Wheel>();
        roadMaterialList = FindAnyObjectByType<RoadMaterialList>();
    }

    private void FixedUpdate()
    {
        DetectMaterial();
        UpdateGroundOffset();
    }

    private void DetectMaterial()
    {
        var oldMaterial = roadMaterial;
        roadMaterial = null;

        roadMaterial = GetMaterialByMaterialIndex();
        if (roadMaterial != null)
        {
            if (oldMaterial != roadMaterial)
            {
                OnMaterialChanged.Invoke(roadMaterial);
            }
            return;
        }

        roadMaterial = GetMaterialByTag();
        if (roadMaterial != null)
        {
            if (oldMaterial != roadMaterial)
            {
                OnMaterialChanged.Invoke(roadMaterial);
            }
            return;
        }

        roadMaterial = GetMaterialByLayer();
        if (roadMaterial != null)
        {
            if (oldMaterial != roadMaterial)
            {
                OnMaterialChanged.Invoke(roadMaterial);
            }
            return;
        }

        roadMaterial = GetMaterialByTerrainTexture();
        if (oldMaterial != roadMaterial)
        {
            OnMaterialChanged.Invoke(roadMaterial);
        }
    }

    private RoadMaterial GetMaterialByLayer()
    {
        if (roadMaterialList == null)
        {
            return null;
        }

        if (!wheel.Grounded)
        {
            return null;
        }

        return roadMaterialList.GetMaterialByLayer(wheel.HitInfo.collider.gameObject.layer);
    }

    private RoadMaterial GetMaterialByTag()
    {
        if (roadMaterialList == null)
        {
            return null;
        }

        if (!wheel.Grounded)
        {
            return null;
        }

        return roadMaterialList.GetMaterialByTag(wheel.HitInfo.collider.tag);
    }

    private RoadMaterial GetMaterialByTerrainTexture()
    {
        if (roadMaterialList == null)
        {
            return null;
        }

        if (!wheel.Grounded)
        {
            return null;
        }

        var terrain = wheel.HitInfo.collider.GetComponent<Terrain>();
        if (terrain == null)
        {
            return null;
        }

        var terrainData = terrain.terrainData;

        var terrainPos = wheel.HitInfo.point - wheel.HitInfo.collider.transform.position;
        var alphamapX = Mathf.FloorToInt((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
        var alphamapY = Mathf.FloorToInt((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

        var alphamaps = terrainData.GetAlphamaps(alphamapX, alphamapY, 1, 1);

        var bestTexIndex = -1;
        var maxTexWeight = float.MinValue;
        for (var i = 0; i < terrainData.alphamapLayers; i++)
        {
            var texWeight = alphamaps[0, 0, i];
            if (texWeight > maxTexWeight)
            {
                bestTexIndex = i;
                maxTexWeight = texWeight;
            }
        }

        return roadMaterialList.GetMaterialByTerrainLayer(bestTexIndex);
    }

    private RoadMaterial GetMaterialByMaterialIndex()
    {
        if (roadMaterialList == null)
        {
            return null;
        }

        if (!wheel.Grounded)
        {
            return null;
        }

        var meshCollider = wheel.HitInfo.collider.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            return null;
        }

        var numTotalFaces = 0;
        var materialIndex = -1;
        for (var i = 0; i < meshCollider.sharedMesh.subMeshCount; i++)
        {
            var numFaces = meshCollider.sharedMesh.GetSubMesh(i).indexCount / 3;
            numTotalFaces += numFaces;
            if (wheel.HitInfo.triangleIndex < numTotalFaces)
            {
                materialIndex = i;
                break;
            }
        }

        return roadMaterialList.GetMaterialByMaterialIndex(materialIndex);
    }

    private void UpdateGroundOffset()
    {
        if (wheel.Grounded && roadMaterial != null)
        {
            var hitInfo = wheel.HitInfo;
            var noiseX = hitInfo.point.x * roadMaterial.BumpPeriod;
            var noiseY = hitInfo.point.z * roadMaterial.BumpPeriod;
            var bump = (Mathf.PerlinNoise(noiseX, noiseY) - 0.5f) * roadMaterial.BumpAmplitude;
            wheel.GroundOffset = bump;
        }
        else
        {
            wheel.GroundOffset = 0f;
        }
    }
}
