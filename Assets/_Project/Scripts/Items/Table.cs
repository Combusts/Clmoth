using UnityEngine;

public class Table : IInteractive
{
    [SerializeField] private string dialogueNode;

    public override void Interact()
    {
        Debug.Log("Interact table");
        CanInteract = false; // 交互后禁用再次交互
        // HideHint(); // 移除多余的HideHint调用，Player.Interact()已经调用了
        YarnSpinnerManager.Instance.StartDialogue(dialogueNode);
    }
}
