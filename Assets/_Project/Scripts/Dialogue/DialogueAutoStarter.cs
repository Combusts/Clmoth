using UnityEngine;
using Yarn.Unity;

public class DialogueAutoStarter : MonoBehaviour
{
    [Header("对话设置")]
    [SerializeField] private string dialogueNodeName = "Start";
    [SerializeField] private bool enableAutoStart = true;
    
    [Header("存档检查")]
    [SerializeField] private bool checkSaveState = true;
    
    [Header("延迟设置")]
    [SerializeField] private float startDelay = 0.1f; // 延迟开始，确保存档系统已初始化
    
    void Start()
    {
        if (enableAutoStart)
        {
            // 延迟开始，确保存档系统已初始化
            Invoke(nameof(StartDialogueIfNeeded), startDelay);
        }
    }
    
    private void StartDialogueIfNeeded()
    {
        // 检查存档状态
        if (checkSaveState && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.IsDialogueCompleted(dialogueNodeName))
            {
                Debug.Log($"[DialogueAutoStarter] Dialogue '{dialogueNodeName}' already completed, skipping auto-start");
                return;
            }
        }
        
        // 开始对话
        if (YarnSpinnerManager.Instance != null)
        {
            Debug.Log($"[DialogueAutoStarter] Starting dialogue '{dialogueNodeName}'");
            YarnSpinnerManager.Instance.StartDialogue(dialogueNodeName);
        }
        else
        {
            Debug.LogWarning($"[DialogueAutoStarter] YarnSpinnerManager.Instance is null, cannot start dialogue");
        }
    }
    
    /// <summary>
    /// 手动触发对话（忽略存档状态）
    /// </summary>
    public void ForceStartDialogue()
    {
        if (YarnSpinnerManager.Instance != null)
        {
            Debug.Log($"[DialogueAutoStarter] Force starting dialogue '{dialogueNodeName}'");
            YarnSpinnerManager.Instance.StartDialogue(dialogueNodeName);
        }
    }
    
    /// <summary>
    /// 安全地开始对话（检查存档状态）
    /// </summary>
    public void StartDialogueSafe()
    {
        if (YarnSpinnerManager.Instance != null)
        {
            YarnSpinnerManager.Instance.StartDialogueSafe(dialogueNodeName, checkSaveState);
        }
    }
    
    /// <summary>
    /// 设置对话节点名称
    /// </summary>
    /// <param name="nodeName">节点名称</param>
    public void SetDialogueNodeName(string nodeName)
    {
        dialogueNodeName = nodeName;
    }
    
    /// <summary>
    /// 启用/禁用自动开始
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetAutoStartEnabled(bool enabled)
    {
        enableAutoStart = enabled;
    }
    
    /// <summary>
    /// 启用/禁用存档检查
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetSaveStateCheckEnabled(bool enabled)
    {
        checkSaveState = enabled;
    }
    
    // 编辑器调试方法
    [ContextMenu("Test Start Dialogue")]
    private void TestStartDialogue()
    {
        StartDialogueIfNeeded();
    }
    
    [ContextMenu("Test Force Start Dialogue")]
    private void TestForceStartDialogue()
    {
        ForceStartDialogue();
    }
}
