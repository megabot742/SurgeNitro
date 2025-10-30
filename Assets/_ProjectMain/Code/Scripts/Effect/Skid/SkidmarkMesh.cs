using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[DisallowMultipleComponent]
public class SkidmarkMesh : MonoBehaviour
{
    [SerializeField, Min(2048f)] private int _maxSections = 2048;
    [SerializeField, Min(0f)] private float _groundOffset = 0.01f;
    [SerializeField, Min(0f)] private float _minDistance = 0.1f;

    private MeshFilter _meshFilter;

    private SkidmarkMeshSection[] _sections;
    private int _headSectionIndex;

    private Mesh _mesh;

    private Vector3[] _vertices;
    private Vector3[] _normals;
    private Vector4[] _tangents;
    private Color[] _colors;
    private int[] _triangles;

    private bool _dirty;
    private bool _setBounds;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();

        _mesh = new Mesh();
        _mesh.MarkDynamic();

        _sections = new SkidmarkMeshSection[_maxSections];
        for (var i = 0; i < _maxSections; i++)
        {
            _sections[i] = new SkidmarkMeshSection();
        }

        _vertices = new Vector3[_maxSections * 4];
        _normals = new Vector3[_maxSections * 4];
        _tangents = new Vector4[_maxSections * 4];
        _colors = new Color[_maxSections * 4];
        _triangles = new int[_maxSections * 6];
    }

    private void LateUpdate()
    {
        UpdateMesh();
    }

    public int Leave(Vector3 position, Vector3 normal, Color color, float width, int lastSectionIndex)
    {
        if (color.a == 0f)
        {
            return -1;
        }

        var currSect = _sections[_headSectionIndex];
        currSect.Position = position + normal * _groundOffset;
        currSect.Normal = normal;
        currSect.Color = color;
        currSect.LastIndex = lastSectionIndex;

        if (lastSectionIndex != -1)
        {
            var lastSect = _sections[lastSectionIndex];

            var diff = currSect.Position - lastSect.Position;
            if (diff.sqrMagnitude < _minDistance * _minDistance)
            {
                return lastSectionIndex;
            }

            var binormal = Vector3.Cross(diff, normal).normalized;
            currSect.Tangent = new Vector4(binormal.x, binormal.y, binormal.z, 1f);
            currSect.PositionLeft = currSect.Position + binormal * width / 2f;
            currSect.PositionRight = currSect.Position - binormal * width / 2f;

            if (lastSect.LastIndex == -1)
            {
                lastSect.Tangent = currSect.Tangent;
                lastSect.PositionLeft = currSect.PositionLeft;
                lastSect.PositionRight = currSect.PositionRight;
            }
        }

        UpdateVertices();

        var currSectIndex = _headSectionIndex;
        _headSectionIndex = (_headSectionIndex + 1) % _maxSections;
        return currSectIndex;
    }

    private void UpdateVertices()
    {
        var currSect = _sections[_headSectionIndex];

        if (currSect.LastIndex == -1)
        {
            return;
        }

        var lastSect = _sections[currSect.LastIndex];

        _vertices[_headSectionIndex * 4 + 0] = lastSect.PositionLeft;
        _vertices[_headSectionIndex * 4 + 1] = lastSect.PositionRight;
        _vertices[_headSectionIndex * 4 + 2] = currSect.PositionLeft;
        _vertices[_headSectionIndex * 4 + 3] = currSect.PositionRight;

        _normals[_headSectionIndex * 4 + 0] = lastSect.Normal;
        _normals[_headSectionIndex * 4 + 1] = lastSect.Normal;
        _normals[_headSectionIndex * 4 + 2] = currSect.Normal;
        _normals[_headSectionIndex * 4 + 3] = currSect.Normal;

        _tangents[_headSectionIndex * 4 + 0] = lastSect.Tangent;
        _tangents[_headSectionIndex * 4 + 1] = lastSect.Tangent;
        _tangents[_headSectionIndex * 4 + 2] = currSect.Tangent;
        _tangents[_headSectionIndex * 4 + 3] = currSect.Tangent;

        _colors[_headSectionIndex * 4 + 0] = lastSect.Color;
        _colors[_headSectionIndex * 4 + 1] = lastSect.Color;
        _colors[_headSectionIndex * 4 + 2] = currSect.Color;
        _colors[_headSectionIndex * 4 + 3] = currSect.Color;

        _triangles[_headSectionIndex * 6 + 0] = _headSectionIndex * 4 + 0;
        _triangles[_headSectionIndex * 6 + 1] = _headSectionIndex * 4 + 2;
        _triangles[_headSectionIndex * 6 + 2] = _headSectionIndex * 4 + 1;

        _triangles[_headSectionIndex * 6 + 3] = _headSectionIndex * 4 + 2;
        _triangles[_headSectionIndex * 6 + 4] = _headSectionIndex * 4 + 3;
        _triangles[_headSectionIndex * 6 + 5] = _headSectionIndex * 4 + 1;

        _dirty = true;
    }

    private void UpdateMesh()
    {
        if (!_dirty)
        {
            return;
        }

        _mesh.vertices = _vertices;
        _mesh.normals = _normals;
        _mesh.tangents = _tangents;
        _mesh.colors = _colors;
        _mesh.triangles = _triangles;

        if (!_setBounds)
        {
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            _setBounds = true;
        }

        _meshFilter.sharedMesh = _mesh;

        _dirty = false;
    }
}
public class SkidmarkMeshSection
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public Vector3 Tangent { get; set; }
    public Vector3 PositionLeft { get; set; }
    public Vector3 PositionRight { get; set; }
    public Color Color { get; set; }
    public int LastIndex { get; set; }
}
