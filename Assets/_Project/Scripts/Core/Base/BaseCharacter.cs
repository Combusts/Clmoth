using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BaseCharacter : MonoBehaviour
{
    [Header("移动状态")]
    [SerializeField] private bool onMove = false;
    [SerializeField] private bool onRunning = false;
    
    [Header("表情控制")]
    [SerializeField] private int emotionState = 0;
    public List<Sprite> emotionSprites = new();
    
    [Header("追逐设置")]
    [SerializeField] private bool isChasing = false;
    [SerializeField] private float chaseStopDistance = 1f; // 停止追逐的距离
    [SerializeField] private bool flipTowardsTarget = true; // 是否朝向目标翻转
    [SerializeField] private float chaseSpeedThreshold = 5f; // 切换速度的距离阈值
    [SerializeField] private float farChaseSpeed = 7f; // 远距离追逐速度（距离大于阈值时）
    [SerializeField] private float closeChaseSpeed = 3f; // 近距离追逐速度（距离小于阈值时）
    
    private Animator animator;
    private Rigidbody2D rb; // 物理组件，用于支持物理移动
    private bool lastOnMoveValue;
    private bool lastOnRunningValue;
    private int lastEmotionState;
    
    // EmotionIcon相关组件引用
    private Transform emotionIconTransform;
    private SpriteRenderer emotionIconSpriteRenderer;
    private Animator emotionIconAnimator;
    
    public bool OnMove
    {
        get => onMove;
        set
        {
            onMove = value;
            UpdateAnimator();
        }
    }
    
    public bool OnRunning
    {
        get => onRunning;
        set
        {
            onRunning = value;
            UpdateAnimator();
        }
    }
    
    public int EmotionState
    {
        get => emotionState;
        set
        {
            emotionState = Mathf.Clamp(value, 0, emotionSprites.Count);
            UpdateEmotion();
        }
    }
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // 初始化物理组件
        lastOnMoveValue = onMove;
        lastOnRunningValue = onRunning;
        lastEmotionState = emotionState;
        
        // 初始化EmotionIcon相关组件
        InitializeEmotionIcon();
        
        UpdateAnimator();
        UpdateEmotion();
    }
    
    private void Update()
    {
        // 检查onMove值是否在运行时被改变
        if (onMove != lastOnMoveValue)
        {
            UpdateAnimator();
            lastOnMoveValue = onMove;
        }
        
        // 检查onRunning值是否在运行时被改变
        if (onRunning != lastOnRunningValue)
        {
            UpdateAnimator();
            lastOnRunningValue = onRunning;
        }
        
        // 检查emotionState值是否在运行时被改变
        if (emotionState != lastEmotionState)
        {
            UpdateEmotion();
            lastEmotionState = emotionState;
        }
    }
    
    private void OnValidate()
    {
        UpdateAnimator();
        
        // 在Editor中安全地更新表情
        if (Application.isPlaying == false)
        {
            // 重新初始化组件引用
            InitializeEmotionIcon();
            
            // 只有在组件都有效时才更新表情
            if (emotionIconSpriteRenderer != null && emotionIconAnimator != null && 
                emotionIconAnimator.runtimeAnimatorController != null)
            {
                UpdateEmotion();
            }
        }
        else
        {
            UpdateEmotion();
        }
        
    }
    
    private void UpdateChasing()
    {
        if (isChasing)
        {
            StartChasePlayer();
        }
        else
        {
            StopChase();
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("OnMove", onMove);
            animator.SetBool("Running", onRunning);
        }
    }
    
    private void InitializeEmotionIcon()
    {
        // 查找EmotionIcon子对象
        emotionIconTransform = transform.Find("BaseEmotion/EmotionIcon");
        
        if (emotionIconTransform != null)
        {
            emotionIconSpriteRenderer = emotionIconTransform.GetComponent<SpriteRenderer>();
            emotionIconAnimator = emotionIconTransform.GetComponent<Animator>();
            
            if (emotionIconSpriteRenderer == null)
            {
                Debug.LogWarning($"BaseCharacter: EmotionIcon上未找到SpriteRenderer组件");
            }
            if (emotionIconAnimator == null)
            {
                Debug.LogWarning($"BaseCharacter: EmotionIcon上未找到Animator组件");
            }
            else if (emotionIconAnimator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"BaseCharacter: EmotionIcon的Animator没有分配AnimatorController");
            }
        }
        else
        {
            Debug.LogWarning($"BaseCharacter: 未找到EmotionIcon子对象，路径: BaseEmotion/EmotionIcon");
        }
    }
    
    private void UpdateEmotion()
    {
        // 检查基本组件是否存在
        if (emotionIconSpriteRenderer == null || emotionIconAnimator == null)
        {
            return;
        }
        
        // 检查Animator是否有Controller
        if (emotionIconAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("BaseCharacter: EmotionIcon的Animator没有分配AnimatorController");
            return;
        }
        
        // 检查Animator是否启用
        if (!emotionIconAnimator.enabled)
        {
            Debug.LogWarning("BaseCharacter: EmotionIcon的Animator未启用");
            return;
        }
        
        // 只在运行时播放动画，Editor模式下只更新Sprite
        if (Application.isPlaying)
        {
            if (emotionState == 0)
            {
                // 只有当之前的状态不是0时，才播放EmotionQuit动画
                if (lastEmotionState != 0)
                {
                    emotionIconAnimator.SetTrigger("EmotionQuit");
                }
            }
            else if (emotionState >= 1 && emotionState <= emotionSprites.Count)
            {
                // 数字1到列表长度时，更新表情图片并播放EmotionPop动画
                int spriteIndex = emotionState - 1;
                if (spriteIndex < emotionSprites.Count && emotionSprites[spriteIndex] != null)
                {
                    emotionIconSpriteRenderer.sprite = emotionSprites[spriteIndex];
                    emotionIconAnimator.SetTrigger("EmotionPop");
                }
                else
                {
                    Debug.LogWarning($"BaseCharacter: 表情索引 {spriteIndex} 对应的Sprite为空或超出范围");
                }
            }
        }
        else
        {
            // Editor模式下只更新Sprite，不播放动画
            if (emotionState >= 1 && emotionState <= emotionSprites.Count)
            {
                int spriteIndex = emotionState - 1;
                if (spriteIndex < emotionSprites.Count && emotionSprites[spriteIndex] != null)
                {
                    emotionIconSpriteRenderer.sprite = emotionSprites[spriteIndex];
                }
            }
        }
    }
    
    #region 追逐功能
    
    /// <summary>
    /// 开始追逐目标
    /// </summary>
    /// <param name="target">目标Transform</param>
    public void StartChasing(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[BaseCharacter] {gameObject.name}: 目标是null");
            return;
        }
        
        isChasing = true;
        OnRunning = true;
        StartCoroutine(ChaseCoroutine(target));
    }
    
    /// <summary>
    /// 停止追逐
    /// </summary>
    public void StopChasing()
    {
        isChasing = false;
        OnRunning = false;
        
        // 如果使用物理移动，停止水平速度
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        StopAllCoroutines();
    }
    
    /// <summary>
    /// 追逐协程
    /// </summary>
    private System.Collections.IEnumerator ChaseCoroutine(Transform target)
    {
        while (isChasing && target != null)
        {
            // 计算到目标的距离和方向
            Vector3 direction = (target.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, target.position);
            
            // 如果距离足够近，停止追逐
            if (distance <= chaseStopDistance)
            {
                Debug.Log($"[BaseCharacter] {gameObject.name} 到达目标，停止追逐");
                break;
            }
            
            // 根据距离选择不同的速度
            float currentSpeed = distance > chaseSpeedThreshold ? farChaseSpeed : closeChaseSpeed;
            
            // 翻转朝向目标（可选）
            if (flipTowardsTarget)
            {
                bool shouldFaceLeft = target.position.x < transform.position.x;
                FlipSprite(shouldFaceLeft);
            }
            
            // 使用物理移动或直接移动
            if (rb != null)
            {
                // 使用物理移动
                rb.velocity = new Vector2(direction.x * currentSpeed, rb.velocity.y);
            }
            else
            {
                // 直接移动
                transform.position += currentSpeed * Time.deltaTime * direction;
            }
            
            yield return null;
        }
        
        // 追逐结束后停止
        StopChasing();
    }
    
    /// <summary>
    /// 翻转精灵朝向
    /// </summary>
    private void FlipSprite(bool faceLeft)
    {
        Vector3 scale = transform.localScale;
        if (faceLeft)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        transform.localScale = scale;
    }
    
    #endregion
    
    #region Yarn Spinner Commands
    
    /// <summary>
    /// 设置角色表情状态
    /// 用法: <<set_emotion 1>> (设置表情为第1个)
    /// 用法: <<set_emotion 0>> (隐藏表情)
    /// </summary>
    /// <param name="emotionIndex">表情索引，0表示隐藏表情，1-N表示对应的表情</param>
    [YarnCommand("set_emotion")]
    public void SetEmotionCommand(int emotionIndex)
    {
        if (emotionIndex < 0)
        {
            Debug.LogWarning($"BaseCharacter: 表情索引不能为负数: {emotionIndex}");
            return;
        }
        
        if (emotionIndex > emotionSprites.Count)
        {
            Debug.LogWarning($"BaseCharacter: 表情索引超出范围: {emotionIndex} (最大: {emotionSprites.Count})");
            return;
        }
        
        EmotionState = emotionIndex;
        Debug.Log($"BaseCharacter: 设置表情为索引 {emotionIndex}");
    }
    
    /// <summary>
    /// 随机设置表情
    /// 用法: <<set_random_emotion>>
    /// </summary>
    [YarnCommand("set_random_emotion")]
    public void SetRandomEmotionCommand()
    {
        if (emotionSprites.Count == 0)
        {
            Debug.LogWarning("BaseCharacter: 没有可用的表情");
            return;
        }
        
        int randomIndex = Random.Range(1, emotionSprites.Count + 1);
        EmotionState = randomIndex;
        Debug.Log($"BaseCharacter: 随机设置表情为索引 {randomIndex}");
    }
    
    /// <summary>
    /// 清除表情（设置为0）
    /// 用法: <<clear_emotion>>
    /// </summary>
    [YarnCommand("clear_emotion")]
    public void ClearEmotionCommand()
    {
        EmotionState = 0;
        Debug.Log("BaseCharacter: 清除表情");
    }
    
    /// <summary>
    /// 启动跑步动画
    /// 用法: <<OnRunning>>
    /// </summary>
    [YarnCommand("OnRunning")]
    public void OnRunningCommand()
    {
        OnRunning = true;
        Debug.Log($"[BaseCharacter] OnRunning: {gameObject.name}");
    }
    
    /// <summary>
    /// 停止跑步动画
    /// 用法: <<OnStopRunning>>
    /// </summary>
    [YarnCommand("OnStopRunning")]
    public void OnStopRunningCommand()
    {
        OnRunning = false;
        Debug.Log($"[BaseCharacter] OnStopRunning: {gameObject.name}");
    }
    
    /// <summary>
    /// Yarn命令：开始追逐玩家
    /// 用法: <<start_chase_player>>
    /// </summary>
    [YarnCommand("start_chase_player")]
    public void StartChasePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[BaseCharacter] 找不到玩家对象");
            return;
        }
        
        StartChasing(player.transform);
        Debug.Log($"[BaseCharacter] {gameObject.name} 开始追逐玩家");
    }
    
    /// <summary>
    /// Yarn命令：停止追逐
    /// 用法: <<stop_chase>>
    /// </summary>
    [YarnCommand("stop_chase")]
    public void StopChase()
    {
        StopChasing();
        Debug.Log($"[BaseCharacter] {gameObject.name} 停止追逐");
    }
    
    #endregion
    
    #region Yarn Spinner Functions
    
    /// <summary>
    /// 获取当前表情状态
    /// 用法: <<if get_emotion() == 1>>
    /// </summary>
    /// <returns>当前表情索引</returns>
    [YarnFunction("get_emotion")]
    public static int GetEmotionCommand()
    {
        // 查找当前场景中的BaseCharacter实例
        BaseCharacter character = FindActiveBaseCharacter();
        if (character == null)
        {
            Debug.LogWarning("BaseCharacter: 找不到活动的BaseCharacter实例");
            return 0;
        }
        
        return character.EmotionState;
    }
    
    /// <summary>
    /// 检查是否有指定表情
    /// 用法: <<if has_emotion(1) == true>>
    /// </summary>
    /// <param name="emotionIndex">要检查的表情索引</param>
    /// <returns>是否存在该表情</returns>
    [YarnFunction("has_emotion")]
    public static bool HasEmotionCommand(int emotionIndex)
    {
        BaseCharacter character = FindActiveBaseCharacter();
        if (character == null)
        {
            Debug.LogWarning("BaseCharacter: 找不到活动的BaseCharacter实例");
            return false;
        }
        
        if (emotionIndex <= 0 || emotionIndex > character.emotionSprites.Count)
        {
            return false;
        }
        
        int spriteIndex = emotionIndex - 1;
        return spriteIndex < character.emotionSprites.Count && character.emotionSprites[spriteIndex] != null;
    }
    
    /// <summary>
    /// 获取表情总数
    /// 用法: <<if get_emotion_count() > 0>>
    /// </summary>
    /// <returns>表情总数</returns>
    [YarnFunction("get_emotion_count")]
    public static int GetEmotionCountCommand()
    {
        BaseCharacter character = FindActiveBaseCharacter();
        if (character == null)
        {
            Debug.LogWarning("BaseCharacter: 找不到活动的BaseCharacter实例");
            return 0;
        }
        
        return character.emotionSprites.Count;
    }
    
    /// <summary>
    /// 查找当前场景中活动的BaseCharacter实例
    /// </summary>
    /// <returns>找到的BaseCharacter实例，如果未找到则返回null</returns>
    private static BaseCharacter FindActiveBaseCharacter()
    {
        BaseCharacter[] characters = FindObjectsOfType<BaseCharacter>();
        if (characters.Length == 0)
        {
            return null;
        }
        
        // 如果有多个BaseCharacter，返回第一个
        if (characters.Length > 1)
        {
            Debug.LogWarning($"BaseCharacter: 找到多个BaseCharacter实例，使用第一个: {characters[0].name}");
        }
        
        return characters[0];
    }
    
    #endregion
}
