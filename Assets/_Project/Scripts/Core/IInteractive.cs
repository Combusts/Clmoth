
using UnityEngine;

public abstract class IInteractive : MonoBehaviour
{
    [SerializeField] private bool isShowHint;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private GameObject hintItemPrefab; // 改为Prefab引用
    [SerializeField] private string hintText;
    [SerializeField] private Vector3 hintOffset = new(0, 1, 0); // 提示位置偏移
    [SerializeField] private Vector3 hintScale = Vector3.one; // 提示大小缩放

    [Header("存档系统")]
    [SerializeField] private string interactiveID = ""; // 唯一标识符
    [SerializeField] private string linkedDialogueNode = ""; // 关联的对话节点
    [SerializeField] private bool disableAfterDialogue = true; // 对话后是否禁用交互
    [SerializeField] private bool deactivateAfterDialogue = false; // 对话后是否隐藏物件

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

    public Vector3 HintScale
    {
        get => hintScale;
        set => hintScale = value;
    }

    void Awake()
    {
        // 确保isShowHint初始状态为false，避免第一次触碰不显示hint的问题
        isShowHint = false;

        // 如果没有设置ID，使用游戏物件名称作为ID
        if (string.IsNullOrEmpty(interactiveID))
        {
            interactiveID = gameObject.name;
        }
    }

    protected virtual void Start()
    {
        // 检查存档状态，恢复交互物件的状态
        CheckSaveState();
        currentHintItem = Instantiate(hintItemPrefab, transform);
        currentHintItem.transform.localPosition = hintOffset;
        currentHintItem.transform.localScale = hintScale;
        currentHintItem.name = "HintItem";
        var hintContentTransform = currentHintItem.transform.Find("Canvas/Hint/HintContent");
        if (hintContentTransform.TryGetComponent<TMPro.TextMeshProUGUI>(out var textComponent))
        {
            textComponent.text = hintText;
            Debug.Log($"[IInteractive] Set HintContent text for {gameObject.name}: {hintText}");
        }
        else
        {
            Debug.LogWarning($"[IInteractive] No HintContent component found in HintItem for {gameObject.name}");
        }
        currentHintItem.SetActive(false);
    }

    // 修改变量自动重载
    private void OnValidate()
    {
        // OnValidate期间不能调用SendMessage，所以只更新文本、位置和大小
        if (currentHintItem != null)
        {
            var hintContentTransform = currentHintItem.transform.Find("Canvas/Hint/HintContent");
            if (hintContentTransform.TryGetComponent<TMPro.TextMeshProUGUI>(out var textComponent))
            {
                textComponent.text = hintText;
                Debug.Log($"[IInteractive] Set HintContent text for {gameObject.name}: {hintText}");
            }
            else
            {
                Debug.LogWarning($"[IInteractive] No HintContent component found in HintItem for {gameObject.name}");
            }
            currentHintItem.transform.localPosition = hintOffset;
            currentHintItem.transform.localScale = hintScale;
        }
    }

    public abstract void Interact();

    /// <summary>
    /// 检查存档状态并恢复物件状态
    /// </summary>
    protected virtual void CheckSaveState()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning($"[IInteractive] SaveManager.Instance is null for {gameObject.name}");
            return;
        }

        Debug.Log($"[IInteractive] CheckSaveState for {gameObject.name} (ID: {interactiveID})");

        // 首先检查是否有直接保存的交互物体状态
        bool canInteract = SaveManager.Instance.GetInteractiveObjectCanInteract(interactiveID);
        bool isActivated = SaveManager.Instance.GetInteractiveObjectIsActivated(interactiveID);

        Debug.Log($"[IInteractive] Saved state for {gameObject.name}: CanInteract={canInteract}, IsActivated={isActivated}");

        if (!canInteract || !isActivated)
        {
            Debug.Log($"[IInteractive] Restoring saved state for {gameObject.name} - CanInteract: {canInteract}, IsActivated: {isActivated}");

            if (!canInteract)
            {
                CanInteract = false;
                Debug.Log($"[IInteractive] Set CanInteract=false for {gameObject.name}");
            }

            if (!isActivated)
            {
                gameObject.SetActive(false);
                Debug.Log($"[IInteractive] Set active=false for {gameObject.name}");
            }
        }
        // 如果没有直接保存的状态，检查对话节点完成状态
        else if (!string.IsNullOrEmpty(linkedDialogueNode))
        {
            bool isDialogueCompleted = SaveManager.Instance.IsDialogueCompleted(linkedDialogueNode);
            Debug.Log($"[IInteractive] Checking dialogue completion for {gameObject.name}: {linkedDialogueNode} = {isDialogueCompleted}");

            if (isDialogueCompleted)
            {
                Debug.Log($"[IInteractive] Dialogue '{linkedDialogueNode}' completed for {gameObject.name}, updating state");

                // 根据设置更新状态
                if (disableAfterDialogue)
                {
                    CanInteract = false;
                    Debug.Log($"[IInteractive] Disabled interaction for {gameObject.name} after dialogue");
                }

                if (deactivateAfterDialogue)
                {
                    gameObject.SetActive(false);
                    Debug.Log($"[IInteractive] Deactivated {gameObject.name} after dialogue");
                }

                // 保存当前状态到存档
                Debug.Log($"[IInteractive] Saving state for {gameObject.name}: CanInteract={CanInteract}, IsActivated={gameObject.activeSelf}");
                SaveManager.Instance.SetInteractiveObjectState(interactiveID, CanInteract, gameObject.activeSelf);
            }
        }
        else
        {
            Debug.Log($"[IInteractive] No linked dialogue node for {gameObject.name}, using default state");
        }
    }

    /// <summary>
    /// 获取交互物件ID
    /// </summary>
    public string GetInteractiveID()
    {
        return interactiveID;
    }

    /// <summary>
    /// 获取关联的对话节点
    /// </summary>
    public string GetLinkedDialogueNode()
    {
        return linkedDialogueNode;
    }

    /// <summary>
    /// 设置关联的对话节点
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    public void SetLinkedDialogueNode(string nodeName)
    {
        linkedDialogueNode = nodeName;
    }

    /// <summary>
    /// 处理对话完成后的状态更新
    /// </summary>
    /// <param name="completedNodeName">完成的对话节点名称</param>
    public virtual void OnDialogueCompleted(string completedNodeName)
    {
        Debug.Log($"[IInteractive] OnDialogueCompleted called for {gameObject.name} with node: {completedNodeName}");
        Debug.Log($"[IInteractive] Linked dialogue node: {linkedDialogueNode}");
        Debug.Log($"[IInteractive] Disable after dialogue: {disableAfterDialogue}");
        Debug.Log($"[IInteractive] Deactivate after dialogue: {deactivateAfterDialogue}");

        // 检查是否是关联的对话节点
        if (!string.IsNullOrEmpty(linkedDialogueNode) && linkedDialogueNode == completedNodeName)
        {
            Debug.Log($"[IInteractive] Dialogue '{completedNodeName}' completed for {gameObject.name}, updating state");

            // 根据设置更新状态
            if (disableAfterDialogue)
            {
                CanInteract = false;
                Debug.Log($"[IInteractive] Disabled interaction for {gameObject.name}");
            }

            if (deactivateAfterDialogue)
            {
                gameObject.SetActive(false);
                Debug.Log($"[IInteractive] Deactivated {gameObject.name}");
            }

            // 保存当前状态到存档
            if (SaveManager.Instance != null)
            {
                Debug.Log($"[IInteractive] Saving state for {gameObject.name}: CanInteract={CanInteract}, IsActivated={gameObject.activeSelf}");
                SaveManager.Instance.SetInteractiveObjectState(interactiveID, CanInteract, gameObject.activeSelf);
            }
            else
            {
                Debug.LogError($"[IInteractive] SaveManager.Instance is null for {gameObject.name}!");
            }
        }
        else
        {
            Debug.Log($"[IInteractive] Node {completedNodeName} does not match linked node {linkedDialogueNode} for {gameObject.name}");
        }
    }

    public virtual void ShowHint()
    {
        if (IsShowHint || !canInteract) return;

        IsShowHint = true;

        // 如果还没有创建提示实例，则创建
        if (currentHintItem == null && hintItemPrefab != null)
        {
            currentHintItem = Instantiate(hintItemPrefab, transform);
            currentHintItem.transform.localPosition = hintOffset;
            currentHintItem.transform.localScale = hintScale;

            // 设置文本内容,找到名为HintContent的子物体
            var hintContentTransform = currentHintItem.transform.Find("Canvas/Hint/HintContent");
            if (hintContentTransform.TryGetComponent<TMPro.TextMeshProUGUI>(out var textComponent))
            {
                textComponent.text = hintText;
            }

            // 验证并确保材质正确应用
            ValidateAndFixMaterial();
        }

        if (currentHintItem != null)
        {
            currentHintItem.SetActive(true);
        }

        // 先定位到Image组件，然后设置材质颜色
        SetImageColor(Color.gray);
    }

    /// <summary>
    /// 验证并修复 HintItem 的材质问题
    /// </summary>
    private void ValidateAndFixMaterial()
    {
        if (currentHintItem == null) return;

        // 获取 Image 组件
        var image = currentHintItem.GetComponentInChildren<UnityEngine.UI.Image>();
        if (image == null)
        {
            Debug.LogWarning($"IInteractive: No Image component found in HintItem for {gameObject.name}");
            return;
        }

        // 检查材质是否正确
        bool hasCorrectMaterial = false;
        if (image.material != null)
        {
            // 检查是否是圆角材质
            if (image.material.name.Contains("UIRounded") ||
                image.material.shader.name.Contains("RoundConorNew"))
            {
                hasCorrectMaterial = true;
                Debug.Log($"IInteractive: HintItem for {gameObject.name} has correct rounded material");
            }
            else
            {
                Debug.LogWarning($"IInteractive: HintItem for {gameObject.name} has incorrect material: {image.material.name}");
            }
        }
        else
        {
            Debug.LogWarning($"IInteractive: HintItem for {gameObject.name} has no material assigned");
        }

        // 如果材质不正确，尝试修复
        if (!hasCorrectMaterial)
        {
            // 尝试从 Resources 加载正确的材质
            var correctMaterial = Resources.Load<Material>("Materials/UIRounded");
            if (correctMaterial != null)
            {
                image.material = correctMaterial;
                Debug.Log($"IInteractive: Successfully loaded and applied rounded material to HintItem for {gameObject.name}");
            }
            else
            {
                Debug.LogError($"IInteractive: Could not find UIRounded material in Resources/Materials/ for {gameObject.name}");
            }
        }

        // 确保 UIDialogueBG 组件正确初始化
        var uiDialogueBG = currentHintItem.GetComponentInChildren<UIDialogueBG>();
        if (uiDialogueBG != null)
        {
            // UIDialogueBG 的 Awake() 应该已经调用了，但我们可以确保它正确工作
            Debug.Log($"IInteractive: UIDialogueBG component found and should be initialized for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"IInteractive: No UIDialogueBG component found in HintItem for {gameObject.name}");
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

        // 先定位到Image组件，然后设置材质颜色为白色
        SetImageColor(Color.white);
    }

    /// <summary>
    /// 设置Image组件的材质颜色
    /// </summary>
    /// <param name="color">要设置的颜色</param>
    private void SetImageColor(Color color)
    {
        // 在当前GameObject上查找名为Sprite的子物体
        var spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null && spriteTransform.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning($"[IInteractive] No SpriteRenderer component found on {gameObject.name}");
        }
        Debug.Log($"[IInteractive] Set SpriteRenderer color to {color} for {gameObject.name}");
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
