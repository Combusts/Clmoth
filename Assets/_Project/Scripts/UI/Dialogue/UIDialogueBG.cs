using UnityEngine;
using UnityEngine.UI;

public class UIDialogueBG : MonoBehaviour
{
    [Header("Shader Parameters")]
    [SerializeField] private float roundedRadius = 64f;
    [SerializeField] private float borderWidth = 1f;
    [SerializeField] private Color borderColor = Color.red;
    
    private Image image;
    private RectTransform rectTransform;
    private Material materialInstance;
    
    // Shader property IDs for better performance
    private static readonly int WidthProperty = Shader.PropertyToID("_Width");
    private static readonly int HeightProperty = Shader.PropertyToID("_Height");
    private static readonly int RoundedRadiusProperty = Shader.PropertyToID("_RoundedRadius");
    private static readonly int BorderWidthProperty = Shader.PropertyToID("_BorderWidth");
    private static readonly int BorderColorProperty = Shader.PropertyToID("_BorderColor");
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void OnValidate()
    {
        // This method is called in the editor when values change
        if (Application.isPlaying)
        {
            // Only update during play mode
            UpdateShaderProperties();
        }
        else
        {
            // In edit mode, initialize components and update
            InitializeComponents();
        }
    }
    
    private void InitializeComponents()
    {
        // Get component references
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (image == null)
        {
            Debug.LogError("UIDialogueBG: Image component not found!", this);
            return;
        }
        
        if (rectTransform == null)
        {
            Debug.LogError("UIDialogueBG: RectTransform component not found!", this);
            return;
        }
        
        // Validate and create material instance
        if (image.material != null)
        {
            // Check if the material is the correct rounded material
            if (image.material.name.Contains("UIRounded") || image.material.shader.name.Contains("RoundConorNew"))
            {
                materialInstance = new Material(image.material);
                image.material = materialInstance;
                Debug.Log($"UIDialogueBG: Successfully initialized rounded material for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"UIDialogueBG: Material '{image.material.name}' is not a rounded material. Expected UIRounded material.", this);
                // Try to load the correct material from Resources
                Material correctMaterial = Resources.Load<Material>("Materials/UIRounded");
                if (correctMaterial != null)
                {
                    materialInstance = new Material(correctMaterial);
                    image.material = materialInstance;
                    Debug.Log($"UIDialogueBG: Loaded correct rounded material from Resources for {gameObject.name}");
                }
                else
                {
                    Debug.LogError("UIDialogueBG: Could not find UIRounded material in Resources folder!", this);
                }
            }
        }
        else
        {
            Debug.LogWarning($"UIDialogueBG: No material assigned to Image component on {gameObject.name}. Attempting to load default rounded material.", this);
            // Try to load the correct material from Resources
            Material correctMaterial = Resources.Load<Material>("Materials/UIRounded");
            if (correctMaterial != null)
            {
                materialInstance = new Material(correctMaterial);
                image.material = materialInstance;
                Debug.Log($"UIDialogueBG: Loaded rounded material from Resources for {gameObject.name}");
            }
            else
            {
                Debug.LogError("UIDialogueBG: Could not find UIRounded material in Resources folder!", this);
            }
        }
        
        // Initial setup
        UpdateShaderProperties();
    }
    
    private void OnRectTransformDimensionsChange()
    {
        // Update shader properties when dimensions change
        UpdateShaderProperties();
    }
    
    private void UpdateShaderProperties()
    {
        if (materialInstance == null) return;
        
        // Get current dimensions from RectTransform
        Rect rect = rectTransform.rect;
        float width = rect.width;
        float height = rect.height;
        
        // Apply shader parameters
        materialInstance.SetFloat(WidthProperty, width);
        materialInstance.SetFloat(HeightProperty, height);
        materialInstance.SetFloat(RoundedRadiusProperty, roundedRadius);
        materialInstance.SetFloat(BorderWidthProperty, borderWidth);
        materialInstance.SetColor(BorderColorProperty, borderColor);
    }
    
    // Public methods to update shader parameters at runtime
    public void SetRoundedRadius(float radius)
    {
        roundedRadius = radius;
        UpdateShaderProperties();
    }
    
    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        UpdateShaderProperties();
    }
    
    public void SetBorderColor(Color color)
    {
        borderColor = color;
        UpdateShaderProperties();
    }
    
    // Getters for current values
    public float GetRoundedRadius() => roundedRadius;
    public float GetBorderWidth() => borderWidth;
    public Color GetBorderColor() => borderColor;
    
    // Cleanup material instance when object is destroyed
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
        }
    }
}
