using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractiveActionMenuItem : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalBackgroundColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Color selectedBackgroundColor = new Color(1, 1, 0, 0.3f);
    
    private TextMeshProUGUI textComponent;
    private Image backgroundImage;
    private bool isSelected = false;
    
    private void Awake()
    {
        SetupComponents();
    }
    
    private void SetupComponents()
    {
        // Add RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Set size
        rectTransform.sizeDelta = new Vector2(200, 40);
        
        // Add background image
        backgroundImage = gameObject.AddComponent<Image>();
        backgroundImage.color = normalBackgroundColor;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(transform, false);
        
        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Action";
        textComponent.fontSize = 24;
        textComponent.color = normalColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    public void Setup(string actionName)
    {
        if (textComponent != null)
        {
            textComponent.text = actionName;
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (textComponent != null)
        {
            textComponent.color = isSelected ? selectedColor : normalColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedBackgroundColor : normalBackgroundColor;
        }
    }
}
