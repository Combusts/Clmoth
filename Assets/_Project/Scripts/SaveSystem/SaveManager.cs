using System;
using System.IO;
using UnityEngine;
using Yarn.Unity;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("存档设置")]
    [SerializeField] private string saveFileName = "gamesave.json";
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private bool enableDebugLogs = true;

    private GameSaveData currentSaveData;
    private string savePath;

    // 事件
    public static event Action<GameSaveData> OnSaveDataLoaded;
    public static event Action<GameSaveData> OnSaveDataSaved;
    public static event Action OnSaveDataCleared;

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化存档路径
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // 初始化存档数据
        currentSaveData = new GameSaveData();

        LogDebug($"SaveManager initialized. Save path: {savePath}");
    }

    // void Start()
    // {
    //     // 尝试加载现有存档
    //     LoadSaveData();
    // }

    /// <summary>
    /// 保存存档数据
    /// </summary>
    public void SaveGame()
    {
        try
        {
            // 更新保存时间
            currentSaveData.saveTime = DateTime.Now;

            // 序列化为JSON
            string jsonData = JsonUtility.ToJson(currentSaveData, true);

            // 写入文件
            File.WriteAllText(savePath, jsonData);

            OnSaveDataSaved?.Invoke(currentSaveData);
        }
        catch (Exception e)
        {
            LogError($"Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    public void LoadSaveData()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string jsonData = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                // 确保子数据类不为null并刷新内部结构
                currentSaveData.playerData ??= new PlayerData();
                currentSaveData.interactiveObjectData ??= new InteractiveObjectData();
                currentSaveData.dialogueProcessData ??= new DialogueProcessData();
                currentSaveData.sceneAnimationData ??= new SceneAnimationData();

                // 刷新所有子数据类的内部字典/集合
                currentSaveData.RefreshDataStructures();

                LogDebug($"Game loaded successfully from: {savePath}");
                LogDebug($"Loaded {currentSaveData.dialogueProcessData.completedNodes.Count} completed dialogue nodes");

                // 添加详细的动画数据debug信息
                LogDebug($"=== 动画存档数据详情 ===");
                LogDebug($"场景动画数据是否为空: {currentSaveData.sceneAnimationData == null}");
                if (currentSaveData.sceneAnimationData != null)
                {
                    LogDebug($"场景动画列表数量: {currentSaveData.sceneAnimationData.animations.Count}");
                    foreach (var anim in currentSaveData.sceneAnimationData.animations)
                    {
                        LogDebug($"动画: {anim.name}, 完成状态: {anim.isCompleted}, 进度: {anim.progress}");
                    }
                }
                LogDebug($"=== 动画存档数据详情结束 ===");

                OnSaveDataLoaded?.Invoke(currentSaveData);
            }
            else
            {
                LogDebug("No save file found, creating new save data");
                currentSaveData = new GameSaveData();
            }
        }
        catch (Exception e)
        {
            LogError($"Failed to load game: {e.Message}");
            currentSaveData = new GameSaveData();
        }
    }

    /// <summary>
    /// 检查存档文件是否存在
    /// </summary>
    /// <returns>是否存在存档</returns>
    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// 删除存档文件
    /// </summary>
    public void DeleteSaveData()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                LogDebug("Save file deleted successfully");
            }

            currentSaveData.Clear();
            OnSaveDataCleared?.Invoke();
        }
        catch (Exception e)
        {
            LogError($"Failed to delete save file: {e.Message}");
        }
    }

    /// <summary>
    /// 添加已完成的对话节点
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    [YarnCommand("AddCompletedDialogueNode")]
    public void AddCompletedDialogueNode(string nodeName)
    {
        if (currentSaveData != null)
        {
            currentSaveData.AddCompletedDialogueNode(nodeName);
            LogDebug($"Added completed dialogue node: {nodeName}");

            // 自动保存
            if (enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// 检查对话节点是否已完成
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    /// <returns>是否已完成</returns>
    public bool IsDialogueCompleted(string nodeName)
    {
        return currentSaveData != null && currentSaveData.IsDialogueNodeCompleted(nodeName);
    }

    /// <summary>
    /// 设置玩家位置和场景
    /// </summary>
    /// <param name="position">玩家位置</param>
    /// <param name="sceneName">场景名称</param>
    public void SetPlayerData(Vector3 position, string sceneName)
    {
        if (currentSaveData != null)
        {
            currentSaveData.SetPlayerData(position, sceneName);

            // 自动保存
            if (enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// 获取玩家位置
    /// </summary>
    /// <returns>玩家位置</returns>
    public Vector3 GetPlayerPosition()
    {
        return currentSaveData != null ? currentSaveData.playerData.position : Vector3.zero;
    }

    /// <summary>
    /// 获取当前场景名称
    /// </summary>
    /// <returns>场景名称</returns>
    public string GetCurrentSceneName()
    {
        return currentSaveData != null ? currentSaveData.playerData.sceneName : "";
    }

    /// <summary>
    /// 获取存档数据
    /// </summary>
    /// <returns>当前存档数据</returns>
    public GameSaveData GetSaveData()
    {
        return currentSaveData;
    }

    /// <summary>
    /// 获取已完成的对话节点数量
    /// </summary>
    /// <returns>节点数量</returns>
    public int GetCompletedDialogueCount()
    {
        return currentSaveData != null ? currentSaveData.dialogueProcessData.completedNodes.Count : 0;
    }

    /// <summary>
    /// 获取存档时间
    /// </summary>
    /// <returns>存档时间</returns>
    public DateTime GetSaveTime()
    {
        return currentSaveData != null ? currentSaveData.saveTime : DateTime.MinValue;
    }

    /// <summary>
    /// 添加已完成的场景动画
    /// </summary>
    /// <param name="animationName">动画名称</param>
    public void AddCompletedSceneAnimation(string animationName)
    {
        if (currentSaveData != null)
        {
            currentSaveData.AddCompletedSceneAnimation(animationName);
            LogDebug($"Added completed scene animation: {animationName}");

            // 自动保存
            if (enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// 检查场景动画是否已完成
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns>是否已完成</returns>
    public bool IsSceneAnimationCompleted(string animationName)
    {
        bool result = currentSaveData != null && currentSaveData.IsSceneAnimationCompleted(animationName);
        LogDebug($"检查动画完成状态: {animationName} = {result}");
        return result;
    }

    /// <summary>
    /// 设置场景动画状态
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="progress">播放进度 (0-1)</param>
    public void SetSceneAnimationState(string animationName, float progress)
    {
        if (currentSaveData != null)
        {
            currentSaveData.SetSceneAnimationState(animationName, progress);
            LogDebug($"Set scene animation state: {animationName} = {progress}");

            // 自动保存
            if (enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// 获取场景动画状态
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns>播放进度 (0-1)</returns>
    public float GetSceneAnimationState(string animationName)
    {
        float result = currentSaveData != null ? currentSaveData.GetSceneAnimationState(animationName) : 0f;
        LogDebug($"获取动画状态: {animationName} = {result}");
        return result;
    }

    /// <summary>
    /// 获取已完成的场景动画数量
    /// </summary>
    /// <returns>动画数量</returns>
    public int GetCompletedSceneAnimationCount()
    {
        return currentSaveData != null ? currentSaveData.sceneAnimationData.animations.Count : 0;
    }

    /// <summary>
    /// 设置交互物体状态
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <param name="canInteract">是否可交互</param>
    /// <param name="isActivated">是否激活</param>
    public void SetInteractiveObjectState(string interactiveID, bool canInteract, bool isActivated)
    {
        if (currentSaveData != null)
        {
            currentSaveData.SetInteractiveObjectState(interactiveID, canInteract, isActivated);
            LogDebug($"Set interactive object state: {interactiveID} - CanInteract: {canInteract}, IsActivated: {isActivated}");

            // 自动保存
            if (enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// 获取交互物体是否可交互
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否可交互，如果没有存档则返回null</returns>
    public bool? GetInteractiveObjectCanInteract(string interactiveID)
    {
        return currentSaveData?.GetInteractiveObjectCanInteract(interactiveID);
    }

    /// <summary>
    /// 获取交互物体是否激活
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否激活，如果没有存档则返回null</returns>
    public bool? GetInteractiveObjectIsActivated(string interactiveID)
    {
        return currentSaveData?.GetInteractiveObjectIsActivated(interactiveID);
    }

    // 向后兼容方法 - 保持旧API可用
    /// <summary>
    /// 设置交互物体状态（向后兼容）
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <param name="isDisabled">是否禁用交互</param>
    /// <param name="isDeactivated">是否隐藏物体</param>
    [System.Obsolete("Use SetInteractiveObjectState(string, bool, bool) with canInteract and isActivated parameters instead")]
    public void SetInteractiveObjectStateLegacy(string interactiveID, bool isDisabled, bool isDeactivated)
    {
        SetInteractiveObjectState(interactiveID, !isDisabled, !isDeactivated);
    }

    /// <summary>
    /// 获取交互物体是否禁用（向后兼容）
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否禁用</returns>
    [System.Obsolete("Use GetInteractiveObjectCanInteract() instead")]
    public bool IsInteractiveObjectDisabled(string interactiveID)
    {
        return !GetInteractiveObjectCanInteract(interactiveID) ?? false;
    }

    /// <summary>
    /// 获取交互物体是否隐藏（向后兼容）
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否隐藏</returns>
    [System.Obsolete("Use GetInteractiveObjectIsActivated() instead")]
    public bool IsInteractiveObjectDeactivated(string interactiveID)
    {
        return !GetInteractiveObjectIsActivated(interactiveID) ?? false;
    }

    // 调试方法
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SaveManager] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[SaveManager] {message}");
    }

    // 编辑器调试
    [ContextMenu("Save Game")]
    private void EditorSaveGame()
    {
        SaveGame();
    }

    [ContextMenu("Load Game")]
    private void EditorLoadGame()
    {
        LoadSaveData();
    }

    [ContextMenu("Delete Save")]
    private void EditorDeleteSave()
    {
        DeleteSaveData();
    }
}
