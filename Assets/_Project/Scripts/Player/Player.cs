using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    [Header("摄像机跟随")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraFollowThreshold = 3f; // 触发跟随的距离阈值
    [SerializeField] private float cameraSmoothSpeed = 5f; // 平滑跟随速度
    [SerializeField] private float cameraMinX = -10f; // 摄像机X轴最小位置
    [SerializeField] private float cameraMaxX = 10f; // 摄像机X轴最大位置

    private List<IInteractive> interactiveObjects = new();

    private IInteractive closestInteractive = null;
    private float direction = 0f;
    [Header("动画")]
    [SerializeField] private Animator animator;

    void Start()
    {
        // 设置默认朝向为左
        FlipSprite(true);
        
        // 初始化摄像机引用
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 检查是否有存档需要恢复位置
        RestorePlayerPosition();
    }

    void OnEnable()
    {
        // 确保 PlayerInputManager 已经初始化
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed += Interact;
        }
        else
        {
            // 如果 PlayerInputManager 还没准备好，延迟订阅
            StartCoroutine(SubscribeToInputManagerWhenReady());
        }
    }

    void Update()
    {
        transform.Translate(speed * Time.deltaTime * new Vector3(direction, 0, 0));

        animator.SetFloat("Speed", Mathf.Abs(direction));

        UpdateClosestInteractive();
        
        // 摄像机跟随逻辑
        UpdateCameraFollow();
        
        // 定期保存玩家位置
        SavePlayerPosition();
    }

    void OnDisable()
    {
        // 安全地取消订阅
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed -= Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled -= Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed -= Interact;
        }
    }

    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractive interactiveObject) && interactiveObject.CanInteract)
        {
            interactiveObjects.Add(interactiveObject);
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
        if (mainCamera == null) return;

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
    }
}
