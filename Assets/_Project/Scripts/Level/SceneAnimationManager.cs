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
        [Tooltip("同一组内的动画互斥，恢复时只应用优先级最高的一条")]
        public string groupKey = "";
        [Tooltip("数值越大优先级越高（仅在同组内生效）")]
        public int priority = 0;
    }
    
    [Header("场景动画配置")]
    public List<SceneAnimation> animations = new List<SceneAnimation>();
    
    private Dictionary<string, AnimationClip> animationDict;
    private Dictionary<string, AnimationState> activeAnimations;
    private Animation animationComponent;
    
    [Header("存档集成")]
    [SerializeField] private bool enableSaveIntegration = true;
    [SerializeField] private bool disableAnimatorsDuringRestore = true;
    
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
        
        // 确保动画在任何情况下都进行评估（包括不可见/裁剪时）
        animationComponent.cullingType = AnimationCullingType.AlwaysAnimate;

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
        
        // 恢复存档的动画状态（Awake 时可能过早，这里仍保持一次以兼容旧行为）
        if (enableSaveIntegration)
        {
            RestoreAnimationStates();
        }

        // 订阅存档加载事件，在数据加载完成与布局稳定后再次恢复，确保最终可见状态正确
        SaveManager.OnSaveDataLoaded += OnSaveDataLoaded;
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
        
        // 先预计算每个动画目标的期望归一化时间（1代表末帧）
        var desiredTimes = new Dictionary<string, float>();
        var chosenByGroup = new Dictionary<string, SceneAnimation>();

        foreach (var anim in animations)
        {
            if (anim.clip == null) continue;
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isCompleted = SaveManager.Instance.IsSceneAnimationCompleted(anim.name, currentScene);

            float? normalized = null;
            if (isCompleted && anim.keepLastFrame)
            {
                normalized = 1f;
            }
            else
            {
                float progress = SaveManager.Instance.GetSceneAnimationState(anim.name, currentScene);
                if (progress > 0f)
                {
                    normalized = Mathf.Clamp01(progress);
                }
            }

            if (normalized.HasValue)
            {
                string key = string.IsNullOrEmpty(anim.groupKey) ? anim.name : anim.groupKey;
                // 在同组内选择优先级最高者；优先级相等时选择normalized更大的；再相等取列表后者
                if (!chosenByGroup.ContainsKey(key))
                {
                    chosenByGroup[key] = anim;
                    desiredTimes[key] = normalized.Value;
                }
                else
                {
                    var current = chosenByGroup[key];
                    if (anim.priority > current.priority ||
                        (anim.priority == current.priority && normalized.Value > desiredTimes[key]))
                    {
                        chosenByGroup[key] = anim;
                        desiredTimes[key] = normalized.Value;
                    }
                }
            }
        }

        // 应用被选中的动画采样（避免同组内多动画互相覆盖）
        foreach (var kv in chosenByGroup)
        {
            var anim = kv.Value;
            float normalized = desiredTimes[kv.Key];
            Debug.Log($"SceneAnimationManager: 处理动画 '{anim.name}'（组: '{(string.IsNullOrEmpty(anim.groupKey)?"<none>":anim.groupKey)}'，优先级: {anim.priority}）");

            var state = animationComponent[anim.name];
            if (state == null) { Debug.LogError($"SceneAnimationManager: 无法找到动画状态 '{anim.name}'"); continue; }

            // 使用Clip采样到目标进度
            ApplyClipAtNormalizedTime(anim.name, normalized);
            // 冻结Legacy Animation状态
            state.time = state.length * normalized;
            state.speed = 0f;
            state.enabled = true;
            state.weight = anim.weight;
            animationComponent.Sample();
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
        
        // 在播放前重置之前可能保留末帧的状态，避免阻塞
        var stateToReset = animationComponent[animationName];
        if (stateToReset != null)
        {
            stateToReset.speed = 1f;
            stateToReset.enabled = true;
            stateToReset.time = 0f;
        }
        
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
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveManager.Instance.AddCompletedSceneAnimation(animationName, currentScene);
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
        
        // 在播放前重置之前可能保留末帧的状态，避免阻塞
        var stateToReset = animationComponent[animationName];
        if (stateToReset != null)
        {
            stateToReset.speed = 1f;
            stateToReset.enabled = true;
            stateToReset.time = 0f;
        }

        // 播放动画
        animationComponent.Play(animationName);
        
        // 保存动画状态
        if (enableSaveIntegration && SaveManager.Instance != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveManager.Instance.SetSceneAnimationState(animationName, 1f, currentScene); // 标记为完成
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
            state.speed = 0f;
            state.enabled = true; // 先启用并采样末帧
            state.weight = 1f; // 确保权重正确
            animationComponent.Sample();
            // 同时对Clip进行一次末帧采样，确保写入到Transform
            ApplyClipAtNormalizedTime(animationName, 1f);

            // 关键：禁用该状态并清权重，避免下次播放被认为仍在播放/被使能
            state.enabled = false;
            state.weight = 0f;
            animationComponent.Sample();
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
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return SaveManager.Instance.IsSceneAnimationCompleted(animationName, currentScene);
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
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveManager.Instance.AddCompletedSceneAnimation(animationName, currentScene);
            Debug.Log($"SceneAnimationManager: Manually marked animation '{animationName}' in scene '{currentScene}' as completed");
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

    private void OnDestroy()
    {
        SaveManager.OnSaveDataLoaded -= OnSaveDataLoaded;
    }

    private void OnSaveDataLoaded(GameSaveData data)
    {
        // 在数据加载完成后，等待布局稳定再恢复，避免被后续布局或其他动画覆盖
        StartCoroutine(RestoreAfterCanvasReady());
    }

    private IEnumerator RestoreAfterCanvasReady()
    {
        // 等一帧让对象与UI实例化完成
        yield return null;
        // 强制刷新Canvas，确保布局树稳定
        Canvas.ForceUpdateCanvases();
        yield return null;
        // 等待到帧末，避免LateUpdate/Canvas系统覆盖
        yield return new WaitForEndOfFrame();
        
        List<Animator> disabledAnimators = null;
        if (disableAnimatorsDuringRestore)
        {
            disabledAnimators = new List<Animator>();
            var animators = GetComponentsInChildren<Animator>(true);
            foreach (var a in animators)
            {
                if (a != null && a.enabled)
                {
                    a.enabled = false;
                    disabledAnimators.Add(a);
                }
            }
        }
        
        if (enableSaveIntegration)
        {
            // 停止正在播放，避免混合影响采样
            animationComponent.Stop();
            RestoreAnimationStates();
            // 再次在帧末采样一次，确保视觉稳定
            yield return new WaitForEndOfFrame();
            animationComponent.Sample();
        }
        
        if (disabledAnimators != null)
        {
            // 下一帧再恢复Animator，避免立刻覆盖
            yield return null;
            foreach (var a in disabledAnimators)
            {
                if (a != null)
                {
                    a.enabled = true;
                }
            }
        }
    }

    private void ApplyClipAtNormalizedTime(string animationName, float normalizedTime)
    {
        if (!animationDict.ContainsKey(animationName)) return;
        var clip = animationDict[animationName];
        if (clip == null) return;
        var t = Mathf.Clamp01(normalizedTime) * clip.length;
        // 使用Clip的采样直接写入对象的最终属性（包括RectTransform）
        clip.SampleAnimation(gameObject, t);
    }
}
