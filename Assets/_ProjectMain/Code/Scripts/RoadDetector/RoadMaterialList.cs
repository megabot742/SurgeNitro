using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoadMaterialList : MonoBehaviour
{
    [System.Serializable]
    private class LayerRoadMaterialIndexPair
    {
        [SerializeField] private int _layer;
        [SerializeField] private int _roadMaterialIndex;

        public int Layer => _layer;
        public int RoadMaterialIndex => _roadMaterialIndex;
    }

    [System.Serializable]
    private class TagRoadMaterialIndexPair
    {
        [SerializeField] private string _tag;
        [SerializeField] private int _roadMaterialIndex;

        public string Tag => _tag;
        public int RoadMaterialIndex => _roadMaterialIndex;
    }

    [System.Serializable]
    private class TerrainLayerRoadMaterialIndexPair
    {
        [SerializeField] private int _terrainLayer;
        [SerializeField] private int _roadMaterialIndex;

        public int TerrainLayer => _terrainLayer;
        public int RoadMaterialIndex => _roadMaterialIndex;
    }

    [System.Serializable]
    private class MaterialIndexRoadMaterialIndexPair
    {
        [SerializeField] private int _materialIndex;
        [SerializeField] private int _roadMaterialIndex;

        public int MaterialIndex => _materialIndex;
        public int RoadMaterialIndex => _roadMaterialIndex;
    }

    [SerializeField] private RoadMaterial[] _materials;

    [SerializeField] private LayerRoadMaterialIndexPair[] _layer;
    [SerializeField] private TagRoadMaterialIndexPair[] _tag;
    [SerializeField] private TerrainLayerRoadMaterialIndexPair[] _terrainLayer;
    [SerializeField] private MaterialIndexRoadMaterialIndexPair[] _materialIndex;

    private Dictionary<int, RoadMaterial> _layerToMaterial = new();
    private Dictionary<string, RoadMaterial> _tagToMaterial = new();
    private Dictionary<int, RoadMaterial> _terrainLayerToMaterial = new();
    private Dictionary<int, RoadMaterial> _materialIndexToMaterial = new();

    private void Awake()
    {
        UpdateLinks();
    }

    public RoadMaterial[] GetMaterials()
    {
        return _materials;
    }

    public RoadMaterial GetMaterialByLayer(int layer)
    {
        if (!_layerToMaterial.ContainsKey(layer))
        {
            return null;
        }
        return _layerToMaterial[layer];
    }

    public RoadMaterial GetMaterialByTag(string tag)
    {
        if (!_tagToMaterial.ContainsKey(tag))
        {
            return null;
        }
        return _tagToMaterial[tag];
    }

    public RoadMaterial GetMaterialByTerrainLayer(int terrainLayer)
    {
        if (!_terrainLayerToMaterial.ContainsKey(terrainLayer))
        {
            return null;
        }
        return _terrainLayerToMaterial[terrainLayer];
    }

    public RoadMaterial GetMaterialByMaterialIndex(int materialIndex)
    {
        if (!_materialIndexToMaterial.ContainsKey(materialIndex))
        {
            return null;
        }
        return _materialIndexToMaterial[materialIndex];
    }

    public void UpdateLinks()
    {
        _layerToMaterial.Clear();
        foreach (var pair in _layer)
        {
            if (pair.RoadMaterialIndex >= _materials.Length)
            {
                continue;
            }
            _layerToMaterial[pair.Layer] = _materials[pair.RoadMaterialIndex];
        }

        _tagToMaterial.Clear();
        foreach (var pair in _tag)
        {
            if (pair.RoadMaterialIndex >= _materials.Length)
            {
                continue;
            }
            _tagToMaterial[pair.Tag] = _materials[pair.RoadMaterialIndex];
        }

        _terrainLayerToMaterial.Clear();
        foreach (var pair in _terrainLayer)
        {
            if (pair.RoadMaterialIndex >= _materials.Length)
            {
                continue;
            }
            _terrainLayerToMaterial[pair.TerrainLayer] = _materials[pair.RoadMaterialIndex];
        }

        _materialIndexToMaterial.Clear();
        foreach (var pair in _materialIndex)
        {
            if (pair.RoadMaterialIndex >= _materials.Length)
            {
                continue;
            }
            _materialIndexToMaterial[pair.MaterialIndex] = _materials[pair.RoadMaterialIndex];
        }
    }
}
