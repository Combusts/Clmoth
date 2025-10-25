using UnityEngine;
using Yarn.Unity;

public class GameObjectCommands : MonoBehaviour
{
    /// <summary>
    /// 激活场景中指定名称的GameObject
    /// 支持两种格式：
    /// 1. <<activate_object "ObjectName">>
    /// 2. <<activate_object "CommandManager" "ObjectName">>
    /// </summary>
    /// <param name="param1">第一个参数（可能是CommandManager或ObjectName）</param>
    /// <param name="param2">第二个参数（可选，如果是CommandManager格式则为ObjectName）</param>
    [YarnCommand("activate_object")]
    public void ActivateObject(string param1, string param2 = null)
    {
        string objectName;
        
        // 判断参数格式
        if (!string.IsNullOrEmpty(param2))
        {
            // 格式2: <<activate_object "CommandManager" "ObjectName">>
            objectName = param2;
            Debug.Log($"[GameObjectCommands] 使用CommandManager格式，目标对象: {objectName}");
        }
        else
        {
            // 格式1: <<activate_object "ObjectName">>
            objectName = param1;
            Debug.Log($"[GameObjectCommands] 使用直接格式，目标对象: {objectName}");
        }
        
        GameObject targetObject = FindGameObjectByName(objectName);
        
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"[GameObjectCommands] 已激活GameObject: {objectName}");
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        }
    }

    /// <summary>
    /// 停用场景中指定名称的GameObject
    /// </summary>
    /// <param name="objectName">要停用的GameObject名称</param>
    [YarnCommand("deactivate_object")]
    public void DeactivateObject(string objectName)
    {
        GameObject targetObject = FindGameObjectByName(objectName);
        
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"[GameObjectCommands] 已停用GameObject: {objectName}");

            // 保存状态到存档
            SaveManager.Instance.SetInteractiveObjectState(objectName, false, false);
            Debug.Log($"[GameObjectCommands] 已保存状态到存档: {objectName}");
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        }
    }

    /// <summary>
    /// 停用场景中指定名称的GameObject（支持CommandManager参数格式）
    /// </summary>
    /// <param name="commandManager">CommandManager名称（忽略）</param>
    /// <param name="objectName">要停用的GameObject名称</param>
    [YarnCommand("deactivate_object")]
    public void DeactivateObject(string commandManager, string objectName)
    {
        // 忽略commandManager参数，直接使用objectName
        DeactivateObject(objectName);
    }

    /// <summary>
    /// 切换场景中指定名称的GameObject的激活状态
    /// 支持两种格式：
    /// 1. <<toggle_object "ObjectName">>
    /// 2. <<toggle_object "CommandManager" "ObjectName">>
    /// </summary>
    /// <param name="param1">第一个参数（可能是CommandManager或ObjectName）</param>
    /// <param name="param2">第二个参数（可选，如果是CommandManager格式则为ObjectName）</param>
    [YarnCommand("toggle_object")]
    public void ToggleObject(string param1, string param2 = null)
    {
        string objectName;
        
        // 判断参数格式
        if (!string.IsNullOrEmpty(param2))
        {
            // 格式2: <<toggle_object "CommandManager" "ObjectName">>
            objectName = param2;
            Debug.Log($"[GameObjectCommands] 使用CommandManager格式，目标对象: {objectName}");
        }
        else
        {
            // 格式1: <<toggle_object "ObjectName">>
            objectName = param1;
            Debug.Log($"[GameObjectCommands] 使用直接格式，目标对象: {objectName}");
        }
        
        GameObject targetObject = FindGameObjectByName(objectName);
        
        if (targetObject != null)
        {
            bool newState = !targetObject.activeSelf;
            targetObject.SetActive(newState);
            Debug.Log($"[GameObjectCommands] 已切换GameObject '{objectName}' 状态为: {(newState ? "激活" : "停用")}");
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        }
    }

    /// <summary>
    /// 激活多个GameObject（用逗号分隔名称）
    /// </summary>
    /// <param name="objectNames">要激活的GameObject名称列表，用逗号分隔</param>
    [YarnCommand("activate_objects")]
    public void ActivateObjects(string objectNames)
    {
        string[] names = objectNames.Split(',');
        int activatedCount = 0;
        
        foreach (string name in names)
        {
            string trimmedName = name.Trim();
            GameObject targetObject = FindGameObjectByName(trimmedName);
            
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                activatedCount++;
            }
            else
            {
                Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{trimmedName}' 的GameObject");
            }
        }
        
        Debug.Log($"[GameObjectCommands] 已激活 {activatedCount}/{names.Length} 个GameObject");
    }

    /// <summary>
    /// 停用多个GameObject（用逗号分隔名称）
    /// </summary>
    /// <param name="objectNames">要停用的GameObject名称列表，用逗号分隔</param>
    [YarnCommand("deactivate_objects")]
    public void DeactivateObjects(string objectNames)
    {
        string[] names = objectNames.Split(',');
        int deactivatedCount = 0;
        
        foreach (string name in names)
        {
            string trimmedName = name.Trim();
            GameObject targetObject = FindGameObjectByName(trimmedName);
            
            if (targetObject != null)
            {
                targetObject.SetActive(false);
                deactivatedCount++;
            }
            else
            {
                Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{trimmedName}' 的GameObject");
            }
        }
        
        Debug.Log($"[GameObjectCommands] 已停用 {deactivatedCount}/{names.Length} 个GameObject");
    }

    /// <summary>
    /// 查找场景中指定名称的GameObject
    /// </summary>
    /// <param name="objectName">GameObject名称</param>
    /// <returns>找到的GameObject，如果未找到则返回null</returns>
    private GameObject FindGameObjectByName(string objectName)
    {
        Debug.Log($"[GameObjectCommands] 正在查找GameObject: '{objectName}'");
        
        // 方法1: 使用GameObject.Find（只能找到激活的对象）
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject != null)
        {
            Debug.Log($"[GameObjectCommands] 通过GameObject.Find找到: '{objectName}' (Active: {foundObject.activeSelf})");
            return foundObject;
        }
        
        // 方法2: 使用FindObjectsOfType查找所有对象（包括被停用的）
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        Debug.Log($"[GameObjectCommands] 场景中共有 {allObjects.Length} 个GameObject（包括被停用的）");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                Debug.Log($"[GameObjectCommands] 通过FindObjectsOfType找到: '{objectName}' (Active: {obj.activeSelf})");
                return obj;
            }
        }
        
        // 方法3: 递归查找所有Transform（包括子对象）
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        Debug.Log($"[GameObjectCommands] 场景中共有 {allTransforms.Length} 个Transform");
        
        foreach (Transform transform in allTransforms)
        {
            if (transform.name == objectName)
            {
                Debug.Log($"[GameObjectCommands] 通过Transform查找找到: '{objectName}' (Active: {transform.gameObject.activeSelf})");
                return transform.gameObject;
            }
        }
        
        // 如果还是找不到，列出所有GameObject名称用于调试
        Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        Debug.Log($"[GameObjectCommands] 场景中所有GameObject名称:");
        foreach (GameObject obj in allObjects)
        {
            Debug.Log($"  - '{obj.name}' (Active: {obj.activeSelf})");
        }
        
        return null;
    }

    /// <summary>
    /// 检查指定名称的GameObject是否存在且激活，并将结果存储到Yarn变量中
    /// </summary>
    /// <param name="objectName">GameObject名称</param>
    /// <param name="variableName">要存储结果的Yarn变量名称</param>
    [YarnCommand("check_object_active")]
    public void CheckObjectActive(string objectName, string variableName)
    {
        GameObject targetObject = FindGameObjectByName(objectName);
        bool isActive = false;
        
        if (targetObject != null)
        {
            isActive = targetObject.activeSelf;
            Debug.Log($"[GameObjectCommands] GameObject '{objectName}' 状态: {(isActive ? "激活" : "停用")}");
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        }
        
        // 将结果存储到Yarn变量中
        var dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            dialogueRunner.VariableStorage.SetValue($"${variableName}", isActive);
            Debug.Log($"[GameObjectCommands] 已将结果 {isActive} 存储到变量 ${variableName}");
        }
        else
        {
            Debug.LogError("[GameObjectCommands] 未找到DialogueRunner，无法设置变量");
        }
    }


    /// <summary>
    /// 设置GameObject AnchoredPosition
    /// </summary>
    /// <param name="objectName">GameObject名称</param>
    /// <param name="anchoredPositionx">AnchoredPositionX</param>
    /// <param name="anchoredPositiony">AnchoredPositionY</param>
    [YarnCommand("set_anchored_position")]
    public void SetAnchoredPosition(string objectName, float anchoredPositionx, float anchoredPositiony)
    {
        GameObject targetObject = FindGameObjectByName(objectName);
        Debug.Log($"[GameObjectCommands] 正在设置GameObject '{objectName}' 的AnchoredPosition为: {anchoredPositionx}, {anchoredPositiony}");
        if (targetObject != null && targetObject.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchoredPosition = new Vector2(anchoredPositionx, anchoredPositiony);
            Debug.Log($"[GameObjectCommands] 已设置GameObject '{objectName}' 的AnchoredPosition为: {anchoredPositionx}, {anchoredPositiony}");
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{objectName}' 的GameObject");
        }
    }


    /// <summary>
    /// 设置GameObject 方向，使用ScaleX和ScaleY的正负来确定方向
    /// </summary>
    /// <param name="objectName">GameObject名称</param>
    /// <param name="scaleX">ScaleX</param>
    /// <param name="scaleY">ScaleY</param>
    [YarnCommand("set_direction")]
    public void SetDirection(string objectName, bool isLeft = true)
    {
        Debug.Log($"[GameObjectCommands] 正在设置GameObject '{objectName}' 的方向为: {(isLeft ? "左侧" : "右侧")}");
        GameObject targetObject = FindGameObjectByName(objectName);
        // 获取GameObject的原始Scale
        Vector3 originalScale = targetObject.transform.localScale;
        // 如果isLeft为true，则设置ScaleX为-1
        if (isLeft)
        {
            targetObject.transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else
        {
            targetObject.transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
    }

    /// <summary>
    /// 显示立绘
    /// 格式：<<show_illustration "IllustrationPanel" "ClothOpenEyes" false>>
    /// </summary>
    /// <param name="panelName">立绘面板名称</param>
    /// <param name="illustrationName">立绘名称</param>
    /// <param name="isLeft">是否显示在左侧</param>
    [YarnCommand("show_illustration")]
    public void ShowIllustration(string panelName, string illustrationName, bool isLeft)
    {
        Debug.Log($"[GameObjectCommands] 显示立绘: {illustrationName} 在 {panelName} {(isLeft ? "左侧" : "右侧")}");
        
        GameObject illustrationPanel = FindGameObjectByName(panelName);
        
        if (illustrationPanel != null)
        {
            IllustrationController controller = illustrationPanel.GetComponent<IllustrationController>();
            if (controller != null)
            {
                controller.ShowIllustration(illustrationName, isLeft);
                Debug.Log($"[GameObjectCommands] 已显示立绘: {illustrationName}");
            }
            else
            {
                Debug.LogError($"[GameObjectCommands] IllustrationController组件未找到在 {panelName} 上");
            }
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{panelName}' 的立绘面板");
        }
    }

    /// <summary>
    /// 隐藏立绘
    /// 格式：<<hide_illustration "IllustrationPanel">>
    /// </summary>
    /// <param name="panelName">立绘面板名称</param>
    [YarnCommand("hide_illustration")]
    public void HideIllustration(string panelName)
    {
        Debug.Log($"[GameObjectCommands] 隐藏立绘: {panelName}");
        
        GameObject illustrationPanel = FindGameObjectByName(panelName);
        
        if (illustrationPanel != null)
        {
            IllustrationController controller = illustrationPanel.GetComponent<IllustrationController>();
            if (controller != null)
            {
                controller.HideIllustration();
                Debug.Log($"[GameObjectCommands] 已隐藏立绘: {panelName}");
            }
            else
            {
                Debug.LogError($"[GameObjectCommands] IllustrationController组件未找到在 {panelName} 上");
            }
        }
        else
        {
            Debug.LogWarning($"[GameObjectCommands] 未找到名为 '{panelName}' 的立绘面板");
        }
    }
}
