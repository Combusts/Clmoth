using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class AnimationCommands : MonoBehaviour {
    
    private Animator animator;
    private Animation animationComponent;
    private SceneAnimationManager sceneAnimationManager;

    private void Awake()
    {
        // 优先检测SceneAnimationManager（用于场景动画）
        sceneAnimationManager = GetComponent<SceneAnimationManager>();
        
        // 检测Animation组件（备用场景动画方案）
        animationComponent = GetComponent<Animation>();
        
        // 检测Animator组件（用于角色动画等）
        animator = GetComponent<Animator>();
        
        // 输出检测结果
        if (sceneAnimationManager != null)
        {
            Debug.Log($"AnimationCommands: 检测到SceneAnimationManager组件在 {gameObject.name}");
        }
        else if (animationComponent != null)
        {
            Debug.Log($"AnimationCommands: 检测到Animation组件在 {gameObject.name}");
        }
        else if (animator != null)
        {
            Debug.Log($"AnimationCommands: 检测到Animator组件在 {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"AnimationCommands: 未找到任何动画组件在 {gameObject.name}");
        }
    }

    [YarnCommand("animator_trigger")]
    public void AnimatorTrigger(string triggerName) {
        // 优先使用SceneAnimationManager
        if (sceneAnimationManager != null)
        {
            sceneAnimationManager.AnimatorTrigger(triggerName);
            return;
        }
        
        // 回退到Animation组件
        if (animationComponent != null)
        {
            if (animationComponent.GetClip(triggerName) != null)
            {
                animationComponent.Play(triggerName);
                return;
            }
        }
        
        // 最后回退到Animator
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning($"AnimationCommands: 未找到可用的动画组件在 {gameObject.name}");
        }
    }

    [YarnCommand("animator_play_and_stay")]
    public void AnimatorPlayAndStay(string animationName) {
        // 优先使用SceneAnimationManager
        if (sceneAnimationManager != null)
        {
            sceneAnimationManager.AnimatorPlayAndStay(animationName);
            return;
        }
        
        // 回退到Animation组件
        if (animationComponent != null)
        {
            if (animationComponent.GetClip(animationName) != null)
            {
                animationComponent.Play(animationName);
                StartCoroutine(WaitForAnimationAndStop_Animation());
                return;
            }
        }
        
        // 最后回退到Animator
        if (animator != null)
        {
            // 直接播放动画状态，不通过Trigger
            animator.Play(animationName);
            
            // 设置动画速度为0，让动画停留在最后一帧
            StartCoroutine(WaitForAnimationAndStop());
        }
        else
        {
            Debug.LogWarning($"AnimationCommands: 未找到可用的动画组件在 {gameObject.name}");
        }
    }

    [YarnCommand("animator_play_and_wait")]
    public IEnumerator AnimatorPlayAndWait(string animationName) {
        // 优先使用SceneAnimationManager
        if (sceneAnimationManager != null)
        {
            yield return sceneAnimationManager.AnimatorPlayAndWait(animationName);
            yield break;
        }
        
        // 回退到Animation组件
        if (animationComponent != null)
        {
            if (animationComponent.GetClip(animationName) != null)
            {
                animationComponent.Play(animationName);
                yield return WaitForAnimationAndContinue_Animation();
                yield break;
            }
        }
        
        // 最后回退到Animator
        if (animator != null)
        {
            // 直接播放动画状态
            animator.Play(animationName);
            
            // 等待动画播放完成后再继续对话，并保持在最后一帧
            yield return WaitForAnimationAndContinue();
        }
        else
        {
            Debug.LogWarning($"AnimationCommands: 未找到可用的动画组件在 {gameObject.name}");
            yield break;
        }
    }

    #region Animator相关协程
    
    private IEnumerator WaitForAnimationAndStop()
    {
        // 等待一帧确保动画开始播放
        yield return null;
        
        // 获取当前播放的动画状态信息
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 等待动画播放完成（使用normalizedTime来判断）
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        
        // 停止动画，保持在最后一帧
        animator.speed = 0f;
    }

    private IEnumerator WaitForAnimationAndContinue()
    {
        // 等待一帧确保动画开始播放
        yield return null;
        
        // 获取当前播放的动画状态信息
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 等待动画播放完成（使用normalizedTime来判断）
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        
        // 停止动画，保持在最后一帧
        animator.speed = 0f;
        
        // 动画播放完成，协程结束会自动继续Yarn对话
    }
    
    #endregion
    
    #region Animation组件相关协程
    
    private IEnumerator WaitForAnimationAndStop_Animation()
    {
        // 等待一帧确保动画开始播放
        yield return null;
        
        // 等待动画播放完成
        while (animationComponent.isPlaying)
        {
            yield return null;
        }
        
        // 保持在最后一帧
        var state = animationComponent.GetClip(animationComponent.clip.name);
        if (state != null)
        {
            animationComponent[state.name].time = animationComponent[state.name].length;
            animationComponent[state.name].enabled = false;
        }
    }
    
    private IEnumerator WaitForAnimationAndContinue_Animation()
    {
        // 等待一帧确保动画开始播放
        yield return null;
        
        // 等待动画播放完成
        while (animationComponent.isPlaying)
        {
            yield return null;
        }
        
        // 保持在最后一帧
        var state = animationComponent.GetClip(animationComponent.clip.name);
        if (state != null)
        {
            animationComponent[state.name].time = animationComponent[state.name].length;
            animationComponent[state.name].enabled = false;
        }
        
        // 动画播放完成，协程结束会自动继续Yarn对话
    }
    
    #endregion
}