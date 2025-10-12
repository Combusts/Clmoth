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
        }
    }

    // 移动事件
    void OnMovePerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        OnMoveActionPerformed?.Invoke(value);
    }

    // 移动事件取消
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        OnMoveActionCanceled?.Invoke(value);
    }

    // 退出事件
    void OnEsePerformed(InputAction.CallbackContext context)
    {
        OnEseActionPerformed?.Invoke();
    }

    // 交互事件
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractActionPerformed?.Invoke();
    }
}
