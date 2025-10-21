using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public string sceneName;
    
    public PlayerData()
    {
        position = Vector3.zero;
        sceneName = "";
    }
    
    public PlayerData(Vector3 pos, string scene)
    {
        position = pos;
        sceneName = scene;
    }
}

[System.Serializable]
public class InteractiveObjectState
{
    public bool canInteract;
    public bool isActivated;
    
    public InteractiveObjectState()
    {
        canInteract = true;
        isActivated = true;
    }
    
    public InteractiveObjectState(bool canInteract, bool isActivated)
    {
        this.canInteract = canInteract;
        this.isActivated = isActivated;
    }
}

[System.Serializable]
public class InteractiveObjectData
{
    // Unity JsonUtility 不支持 Dictionary，使用 List 进行序列化
    [System.Serializable]
    public class InteractiveObjectEntry
    {
        public string id;
        public InteractiveObjectState state;
        
        public InteractiveObjectEntry() { }
        
        public InteractiveObjectEntry(string id, InteractiveObjectState state)
        {
            this.id = id;
            this.state = state;
        }
    }
    
    public List<InteractiveObjectEntry> objects = new List<InteractiveObjectEntry>();
    
    // 运行时使用的 Dictionary，用于快速查找
    private Dictionary<string, InteractiveObjectState> objectDict = new Dictionary<string, InteractiveObjectState>();
    
    public InteractiveObjectData()
    {
        objects = new List<InteractiveObjectEntry>();
        RefreshDictionary();
    }
    
    public void SetObjectState(string id, bool canInteract, bool isActivated)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var state = new InteractiveObjectState(canInteract, isActivated);
            objectDict[id] = state;
            
            // 更新 List
            var existingEntry = objects.Find(entry => entry.id == id);
            if (existingEntry != null)
            {
                existingEntry.state = state;
            }
            else
            {
                objects.Add(new InteractiveObjectEntry(id, state));
            }
        }
    }
    
    public bool GetCanInteract(string id)
    {
        if (!string.IsNullOrEmpty(id) && objectDict.ContainsKey(id))
        {
            return objectDict[id].canInteract;
        }
        return true; // 默认可交互
    }
    
    public bool GetIsActivated(string id)
    {
        if (!string.IsNullOrEmpty(id) && objectDict.ContainsKey(id))
        {
            return objectDict[id].isActivated;
        }
        return true; // 默认激活
    }
    
    public void RefreshDictionary()
    {
        objectDict.Clear();
        foreach (var entry in objects)
        {
            if (!string.IsNullOrEmpty(entry.id))
            {
                objectDict[entry.id] = entry.state;
            }
        }
    }
    
    public void Clear()
    {
        objects.Clear();
        objectDict.Clear();
    }
}

[System.Serializable]
public class DialogueProcessData
{
    // Unity JsonUtility 不支持 HashSet，使用 List 进行序列化
    public List<string> completedNodes = new List<string>();
    
    // 运行时使用的 HashSet，用于快速查找
    private HashSet<string> completedSet = new HashSet<string>();
    
    public DialogueProcessData()
    {
        completedNodes = new List<string>();
        RefreshSet();
    }
    
    public void AddCompletedNode(string nodeName)
    {
        if (!string.IsNullOrEmpty(nodeName) && !completedSet.Contains(nodeName))
        {
            completedSet.Add(nodeName);
            completedNodes.Add(nodeName);
        }
    }
    
    public bool IsNodeCompleted(string nodeName)
    {
        return !string.IsNullOrEmpty(nodeName) && completedSet.Contains(nodeName);
    }
    
    public void RefreshSet()
    {
        completedSet.Clear();
        foreach (var node in completedNodes)
        {
            if (!string.IsNullOrEmpty(node))
            {
                completedSet.Add(node);
            }
        }
    }
    
    public void Clear()
    {
        completedNodes.Clear();
        completedSet.Clear();
    }
}

[System.Serializable]
public class SceneAnimationData
{
    // Unity JsonUtility 不支持 Dictionary，使用 List 进行序列化
    [System.Serializable]
    public class SceneAnimationEntry
    {
        public string name;
        public bool isCompleted;
        public float progress; // 播放进度 (0-1)
        
        public SceneAnimationEntry() { }
        
        public SceneAnimationEntry(string name, bool isCompleted, float progress)
        {
            this.name = name;
            this.isCompleted = isCompleted;
            this.progress = progress;
        }
    }
    
    public List<SceneAnimationEntry> animations = new List<SceneAnimationEntry>();
    
    // 运行时使用的 Dictionary，用于快速查找
    private Dictionary<string, SceneAnimationEntry> animationDict = new Dictionary<string, SceneAnimationEntry>();
    
    public SceneAnimationData()
    {
        animations = new List<SceneAnimationEntry>();
        RefreshDictionary();
    }
    
    public void AddCompletedAnimation(string animationName)
    {
        if (!string.IsNullOrEmpty(animationName))
        {
            var entry = new SceneAnimationEntry(animationName, true, 1f);
            animationDict[animationName] = entry;
            
            // 更新 List
            var existingEntry = animations.Find(anim => anim.name == animationName);
            if (existingEntry != null)
            {
                existingEntry.isCompleted = true;
                existingEntry.progress = 1f;
            }
            else
            {
                animations.Add(entry);
            }
        }
    }
    
    public void SetAnimationState(string animationName, float progress)
    {
        if (!string.IsNullOrEmpty(animationName))
        {
            var clampedProgress = Mathf.Clamp01(progress);
            var entry = new SceneAnimationEntry(animationName, clampedProgress >= 1f, clampedProgress);
            animationDict[animationName] = entry;
            
            // 更新 List
            var existingEntry = animations.Find(anim => anim.name == animationName);
            if (existingEntry != null)
            {
                existingEntry.isCompleted = clampedProgress >= 1f;
                existingEntry.progress = clampedProgress;
            }
            else
            {
                animations.Add(entry);
            }
        }
    }
    
    public bool IsAnimationCompleted(string animationName)
    {
        if (!string.IsNullOrEmpty(animationName) && animationDict.ContainsKey(animationName))
        {
            return animationDict[animationName].isCompleted;
        }
        return false;
    }
    
    public float GetAnimationProgress(string animationName)
    {
        if (!string.IsNullOrEmpty(animationName) && animationDict.ContainsKey(animationName))
        {
            return animationDict[animationName].progress;
        }
        return 0f;
    }
    
    public void RefreshDictionary()
    {
        animationDict.Clear();
        foreach (var entry in animations)
        {
            if (!string.IsNullOrEmpty(entry.name))
            {
                animationDict[entry.name] = entry;
            }
        }
    }
    
    public void Clear()
    {
        animations.Clear();
        animationDict.Clear();
    }
}

[System.Serializable]
public class GameSaveData
{
    [Header("玩家数据")]
    public PlayerData playerData;
    
    [Header("交互物体数据")]
    public InteractiveObjectData interactiveObjectData;
    
    [Header("对话进度数据")]
    public DialogueProcessData dialogueProcessData;
    
    [Header("场景动画数据")]
    public SceneAnimationData sceneAnimationData;
    
    [Header("存档信息")]
    public DateTime saveTime;
    public string saveVersion = "2.0";
    
    public GameSaveData()
    {
        saveTime = DateTime.Now;
        playerData = new PlayerData();
        interactiveObjectData = new InteractiveObjectData();
        dialogueProcessData = new DialogueProcessData();
        sceneAnimationData = new SceneAnimationData();
    }
    
    /// <summary>
    /// 添加已完成的对话节点
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    public void AddCompletedDialogueNode(string nodeName)
    {
        dialogueProcessData.AddCompletedNode(nodeName);
    }
    
    /// <summary>
    /// 检查对话节点是否已完成
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    /// <returns>是否已完成</returns>
    public bool IsDialogueNodeCompleted(string nodeName)
    {
        return dialogueProcessData.IsNodeCompleted(nodeName);
    }
    
    /// <summary>
    /// 设置玩家位置和场景
    /// </summary>
    /// <param name="position">玩家位置</param>
    /// <param name="sceneName">场景名称</param>
    public void SetPlayerData(Vector3 position, string sceneName)
    {
        playerData = new PlayerData(position, sceneName);
        saveTime = DateTime.Now;
    }
    
    /// <summary>
    /// 添加已完成的场景动画
    /// </summary>
    /// <param name="animationName">动画名称</param>
    public void AddCompletedSceneAnimation(string animationName)
    {
        sceneAnimationData.AddCompletedAnimation(animationName);
    }
    
    /// <summary>
    /// 检查场景动画是否已完成
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns>是否已完成</returns>
    public bool IsSceneAnimationCompleted(string animationName)
    {
        return sceneAnimationData.IsAnimationCompleted(animationName);
    }
    
    /// <summary>
    /// 设置场景动画状态
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="progress">播放进度 (0-1)</param>
    public void SetSceneAnimationState(string animationName, float progress)
    {
        sceneAnimationData.SetAnimationState(animationName, progress);
    }
    
    /// <summary>
    /// 获取场景动画状态
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns>播放进度 (0-1)</returns>
    public float GetSceneAnimationState(string animationName)
    {
        return sceneAnimationData.GetAnimationProgress(animationName);
    }
    
    /// <summary>
    /// 设置交互物体状态
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <param name="canInteract">是否可交互</param>
    /// <param name="isActivated">是否激活</param>
    public void SetInteractiveObjectState(string interactiveID, bool canInteract, bool isActivated)
    {
        interactiveObjectData.SetObjectState(interactiveID, canInteract, isActivated);
    }
    
    /// <summary>
    /// 获取交互物体是否可交互
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否可交互</returns>
    public bool GetInteractiveObjectCanInteract(string interactiveID)
    {
        return interactiveObjectData.GetCanInteract(interactiveID);
    }
    
    /// <summary>
    /// 获取交互物体是否激活
    /// </summary>
    /// <param name="interactiveID">交互物体ID</param>
    /// <returns>是否激活</returns>
    public bool GetInteractiveObjectIsActivated(string interactiveID)
    {
        return interactiveObjectData.GetIsActivated(interactiveID);
    }
    
    /// <summary>
    /// 清空存档数据
    /// </summary>
    public void Clear()
    {
        playerData = new PlayerData();
        interactiveObjectData.Clear();
        dialogueProcessData.Clear();
        sceneAnimationData.Clear();
        saveTime = DateTime.Now;
    }
    
    /// <summary>
    /// 刷新所有子数据类的内部字典/集合
    /// 在反序列化后调用以确保数据一致性
    /// </summary>
    public void RefreshDataStructures()
    {
        interactiveObjectData.RefreshDictionary();
        dialogueProcessData.RefreshSet();
        sceneAnimationData.RefreshDictionary();
    }
}
