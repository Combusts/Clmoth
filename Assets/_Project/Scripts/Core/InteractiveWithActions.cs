using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveWithActions : MonoBehaviour, IInteractive
{
    [Header("Action Configuration")]
    [SerializeField] protected List<InteractiveAction> actions = new List<InteractiveAction>();
    
    [Header("Menu Settings")]
    [SerializeField] protected GameObject actionMenuPrefab;
    [SerializeField] protected Vector3 menuOffset = Vector3.up * 2f;
    
    [Header("Visual Settings")]
    [SerializeField] protected Color interactingColor = Color.gray;
    
    // IInteractive properties
    public bool IsShowHint { get; set; }
    public bool CanInteract { get; set; } = true;
    
    // Internal state
    protected Color originalColor;
    protected bool isMenuOpen = false;
    
    protected virtual void Awake()
    {
        StoreOriginalVisualState();
    }
    
    protected virtual void Start()
    {
        // Subscribe to input events
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteractActionPerformed += OnInteractInput;
        }
    }
    
    protected virtual void OnDestroy()
    {
        // Unsubscribe from input events
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteractActionPerformed -= OnInteractInput;
        }
    }
    
    public virtual void Interact()
    {
        if (actions.Count == 0 || isMenuOpen) return;
        
        CanInteract = false;
        isMenuOpen = true;
        SetVisualState(interactingColor);
        PlayerInputManager.Instance.DisableGameplayInput();
        
        InteractiveActionMenu.Instance.ShowMenu(
            transform, 
            actions, 
            OnMenuClosed
        );
    }
    
    protected virtual void OnInteractInput()
    {
        // Only handle interact input when menu is open
        if (isMenuOpen)
        {
            InteractiveActionMenu.Instance.ExecuteSelectedAction();
        }
    }
    
    protected virtual void OnMenuClosed()
    {
        CanInteract = true;
        isMenuOpen = false;
        SetVisualState(originalColor);
        PlayerInputManager.Instance.EnableGameplayInput();
    }
    
    protected abstract void StoreOriginalVisualState();
    protected abstract void SetVisualState(Color color);
    
    public abstract void ShowHint();
    public abstract void HideHint();
}
