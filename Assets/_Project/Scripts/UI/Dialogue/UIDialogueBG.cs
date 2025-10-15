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
    
    private void Start()
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
        
        // Create material instance to avoid modifying shared material
        if (image.material != null)
        {
            materialInstance = new Material(image.material);
            image.material = materialInstance;
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
