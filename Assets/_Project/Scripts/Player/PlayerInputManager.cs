using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }
    Actions inputActions;

    public Action<float> OnMoveActionPerformed;
    public Action<float> OnMoveActionCanceled;
    public Action OnEseActionPerformed;
    public Action OnInteractActionPerformed;
    public Action<float> OnNavigateActionPerformed;
    
    [Header("Input State")]
    [SerializeField] private bool gameplayInputEnabled = true;

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化输入系统
        inputActions = new Actions();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Ese.performed += OnEsePerformed;
        inputActions.Player.Interact.performed += OnInteractPerformed;
        // TODO: Uncomment after Unity regenerates Actions.cs with Navigate action
        // inputActions.Player.Navigate.performed += OnNavigatePerformed;
        inputActions.Enable();
    }
    
    private void OnDestroy()
    {        
        // 清理输入系统
        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions.Player.Move.performed -= OnMovePerformed;
            inputActions.Player.Move.canceled -= OnMoveCanceled;
            inputActions.Player.Ese.performed -= OnEsePerformed;
            inputActions.Player.Interact.performed -= OnInteractPerformed;
            // TODO: Uncomment after Unity regenerates Actions.cs with Navigate action
            // inputActions.Player.Navigate.performed -= OnNavigatePerformed;
        }
    }

    // 移动事件
    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!gameplayInputEnabled) return;
        
        float value = context.ReadValue<float>();
        OnMoveActionPerformed?.Invoke(value);
    }

    // 移动事件取消
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!gameplayInputEnabled) return;
        
        float value = context.ReadValue<float>();
        OnMoveActionCanceled?.Invoke(value);
    }

    // 退出事件
    void OnEsePerformed(InputAction.CallbackContext context)
    {
        // ESC key always works, even during dialogue
        OnEseActionPerformed?.Invoke();
    }

    // 交互事件
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!gameplayInputEnabled) return;
        
        OnInteractActionPerformed?.Invoke();
    }
    
    // 导航事件
    void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        if (!gameplayInputEnabled) return;
        
        float value = context.ReadValue<float>();
        OnNavigateActionPerformed?.Invoke(value);
    }
    
    // Public methods for dialogue system
    public void DisableGameplayInput()
    {
        gameplayInputEnabled = false;
        Debug.Log("[PlayerInputManager] Gameplay input disabled");
    }
    
    public void EnableGameplayInput()
    {
        gameplayInputEnabled = true;
        Debug.Log("[PlayerInputManager] Gameplay input enabled");
    }
    
    public bool IsGameplayInputEnabled => gameplayInputEnabled;

    

}
