using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CarLightController : MonoBehaviour
{
    [SerializeField] Material lightMaterial;
    [SerializeField] float intensityValue = 5f; //default 5

    private Renderer carRenderer; // Kéo thả renderer của đèn hậu vào Inspector
    private MaterialPropertyBlock propBlock;
    private int lightMaterialIndex = -1;           // Sẽ tự tìm
    private static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        carRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        if(carRenderer == null)
        {
            Debug.LogWarning("Wrong Object");
        }
        //Check Material
        if (lightMaterial != null)
        {
            for (int i = 0; i < carRenderer.sharedMaterials.Length; i++)
            {
                if (carRenderer.sharedMaterials[i] == lightMaterial)
                {
                    lightMaterialIndex = i;
                    break;
                }
            }
        }

        if (lightMaterialIndex == -1 || lightMaterial == null)
        {
            Debug.LogWarning("Missing or Wrong Material");
            enabled = false;
            return;
        }
        // Optional: Set default state (tắt đèn)
        ToggleRearLights(false);
    }

    public void ToggleRearLights(bool isOn)
    {
        if (lightMaterialIndex == -1) return;
        // Lấy current properties từ renderer
        carRenderer.GetPropertyBlock(propBlock, 1);

        if (isOn)
        {
            // Bật emission: Set color đỏ với intensity
            propBlock.SetColor("_EmissionColor", Color.white * intensityValue ); // HDR intensity qua multiplier
        }
        else
        {
            // Tắt emission
            propBlock.SetColor("_EmissionColor", Color.black);
        }

        // Áp dụng block trở lại
        carRenderer.SetPropertyBlock(propBlock, 1);
    }
}
