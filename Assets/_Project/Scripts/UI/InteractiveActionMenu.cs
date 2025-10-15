using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveActionMenu : MonoBehaviour
{
    public static InteractiveActionMenu Instance { get; private set; }
    
    [Header("Menu Prefab")]
    [SerializeField] private GameObject menuPrefab;
    
    [Header("Menu Settings")]
    [SerializeField] private float menuScale = 0.01f;
    [SerializeField] private float menuOffsetY = 2f;
    
    private GameObject currentMenu;
    private List<InteractiveActionMenuItem> menuItems = new List<InteractiveActionMenuItem>();
    private int selectedIndex = 0;
    private List<InteractiveAction> currentActions;
    private Action onMenuClosed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Subscribe to input events
        if (PlayerInputManager.Instance != null)
        {
            // TODO: Uncomment after Unity regenerates Actions.cs with Navigate action
            // PlayerInputManager.Instance.OnNavigateActionPerformed += OnNavigateInput;
            PlayerInputManager.Instance.OnInteractActionPerformed += OnInteractInput;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from input events
        if (PlayerInputManager.Instance != null)
        {
            // TODO: Uncomment after Unity regenerates Actions.cs with Navigate action
            // PlayerInputManager.Instance.OnNavigateActionPerformed -= OnNavigateInput;
            PlayerInputManager.Instance.OnInteractActionPerformed -= OnInteractInput;
        }
    }
    
    public void ShowMenu(Transform target, List<InteractiveAction> actions, Action onClose)
    {
        if (currentMenu != null)
        {
            HideMenu();
        }
        
        if (actions.Count == 0) return;
        
        currentActions = actions;
        onMenuClosed = onClose;
        selectedIndex = 0;
        
        // Create menu instance
        currentMenu = Instantiate(menuPrefab, target.position + Vector3.up * menuOffsetY, Quaternion.identity);
        currentMenu.transform.localScale = Vector3.one * menuScale;
        
        // Setup menu items
        SetupMenuItems();
        
        // Update selection
        UpdateSelection();
    }
    
    public void HideMenu()
    {
        if (currentMenu != null)
        {
            Destroy(currentMenu);
            currentMenu = null;
            menuItems.Clear();
            currentActions = null;
            
            onMenuClosed?.Invoke();
            onMenuClosed = null;
        }
    }
    
    private void SetupMenuItems()
    {
        // Find the menu items container
        Transform itemsContainer = currentMenu.transform.Find("MenuItems");
        if (itemsContainer == null)
        {
            Debug.LogError("MenuItems container not found in menu prefab!");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        menuItems.Clear();
        
        // Create menu items
        for (int i = 0; i < currentActions.Count; i++)
        {
            GameObject itemObj = new GameObject($"MenuItem_{i}");
            itemObj.transform.SetParent(itemsContainer, false);
            
            InteractiveActionMenuItem item = itemObj.AddComponent<InteractiveActionMenuItem>();
            item.Setup(currentActions[i].actionName);
            
            menuItems.Add(item);
        }
    }
    
    private void OnNavigateInput(float direction)
    {
        if (currentMenu == null || menuItems.Count == 0) return;
        
        if (direction < 0) // W key - move up
        {
            selectedIndex = (selectedIndex - 1 + menuItems.Count) % menuItems.Count;
        }
        else if (direction > 0) // S key - move down
        {
            selectedIndex = (selectedIndex + 1) % menuItems.Count;
        }
        
        UpdateSelection();
    }
    
    private void OnInteractInput()
    {
        if (currentMenu == null) return;
        
        ExecuteSelectedAction();
    }
    
    public void ExecuteSelectedAction()
    {
        if (currentActions == null || selectedIndex < 0 || selectedIndex >= currentActions.Count)
            return;
        
        // Execute the selected action
        currentActions[selectedIndex].onExecute?.Invoke();
        
        // Close menu after execution
        HideMenu();
    }
    
    private void UpdateSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            menuItems[i].SetSelected(i == selectedIndex);
        }
    }
}
