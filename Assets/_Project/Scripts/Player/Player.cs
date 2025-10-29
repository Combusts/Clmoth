using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Yarn.Unity;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    [Header("摄像机跟随")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraFollowThreshold = 3f; // 触发跟随的距离阈值
    [SerializeField] private float cameraSmoothSpeed = 5f; // 平滑跟随速度
    [SerializeField] private float cameraMinX = -10f; // 摄像机X轴最小位置
    [SerializeField] private float cameraMaxX = 10f; // 摄像机X轴最大位置
    [SerializeField] private bool cameraFollowEnabled = true; // 摄像机跟随开关

    [Header("跳跃")]
    [SerializeField] private bool canJump = true; // 跳跃开关
    [SerializeField] private float jumpForce = 20f; // 跳跃力度
    [SerializeField] private float groundCheckDistance = 0.1f; // 地面检测距离
    [SerializeField] private LayerMask groundLayer; // 地面层

    private List<IInteractive> interactiveObjects = new();

    private IInteractive closestInteractive = null;
    private float direction = 0f;
    [Header("动画")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool isRunning = false;

    // 物理组件
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    void Start()
    {
        // 设置默认朝向为左
        FlipSprite(true);
        
        // 初始化摄像机引用
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 初始化物理组件
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        
        // 检查是否有存档需要恢复位置
        RestorePlayerPosition();

        if (isRunning)
        {
            OnRunning();
        }
        else
        {
            OnStopRunning();
        }
    }

    void OnEnable()
    {
        // 确保 PlayerInputManager 已经初始化
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed += Interact;
            PlayerInputManager.Instance.OnJumpActionPerformed += Jump;
        }
        else
        {
            // 如果 PlayerInputManager 还没准备好，延迟订阅
            StartCoroutine(SubscribeToInputManagerWhenReady());
        }
    }

    void Update()
    {
        // 使用 Rigidbody2D 进行水平移动以支持跳跃
        if (rb != null)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
        else
        {
            transform.Translate(speed * Time.deltaTime * new Vector3(direction, 0, 0));
        }

        animator.SetFloat("Speed", Mathf.Abs(direction));

        UpdateClosestInteractive();
        
        // 摄像机跟随逻辑
        UpdateCameraFollow();
        
        // 定期保存玩家位置
        SavePlayerPosition();

        // 更新动画器状态
        if (animator != null)
        {
            animator.SetBool("IsGrounded", IsGrounded());
        }
    }

    void OnDisable()
    {
        // 安全地取消订阅
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed -= Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled -= Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed -= Interact;
            PlayerInputManager.Instance.OnJumpActionPerformed -= Jump;
        }
    }

    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractive interactiveObject) && interactiveObject.CanInteract)
        {
            interactiveObjects.Add(interactiveObject);
        }

        if (collision.TryGetComponent(out Trap trap))
        {
            // 触发对话
            YarnSpinnerManager.Instance.StartDialogue(trap.dialogueNodeName);
            
            // 如果是一次性陷阱，移除碰撞组件
            if (trap.isOneTime)
            {
                collision.gameObject.SetActive(false);
            }
        }
    }
    public void OnTriggerExit2D(UnityEngine.Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractive interactiveObject))
        {
            interactiveObject.HideHint();
            interactiveObjects.Remove(interactiveObject);
        }
    }

    void Moveperformed(float direction)
    {
        this.direction = direction;
        
        // 根据输入方向更新朝向
        if (direction < 0) // 输入左
        {
            FlipSprite(true);
        }
        else if (direction > 0) // 输入右
        {
            FlipSprite(false);
        }
    }

    // 移动事件取消
    void Movecanceled(float direction)
    {
        this.direction = 0f;
    }

    // 翻转精灵朝向
    void FlipSprite(bool faceLeft)
    {
        Vector3 scale = transform.localScale;
        if (faceLeft)
        {
            scale.x = Mathf.Abs(scale.x); // 确保x轴为正值（朝左）
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x); // 确保x轴为负值（朝右）
        }
        transform.localScale = scale;
    }

    // 等待 PlayerInputManager 准备就绪的协程
    private IEnumerator SubscribeToInputManagerWhenReady()
    {
        // 等待直到 PlayerInputManager.Instance 不为空
        while (PlayerInputManager.Instance == null)
        {
            yield return null; // 等待一帧
        }
        
        // 现在可以安全地订阅事件
        PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
        PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
        PlayerInputManager.Instance.OnInteractActionPerformed += Interact;
        PlayerInputManager.Instance.OnJumpActionPerformed += Jump;
    }


    private IInteractive FindClosestInteractive()
    {
        if (interactiveObjects.Count == 0) 
        {
            return null;
        }

        IInteractive closestInteractive = interactiveObjects[0];
        float closestDistance = Vector3.Distance(transform.position, ((MonoBehaviour)closestInteractive).transform.position);

        for (int i = 1; i < interactiveObjects.Count; i++)
        {
            IInteractive currentInteractive = interactiveObjects[i];
            float currentDistance = Vector3.Distance(transform.position, ((MonoBehaviour)currentInteractive).transform.position);

            if (currentDistance < closestDistance)
            {
                closestInteractive = currentInteractive;
                closestDistance = currentDistance;
            }
        }

        return closestInteractive;
    }

    private void UpdateClosestInteractive()
    {
        IInteractive previousClosest = closestInteractive;
        closestInteractive = FindClosestInteractive();
        
        foreach (IInteractive interactiveObject in interactiveObjects)
        {
            if (interactiveObject != closestInteractive)
            {
                interactiveObject.HideHint();
            }
            else
            {
                interactiveObject.ShowHint();
            }
        }
    }

    public void Interact()
    {
        if (closestInteractive != null)
        {
            closestInteractive.HideHint();
            closestInteractive.Interact();
            interactiveObjects.Remove(closestInteractive);

            if (closestInteractive.CanInteract)
            {
                interactiveObjects.Add(closestInteractive);
            }

            closestInteractive = null;
        }
    }

    // 摄像机跟随逻辑
    private void UpdateCameraFollow()
    {
        if (mainCamera == null || !cameraFollowEnabled) return;

        // 计算玩家相对于屏幕中心的偏移距离
        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, playerScreenPos.z);
        float offsetDistance = Mathf.Abs(playerScreenPos.x - screenCenter.x);

        // 当偏移超过阈值时，开始跟随
        if (offsetDistance > cameraFollowThreshold)
        {
            // 计算目标摄像机位置
            Vector3 currentCameraPos = mainCamera.transform.position;
            Vector3 targetCameraPos = new Vector3(transform.position.x, currentCameraPos.y, currentCameraPos.z);

            // 限制摄像机位置在指定范围内
            targetCameraPos.x = Mathf.Clamp(targetCameraPos.x, cameraMinX, cameraMaxX);

            // 使用 Lerp 实现平滑跟随
            Vector3 newCameraPos = Vector3.Lerp(currentCameraPos, targetCameraPos, cameraSmoothSpeed * Time.deltaTime);
            mainCamera.transform.position = newCameraPos;
        }
    }

    /// <summary>
    /// 手动添加交互对象到列表中（用于处理玩家已在范围内但交互对象刚启用的情况）
    /// </summary>
    /// <param name="interactiveObject">要添加的交互对象</param>
    public void AddInteractiveObject(IInteractive interactiveObject)
    {
        if (interactiveObject != null && interactiveObject.CanInteract && !interactiveObjects.Contains(interactiveObject))
        {
            interactiveObjects.Add(interactiveObject);
            Debug.Log($"[Player] 手动添加交互对象: {interactiveObject.gameObject.name}");
        }
    }

    /// <summary>
    /// 恢复玩家位置
    /// </summary>
    private void RestorePlayerPosition()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            Vector3 savedPosition = SaveManager.Instance.GetPlayerPosition();
            string savedScene = SaveManager.Instance.GetCurrentSceneName();
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // 只有在相同场景时才恢复位置
            if (!string.IsNullOrEmpty(savedScene) && savedScene == currentScene && savedPosition != Vector3.zero)
            {
                transform.position = savedPosition;
                Debug.Log($"[Player] Position restored to: {savedPosition}");
            }
        }
    }
    
    /// <summary>
    /// 保存玩家位置
    /// </summary>
    private void SavePlayerPosition()
    {
        if (SaveManager.Instance != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveManager.Instance.SetPlayerData(transform.position, currentScene);
        }
    }
    
    // 编辑器实时预览
    private void OnValidate()
    {
        if (mainCamera != null)
        {
            // 立即应用摄像机位置限制
            Vector3 currentPos = mainCamera.transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, cameraMinX, cameraMaxX);
            mainCamera.transform.position = currentPos;
        }

        if (cameraFollowEnabled)
        {
            EnableCameraFollow();
        }
        else
        {
            DisableCameraFollow();
        }
    }

    [YarnCommand("OnRunning")]
    public void OnRunning()
    {
        Debug.Log($"[Player] OnRunning");
        isRunning = true;
        animator.SetBool("Running", true);
    }

    [YarnCommand("OnStopRunning")]
    public void OnStopRunning()
    {
        Debug.Log($"[Player] OnStopRunning");
        isRunning = false;
        animator.SetBool("Running", false);
    }

    /// <summary>
    /// 跳跃方法
    /// </summary>
    private void Jump()
    {
        // 检查是否允许跳跃
        if (!canJump)
        {
            return;
        }

        // 检查是否在地面上
        if (!IsGrounded())
        {
            return;
        }

        // 执行跳跃
        if (rb != null)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            Debug.Log("[Player] Jump");
        }
    }

    /// <summary>
    /// 检测是否在地面上
    /// </summary>
    /// <returns>是否在地面上</returns>
    private bool IsGrounded()
    {
        if (capsuleCollider == null)
        {
            return true; // 如果没有碰撞器，默认认为在地面上
        }

        // 在角色底部发射射线检测地面
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - capsuleCollider.bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    /// <summary>
    /// 设置是否允许跳跃
    /// </summary>
    /// <param name="enabled">是否允许跳跃</param>
    public void SetCanJump(bool enabled)
    {
        canJump = enabled;
        Debug.Log($"[Player] CanJump set to: {enabled}");
    }

    /// <summary>
    /// 获取是否允许跳跃
    /// </summary>
    /// <returns>是否允许跳跃</returns>
    public bool GetCanJump()
    {
        return canJump;
    }

    /// <summary>
    /// Yarn命令：启用跳跃
    /// </summary>
    [YarnCommand("EnableJump")]
    public void EnableJump()
    {
        SetCanJump(true);
    }

    /// <summary>
    /// Yarn命令：禁用跳跃
    /// </summary>
    [YarnCommand("DisableJump")]
    public void DisableJump()
    {
        SetCanJump(false);
    }

    /// <summary>
    /// Yarn命令：禁用摄像机跟随
    /// </summary>
    [YarnCommand("DisableCameraFollow")]
    public void DisableCameraFollow()
    {
        cameraFollowEnabled = false;
        Debug.Log("[Player] Camera follow disabled");
    }

    /// <summary>
    /// Yarn命令：启用摄像机跟随
    /// </summary>
    [YarnCommand("EnableCameraFollow")]
    public void EnableCameraFollow()
    {
        cameraFollowEnabled = true;
        Debug.Log("[Player] Camera follow enabled");
    }
}
