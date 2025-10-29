using UnityEngine;

/// <summary>
/// 石头生成触发器 - 当玩家进入触发区域时，触发 StoneGenerator 开始生成石头
/// </summary>
public class StoneTrigger : MonoBehaviour
{
    [Header("Stone Generator")]
    [SerializeField] private StoneGenerator stoneGenerator;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true; // 是否只触发一次
    
    private bool hasTriggered = false; // 是否已经触发过
    
    /// <summary>
    /// 当玩家进入触发区域时调用
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否是玩家
        if (collision.GetComponent<Player>() == null)
        {
            return;
        }
        
        // 如果设置为只触发一次且已经触发过，则直接返回
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        // 检查 StoneGenerator 是否已分配
        if (stoneGenerator == null)
        {
            Debug.LogError($"[StoneTrigger] StoneGenerator 未分配！请检查 {gameObject.name} 上的 StoneTrigger 组件。");
            return;
        }
        
        // 触发石头生成
        stoneGenerator.GenerateStones();
        hasTriggered = true;
        
        Debug.Log($"[StoneTrigger] 玩家进入触发区域，开始生成石头。");
        
        // 如果只触发一次，可以禁用碰撞器或整个对象
        if (triggerOnce)
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
    
    /// <summary>
    /// 重置触发状态（用于多次触发的情况）
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        GetComponent<Collider2D>().enabled = true;
    }
}
