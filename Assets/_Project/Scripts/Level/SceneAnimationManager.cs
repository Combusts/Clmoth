using UnityEngine;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

public class SceneAnimationManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneAnimation
    {
        [Header("动画配置")]
        public string name;
        public AnimationClip clip;
        public bool keepLastFrame = true; // 是否保留最后一帧
        public float weight = 1f; // 动画权重
    }
    
    [Header("场景动画配置")]
    public List<SceneAnimation> animations = new List<SceneAnimation>();
    
    private Dictionary<string, AnimationClip> animationDict;
    private Dictionary<string, AnimationState> activeAnimations;
    private Animation animationComponent;
    
    [Header("存档集成")]
    [SerializeField] private bool enableSaveIntegration = true;
    
    private void Awake()
    {
        Debug.Log($"SceneAnimationManager: Awake() 开始初始化");
        
        animationComponent = GetComponent<Animation>();
        if (animationComponent == null)
        {
            Debug.LogWarning($"SceneAnimationManager: 未找到Animation组件，将自动添加");
            animationComponent = gameObject.AddComponent<Animation>();
        }
        else
        {
            Debug.Log($"SceneAnimationManager: 找到现有的Animation组件");
        }
        
        // 初始化动画字典
        animationDict = new Dictionary<string, AnimationClip>();
        activeAnimations = new Dictionary<string, AnimationState>();
        
        Debug.Log($"SceneAnimationManager: 动画字典初始化完成");
        
        // 配置动画
        SetupAnimations();
        
        // 如果没有配置任何动画，输出警告
        if (animationDict.Count == 0)
        {
            Debug.LogWarning($"SceneAnimationManager: 没有配置任何动画！请在Inspector中配置动画列表。");
            Debug.LogWarning($"SceneAnimationManager: 需要配置的动画: Manager_Exit, DocumentPop, DocumentExit");
        }
        else
        {
            Debug.Log($"SceneAnimationManager: Awake() 完成，成功配置了 {animationDict.Count} 个动画");
        }
    }
    
    private void SetupAnimations()
    {
        // 确保Animation组件已启用
        if (!animationComponent.enabled)
        {
            animationComponent.enabled = true;
        }
        
        foreach (var anim in animations)
        {
            if (anim.clip != null)
            {
                animationDict[anim.name] = anim.clip;
                
                // 添加clip到Animation组件
                animationComponent.AddClip(anim.clip, anim.name);
                
                // 验证clip是否成功添加
                if (animationComponent.GetClip(anim.name) != null)
                {
                    Debug.Log($"SceneAnimationManager: 成功添加动画 '{anim.name}'");
                    
                    // 设置动画状态
                    var state = animationComponent[anim.name];
                    if (state != null)
                    {
                        state.weight = anim.weight;
                        state.wrapMode = WrapMode.Once; // 播放一次
                    }
                }
                else
                {
                    Debug.LogError($"SceneAnimationManager: 无法添加动画 '{anim.name}' 到Animation组件");
                }
            }
            else
            {
                Debug.LogWarning($"SceneAnimationManager: 动画 '{anim.name}' 的clip为空");
            }
        }
        
        Debug.Log($"SceneAnimationManager: 动画配置完成，共配置 {animationDict.Count} 个动画");
        
        // 恢复存档的动画状态
        if (enableSaveIntegration)
        {
            RestoreAnimationStates();
        }
    }
    
    /// <summary>
    /// 恢复存档的动画状态
    /// </summary>
    private void RestoreAnimationStates()
    {
        Debug.Log($"SceneAnimationManager: 开始恢复动画状态");
        
        if (SaveManager.Instance == null) 
        {
            Debug.LogError($"SceneAnimationManager: SaveManager.Instance 为空，无法恢复动画状态");
            return;
        }
        
        Debug.Log($"SceneAnimationManager: SaveManager实例存在，开始检查动画");
        Debug.Log($"SceneAnimationManager: 配置的动画数量: {animations.Count}");
        
        foreach (var anim in animations)
        {
            Debug.Log($"SceneAnimationManager: 处理动画 '{anim.name}'");
            
            if (anim.clip != null)
            {
                Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 的clip存在: {anim.clip.name}");
                
                // 检查动画是否已完成
                bool isCompleted = SaveManager.Instance.IsSceneAnimationCompleted(anim.name);
                Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 完成状态: {isCompleted}");
                
                if (isCompleted)
                {
                    Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 已完成，检查是否需要保留最后一帧");
                    
                    // 如果动画已完成且设置了保留最后一帧，直接跳到最后一帧
                    if (anim.keepLastFrame)
                    {
                        Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 需要保留最后一帧");
                        
                        var state = animationComponent[anim.name];
                        if (state != null)
                        {
                            Debug.Log($"SceneAnimationManager: 找到动画状态 '{anim.name}', 长度: {state.length}");
                            state.time = state.length;
                            state.enabled = true; // 保持启用状态以显示最后一帧
                            state.weight = anim.weight; // 确保权重正确
                            Debug.Log($"SceneAnimationManager: 成功恢复动画 '{anim.name}' 到最后一帧");
                        }
                        else
                        {
                            Debug.LogError($"SceneAnimationManager: 无法找到动画状态 '{anim.name}'");
                        }
                    }
                    else
                    {
                        Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 已完成但不需要保留最后一帧");
                    }
                }
                else
                {
                    Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 未完成，检查播放进度");
                    
                    // 检查是否有部分播放进度
                    float progress = SaveManager.Instance.GetSceneAnimationState(anim.name);
                    Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 播放进度: {progress}");
                    
                    if (progress > 0f)
                    {
                        var state = animationComponent[anim.name];
                        if (state != null)
                        {
                            float targetTime = state.length * progress;
                            Debug.Log($"SceneAnimationManager: 设置动画 '{anim.name}' 到时间 {targetTime} (总长度: {state.length})");
                            state.time = targetTime;
                            Debug.Log($"SceneAnimationManager: 成功恢复动画 '{anim.name}' 到进度 {progress}");
                        }
                        else
                        {
                            Debug.LogError($"SceneAnimationManager: 无法找到动画状态 '{anim.name}' 来恢复进度");
                        }
                    }
                    else
                    {
                        Debug.Log($"SceneAnimationManager: 动画 '{anim.name}' 没有播放进度，保持初始状态");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"SceneAnimationManager: 动画 '{anim.name}' 的clip为空，跳过恢复");
            }
        }
        
        Debug.Log($"SceneAnimationManager: 动画状态恢复完成");
    }
    
    #region Yarn Commands - 保持与AnimationCommands兼容的命令名称
    
    /// <summary>
    /// 播放动画并等待完成，保持在最后一帧
    /// 用法: <<animator_play_and_wait MainScene "Manager_Exit">>
    /// </summary>
    [YarnCommand("animator_play_and_wait")]
    public IEnumerator AnimatorPlayAndWait(string animationName)
    {
        Debug.Log($"SceneAnimationManager: 尝试播放动画 '{animationName}'");
        
        if (!animationDict.ContainsKey(animationName))
        {
            Debug.LogError($"SceneAnimationManager: 动画 '{animationName}' 不存在！");
            Debug.LogError($"SceneAnimationManager: 可用的动画: {string.Join(", ", animationDict.Keys)}");
            yield break;
        }
        
        var anim = animations.Find(a => a.name == animationName);
        if (anim == null) 
        {
            Debug.LogError($"SceneAnimationManager: 无法找到动画配置 '{animationName}'");
            yield break;
        }
        
        Debug.Log($"SceneAnimationManager: 开始播放动画 '{animationName}' (clip: {anim.clip.name})");
        
        // 播放动画
        animationComponent.Play(animationName);
        
        // 等待动画播放完成
        while (animationComponent.IsPlaying(animationName))
        {
            yield return null;
        }
        
        Debug.Log($"SceneAnimationManager: 动画 '{animationName}' 播放完成");
        
        // 保存动画完成状态
        if (enableSaveIntegration && SaveManager.Instance != null)
        {
            SaveManager.Instance.AddCompletedSceneAnimation(animationName);
        }
        
        // 如果设置了保留最后一帧，保持在最后一帧
        if (anim.keepLastFrame)
        {
            yield return StartCoroutine(KeepLastFrame(animationName));
        }
    }
    
    /// <summary>
    /// 播放动画并保持在最后一帧
    /// 用法: <<animator_play_and_stay MainScene "DocumentPop">>
    /// </summary>
    [YarnCommand("animator_play_and_stay")]
    public void AnimatorPlayAndStay(string animationName)
    {
        if (!animationDict.ContainsKey(animationName))
        {
            Debug.LogWarning($"SceneAnimationManager: 动画 '{animationName}' 不存在");
            return;
        }
        
        var anim = animations.Find(a => a.name == animationName);
        if (anim == null) return;
        
        // 播放动画
        animationComponent.Play(animationName);
        
        // 保存动画状态
        if (enableSaveIntegration && SaveManager.Instance != null)
        {
            SaveManager.Instance.SetSceneAnimationState(animationName, 1f); // 标记为完成
        }
        
        // 如果设置了保留最后一帧，等待完成后保持在最后一帧
        if (anim.keepLastFrame)
        {
            StartCoroutine(KeepLastFrame(animationName));
        }
    }
    
    /// <summary>
    /// 触发动画（兼容性方法，实际调用play_and_stay）
    /// 用法: <<animator_trigger MainScene "Manager_Exit">>
    /// </summary>
    [YarnCommand("animator_trigger")]
    public void AnimatorTrigger(string animationName)
    {
        // 为了兼容性，trigger命令等同于play_and_stay
        AnimatorPlayAndStay(animationName);
    }
    
    #endregion
    
    #region 额外的动画控制方法
    
    /// <summary>
    /// 停止特定动画
    /// </summary>
    [YarnCommand("stop_scene_animation")]
    public void StopSceneAnimation(string animationName)
    {
        if (animationComponent.IsPlaying(animationName))
        {
            animationComponent.Stop(animationName);
        }
    }
    
    /// <summary>
    /// 停止所有动画
    /// </summary>
    [YarnCommand("stop_all_scene_animations")]
    public void StopAllSceneAnimations()
    {
        animationComponent.Stop();
    }
    
    /// <summary>
    /// 设置动画播放速度
    /// </summary>
    [YarnCommand("set_animation_speed")]
    public void SetAnimationSpeed(string animationName, float speed)
    {
        if (animationDict.ContainsKey(animationName))
        {
            var state = animationComponent[animationName];
            if (state != null)
            {
                state.speed = speed;
            }
        }
    }
    
    #endregion
    
    #region 私有方法
    
    private IEnumerator KeepLastFrame(string animationName)
    {
        // 等待动画播放完成
        while (animationComponent.IsPlaying(animationName))
        {
            yield return null;
        }
        
        // 保持在最后一帧
        var state = animationComponent[animationName];
        if (state != null)
        {
            state.time = state.length;
            state.enabled = true; // 保持启用状态以显示最后一帧
            state.weight = 1f; // 确保权重正确
        }
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 检查动画是否正在播放
    /// </summary>
    public bool IsAnimationPlaying(string animationName)
    {
        return animationComponent.IsPlaying(animationName);
    }
    
    /// <summary>
    /// 获取动画播放进度
    /// </summary>
    public float GetAnimationProgress(string animationName)
    {
        if (animationComponent.IsPlaying(animationName))
        {
            var state = animationComponent[animationName];
            return state.time / state.length;
        }
        return 0f;
    }
    
    /// <summary>
    /// 获取所有可用的动画名称
    /// </summary>
    public string[] GetAvailableAnimations()
    {
        return new List<string>(animationDict.Keys).ToArray();
    }
    
    /// <summary>
    /// 检查动画是否已完成（从存档）
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns>是否已完成</returns>
    public bool IsAnimationCompleted(string animationName)
    {
        if (enableSaveIntegration && SaveManager.Instance != null)
        {
            return SaveManager.Instance.IsSceneAnimationCompleted(animationName);
        }
        return false;
    }
    
    /// <summary>
    /// 手动标记动画为已完成
    /// </summary>
    /// <param name="animationName">动画名称</param>
    public void MarkAnimationAsCompleted(string animationName)
    {
        if (enableSaveIntegration && SaveManager.Instance != null)
        {
            SaveManager.Instance.AddCompletedSceneAnimation(animationName);
            Debug.Log($"SceneAnimationManager: Manually marked animation '{animationName}' as completed");
        }
    }
    
    #endregion
    
    #region Editor辅助方法
    
    #if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中预览动画
    /// </summary>
    [ContextMenu("Preview All Animations")]
    public void PreviewAllAnimations()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("预览功能需要在运行时使用");
            return;
        }
        
        StartCoroutine(PreviewAnimationsCoroutine());
    }
    
    private IEnumerator PreviewAnimationsCoroutine()
    {
        foreach (var anim in animations)
        {
            if (anim.clip != null)
            {
                Debug.Log($"播放动画: {anim.name}");
                AnimatorPlayAndStay(anim.name);
                yield return new WaitForSeconds(anim.clip.length + 0.5f);
            }
        }
        Debug.Log("所有动画预览完成");
    }
    #endif
    
    #endregion
}
