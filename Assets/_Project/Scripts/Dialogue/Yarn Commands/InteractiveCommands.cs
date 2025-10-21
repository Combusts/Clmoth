using UnityEngine;
using Yarn.Unity;

public class InteractiveCommands : MonoBehaviour
{
    /// <summary>
    /// 启用指定名称的交互对象的交互功能
    /// 支持两种格式：
    /// 1. <<enable_interaction "ObjectName">>
    /// 2. <<enable_interaction "CommandManager" "ObjectName">>
    /// </summary>
    /// <param name="param1">第一个参数（可能是CommandManager或ObjectName）</param>
    /// <param name="param2">第二个参数（可选，如果是CommandManager格式则为ObjectName）</param>
    [YarnCommand("enable_interaction")]
    public void EnableInteraction(string param1, string param2 = null)
    {
        string objectName;
        
        // 判断参数格式
        if (!string.IsNullOrEmpty(param2))
        {
            // 格式2: <<enable_interaction "CommandManager" "ObjectName">>
            objectName = param2;
            Debug.Log($"[InteractiveCommands] 使用CommandManager格式，目标对象: {objectName}");
        }
        else
        {
            // 格式1: <<enable_interaction "ObjectName">>
            objectName = param1;
            Debug.Log($"[InteractiveCommands] 使用直接格式，目标对象: {objectName}");
        }
        
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            Debug.Log($"[InteractiveCommands] 找到交互对象: {interactiveObject.gameObject.name}");
            Debug.Log($"[InteractiveCommands] 设置前CanInteract状态: {interactiveObject.CanInteract}");
            
            interactiveObject.CanInteract = true;
            
            Debug.Log($"[InteractiveCommands] 设置后CanInteract状态: {interactiveObject.CanInteract}");
            Debug.Log($"[InteractiveCommands] 已启用GameObject '{objectName}' 的交互功能");
            
            // 保存状态到存档
            SaveInteractiveState(interactiveObject);
            
            // 再次验证状态
            Debug.Log($"[InteractiveCommands] 验证最终CanInteract状态: {interactiveObject.CanInteract}");
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 禁用指定名称的交互对象的交互功能
    /// 支持两种格式：
    /// 1. <<disable_interaction "ObjectName">>
    /// 2. <<disable_interaction "CommandManager" "ObjectName">>
    /// </summary>
    /// <param name="param1">第一个参数（可能是CommandManager或ObjectName）</param>
    /// <param name="param2">第二个参数（可选，如果是CommandManager格式则为ObjectName）</param>
    [YarnCommand("disable_interaction")]
    public void DisableInteraction(string param1, string param2 = null)
    {
        string objectName;
        
        // 判断参数格式
        if (!string.IsNullOrEmpty(param2))
        {
            // 格式2: <<disable_interaction "CommandManager" "ObjectName">>
            objectName = param2;
            Debug.Log($"[InteractiveCommands] 使用CommandManager格式，目标对象: {objectName}");
        }
        else
        {
            // 格式1: <<disable_interaction "ObjectName">>
            objectName = param1;
            Debug.Log($"[InteractiveCommands] 使用直接格式，目标对象: {objectName}");
        }
        
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            interactiveObject.CanInteract = false;
            interactiveObject.HideHint(); // 隐藏提示
            Debug.Log($"[InteractiveCommands] 已禁用GameObject '{objectName}' 的交互功能");
            
            // 保存状态到存档
            SaveInteractiveState(interactiveObject);
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 切换指定名称的交互对象的交互状态
    /// </summary>
    /// <param name="objectName">要切换交互状态的GameObject名称</param>
    [YarnCommand("toggle_interaction")]
    public void ToggleInteraction(string objectName)
    {
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            bool newState = !interactiveObject.CanInteract;
            interactiveObject.CanInteract = newState;
            
            if (!newState)
            {
                interactiveObject.HideHint(); // 如果禁用交互，隐藏提示
            }
            
            Debug.Log($"[InteractiveCommands] 已切换GameObject '{objectName}' 的交互状态为: {(newState ? "启用" : "禁用")}");
            
            // 保存状态到存档
            SaveInteractiveState(interactiveObject);
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 启用多个交互对象的交互功能（用逗号分隔名称）
    /// </summary>
    /// <param name="objectNames">要启用交互的GameObject名称列表，用逗号分隔</param>
    [YarnCommand("enable_interactions")]
    public void EnableInteractions(string objectNames)
    {
        string[] names = objectNames.Split(',');
        int enabledCount = 0;
        
        foreach (string name in names)
        {
            string trimmedName = name.Trim();
            IInteractive interactiveObject = FindInteractiveObjectByName(trimmedName);
            
            if (interactiveObject != null)
            {
                interactiveObject.CanInteract = true;
                enabledCount++;
                SaveInteractiveState(interactiveObject);
            }
            else
            {
                Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{trimmedName}' 的交互对象");
            }
        }
        
        Debug.Log($"[InteractiveCommands] 已启用 {enabledCount}/{names.Length} 个交互对象的交互功能");
    }

    /// <summary>
    /// 禁用多个交互对象的交互功能（用逗号分隔名称）
    /// </summary>
    /// <param name="objectNames">要禁用交互的GameObject名称列表，用逗号分隔</param>
    [YarnCommand("disable_interactions")]
    public void DisableInteractions(string objectNames)
    {
        string[] names = objectNames.Split(',');
        int disabledCount = 0;
        
        foreach (string name in names)
        {
            string trimmedName = name.Trim();
            IInteractive interactiveObject = FindInteractiveObjectByName(trimmedName);
            
            if (interactiveObject != null)
            {
                interactiveObject.CanInteract = false;
                interactiveObject.HideHint(); // 隐藏提示
                disabledCount++;
                SaveInteractiveState(interactiveObject);
            }
            else
            {
                Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{trimmedName}' 的交互对象");
            }
        }
        
        Debug.Log($"[InteractiveCommands] 已禁用 {disabledCount}/{names.Length} 个交互对象的交互功能");
    }

    /// <summary>
    /// 检查指定名称的交互对象是否可以交互，并将结果存储到Yarn变量中
    /// </summary>
    /// <param name="objectName">交互对象名称</param>
    /// <param name="variableName">要存储结果的Yarn变量名称</param>
    [YarnCommand("check_interaction_enabled")]
    public void CheckInteractionEnabled(string objectName, string variableName)
    {
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        bool canInteract = false;
        
        if (interactiveObject != null)
        {
            canInteract = interactiveObject.CanInteract;
            Debug.Log($"[InteractiveCommands] GameObject '{objectName}' 交互状态: {(canInteract ? "启用" : "禁用")}");
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
        
        // 将结果存储到Yarn变量中
        var dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            dialogueRunner.VariableStorage.SetValue($"${variableName}", canInteract);
            Debug.Log($"[InteractiveCommands] 已将结果 {canInteract} 存储到变量 ${variableName}");
        }
        else
        {
            Debug.LogError("[InteractiveCommands] 未找到DialogueRunner，无法设置变量");
        }
    }

    /// <summary>
    /// 设置交互对象的提示文本
    /// </summary>
    /// <param name="objectName">交互对象名称</param>
    /// <param name="hintText">新的提示文本</param>
    [YarnCommand("set_hint_text")]
    public void SetHintText(string objectName, string hintText)
    {
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            interactiveObject.HintText = hintText;
            Debug.Log($"[InteractiveCommands] 已设置GameObject '{objectName}' 的提示文本为: {hintText}");
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 显示指定交互对象的提示
    /// </summary>
    /// <param name="objectName">交互对象名称</param>
    [YarnCommand("show_hint")]
    public void ShowHint(string objectName)
    {
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            interactiveObject.ShowHint();
            Debug.Log($"[InteractiveCommands] 已显示GameObject '{objectName}' 的提示");
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 隐藏指定交互对象的提示
    /// </summary>
    /// <param name="objectName">交互对象名称</param>
    [YarnCommand("hide_hint")]
    public void HideHint(string objectName)
    {
        IInteractive interactiveObject = FindInteractiveObjectByName(objectName);
        
        if (interactiveObject != null)
        {
            interactiveObject.HideHint();
            Debug.Log($"[InteractiveCommands] 已隐藏GameObject '{objectName}' 的提示");
        }
        else
        {
            Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        }
    }

    /// <summary>
    /// 查找场景中指定名称的交互对象
    /// </summary>
    /// <param name="objectName">交互对象名称</param>
    /// <returns>找到的交互对象，如果未找到则返回null</returns>
    private IInteractive FindInteractiveObjectByName(string objectName)
    {
        Debug.Log($"[InteractiveCommands] 正在查找交互对象: '{objectName}'");
        
        // 首先尝试通过GameObject名称查找
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject != null)
        {
            Debug.Log($"[InteractiveCommands] 通过GameObject.Find找到: '{objectName}' (Active: {foundObject.activeSelf})");
            IInteractive interactive = foundObject.GetComponent<IInteractive>();
            if (interactive != null)
            {
                Debug.Log($"[InteractiveCommands] 找到IInteractive组件，当前CanInteract状态: {interactive.CanInteract}");
                return interactive;
            }
            else
            {
                Debug.LogWarning($"[InteractiveCommands] GameObject '{objectName}' 没有IInteractive组件");
            }
        }
        
        // 如果在当前场景中未找到，尝试在所有场景中查找
        IInteractive[] allInteractiveObjects = FindObjectsOfType<IInteractive>();
        Debug.Log($"[InteractiveCommands] 场景中共有 {allInteractiveObjects.Length} 个IInteractive对象");
        
        foreach (IInteractive interactive in allInteractiveObjects)
        {
            Debug.Log($"[InteractiveCommands] 检查对象: '{interactive.gameObject.name}' (Active: {interactive.gameObject.activeSelf}, CanInteract: {interactive.CanInteract})");
            if (interactive.gameObject.name == objectName)
            {
                Debug.Log($"[InteractiveCommands] 通过FindObjectsOfType找到匹配对象: '{objectName}'");
                return interactive;
            }
        }
        
        Debug.LogWarning($"[InteractiveCommands] 未找到名为 '{objectName}' 的交互对象");
        return null;
    }

    /// <summary>
    /// 保存交互对象状态到存档
    /// </summary>
    /// <param name="interactiveObject">要保存的交互对象</param>
    private void SaveInteractiveState(IInteractive interactiveObject)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetInteractiveObjectState(
                interactiveObject.GetInteractiveID(), 
                interactiveObject.CanInteract, 
                interactiveObject.gameObject.activeSelf
            );
        }
    }
}
