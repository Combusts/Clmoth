using UnityEngine;

public class Table : IInteractive
{
    [SerializeField] private string dialogueNode;

    public override void Interact()
    {
        Debug.Log("Interact table");
        
        // 启动对话
        if (!string.IsNullOrEmpty(dialogueNode))
        {
            YarnSpinnerManager.Instance.StartDialogue(dialogueNode);
        }
        else
        {
            Debug.LogWarning("Table has no dialogue node assigned!");
        }
    }
    
    protected override void Start()
    {
        
        // 调用基类的存档状态检查
        base.Start();
    }
}
