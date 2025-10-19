using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class AnimationCommands : MonoBehaviour {
    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Animator component not found on {gameObject.name}");
        }
    }

    [YarnCommand("animator_trigger")]
    public void AnimatorTrigger(string triggerName) {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning($"Animator not found on {gameObject.name}");
        }
    }

    [YarnCommand("animator_play_and_stay")]
    public void AnimatorPlayAndStay(string animationName) {
        if (animator != null)
        {
            // 直接播放动画状态，不通过Trigger
            animator.Play(animationName);
            
            // 设置动画速度为0，让动画停留在最后一帧
            StartCoroutine(WaitForAnimationAndStop());
        }
        else
        {
            Debug.LogWarning($"Animator not found on {gameObject.name}");
        }
    }

    [YarnCommand("animator_play_and_wait")]
    public IEnumerator AnimatorPlayAndWait(string animationName) {
        if (animator != null)
        {
            // 直接播放动画状态
            animator.Play(animationName);
            
            // 等待动画播放完成后再继续对话，并保持在最后一帧
            yield return WaitForAnimationAndContinue();
        }
        else
        {
            Debug.LogWarning($"Animator not found on {gameObject.name}");
            yield break;
        }
    }

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
}