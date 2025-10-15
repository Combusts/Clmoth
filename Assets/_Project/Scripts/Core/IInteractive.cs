
using UnityEngine;

public abstract class IInteractive : MonoBehaviour
{
    [SerializeField] private bool isShowHint;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private GameObject hintItemPrefab; // 改为Prefab引用
    [SerializeField] private string hintText;
    [SerializeField] private Vector3 hintOffset = new(0, 1, 0); // 提示位置偏移
    
    private GameObject currentHintItem; // 当前显示的提示实例

    public bool IsShowHint 
    { 
        get => isShowHint; 
        set => isShowHint = value; 
    }
    
    public bool CanInteract 
    { 
        get => canInteract; 
        set => canInteract = value; 
    }

    public string HintText
    {
        get => hintText;
        set => hintText = value;
    }

    // 修改变量自动重载
    private void OnValidate()
    {
        // OnValidate期间不能调用SendMessage，所以只更新文本和位置
        if (currentHintItem != null)
        {
            var textComponent = currentHintItem.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = hintText;
            }
            currentHintItem.transform.localPosition = hintOffset;
        }
    }

    public abstract void Interact();

    public virtual void ShowHint()
    {
        if (IsShowHint || !canInteract) return;
        
        IsShowHint = true;
        
        // 如果还没有创建提示实例，则创建
        if (currentHintItem == null && hintItemPrefab != null)
        {
            currentHintItem = Instantiate(hintItemPrefab, transform);
            currentHintItem.transform.localPosition = hintOffset;
            
            // 设置文本内容
            var textComponent = currentHintItem.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = hintText;
            }
        }
        
        if (currentHintItem != null)
        {
            currentHintItem.SetActive(true);
        }
        
        // 获取渲染组件并将颜色设置为灰色
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.gray;
        }
    }

    public virtual void HideHint()
    {
        if (!IsShowHint) return;
        
        IsShowHint = false;
        
        if (currentHintItem != null)
        {
            currentHintItem.SetActive(false);
        }
        
        // 获取渲染组件并将颜色设为源色
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.white;
        }
    }
    
    // 清理提示实例
    private void OnDestroy()
    {
        if (currentHintItem != null)
        {
            Destroy(currentHintItem);
        }
    }
}
