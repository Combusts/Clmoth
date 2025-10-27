using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class YarnSpinnerManager : MonoBehaviour
{
    public static YarnSpinnerManager Instance { get; private set; }
    
    [Header("Dialogue System")]
    [SerializeField] private DialogueRunner dialogueRunner;
    
    [Header("Scene Configuration")]
    [SerializeField] private List<SceneDialogueConfig> sceneConfigs = new();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Illustration")]
    [SerializeField] private GameObject illustrationPanel;
    
    private readonly Dictionary<string, List<string>> sceneToNodesMap = new();
    private bool isDialogueActive = false;
    private string currentDialogueNode = "";
    
    [System.Serializable]
    public class SceneDialogueConfig
    {
        public string sceneName;
        public List<string> nodeNames = new();
    }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize scene-to-nodes mapping
        InitializeSceneMapping();
        
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void Start()
    {
        // Ensure DialogueRunner is assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                LogError("DialogueRunner not found! Please assign it in the Inspector or ensure it exists in the scene.");
                return;
            }
        }
        
        // Subscribe to dialogue events
        SubscribeToDialogueEvents();
        
        // Load nodes for current scene
        LoadNodesForCurrentScene();

        // Create IllustrationPanel in don't destroy on load
        illustrationPanel = Instantiate(Resources.Load<GameObject>("Prefabs/UI/InGame/IllustrationPanel"), transform);
        illustrationPanel.name = "IllustrationPanel";
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromDialogueEvents();
    }
    
    private void InitializeSceneMapping()
    {
        sceneToNodesMap.Clear();
        
        foreach (var config in sceneConfigs)
        {
            if (!string.IsNullOrEmpty(config.sceneName) && config.nodeNames.Count > 0)
            {
                sceneToNodesMap[config.sceneName] = new List<string>(config.nodeNames);
                LogDebug($"Mapped scene '{config.sceneName}' to nodes: {string.Join(", ", config.nodeNames)}");
            }
        }
        
        // Add default Level_01 configuration if not present
        if (!sceneToNodesMap.ContainsKey("Level_01"))
        {
            sceneToNodesMap["Level_01"] = new List<string> { "Start", "work", "file", "A", "window", "badream" };
            LogDebug("Added default Level_01 configuration");
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"Scene loaded: {scene.name}");
        LoadNodesForScene(scene.name);
    }
    
    private void LoadNodesForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadNodesForScene(currentSceneName);
    }
    
    private void LoadNodesForScene(string sceneName)
    {
        if (dialogueRunner == null)
        {
            LogError("DialogueRunner is null, cannot load nodes");
            return;
        }
        
        if (sceneToNodesMap.ContainsKey(sceneName))
        {
            var nodes = sceneToNodesMap[sceneName];
            LogDebug($"Loading {nodes.Count} nodes for scene '{sceneName}': {string.Join(", ", nodes)}");
            
            // Note: YarnSpinner automatically loads all nodes from assigned Yarn scripts
            // This method is for tracking and debugging purposes
        }
        else
        {
            LogDebug($"No dialogue nodes configured for scene '{sceneName}'");
        }
    }
    
    private void SubscribeToDialogueEvents()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            dialogueRunner.onNodeStart.AddListener(OnNodeStart);
            dialogueRunner.onNodeComplete.AddListener(OnNodeComplete);
        }
    }
    
    private void UnsubscribeFromDialogueEvents()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
            dialogueRunner.onNodeStart.RemoveListener(OnNodeStart);
            dialogueRunner.onNodeComplete.RemoveListener(OnNodeComplete);
        }
    }
    
    private void OnDialogueStart()
    {
        isDialogueActive = true;
        LogDebug("Dialogue started - disabling gameplay input");
        
        // Disable player input during dialogue
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.DisableGameplayInput();
        }
    }
    
    private void OnDialogueComplete()
    {
        isDialogueActive = false;
        LogDebug("Dialogue completed - enabling gameplay input");
        
        // Re-enable player input after dialogue
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.EnableGameplayInput();
        }
        
        // 保存当前对话节点为已完成
        if (!string.IsNullOrEmpty(currentDialogueNode))
        {
            // 检查SaveManager是否可用
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AddCompletedDialogueNode(currentDialogueNode);
                LogDebug($"Marked dialogue node '{currentDialogueNode}' as completed");
            }
            else
            {
                LogError("SaveManager.Instance is null! Cannot save dialogue completion. Make sure SaveManager exists in the scene.");
            }
            
            // 通知所有交互物体对话已完成
            NotifyInteractiveObjects(currentDialogueNode);
        }
        
        // 清空当前对话节点
        currentDialogueNode = "";
    }
    
    private void OnNodeStart(string nodeName)
    {
        currentDialogueNode = nodeName;
        LogDebug($"Dialogue node started: {nodeName}");
    }
    
    private void OnNodeComplete(string nodeName)
    {
        LogDebug($"Dialogue node completed: {nodeName}");
        // 注意：这里不直接保存，而是在整个对话完成时保存
        // 这样可以避免在对话中间保存不完整的状态
    }
    
    /// <summary>
    /// 通知所有交互物体对话已完成
    /// </summary>
    /// <param name="completedNodeName">完成的对话节点名称</param>
    private void NotifyInteractiveObjects(string completedNodeName)
    {
        // 查找场景中所有的交互物体
        IInteractive[] interactiveObjects = FindObjectsOfType<IInteractive>();
        
        LogDebug($"Notifying {interactiveObjects.Length} interactive objects about completed dialogue: {completedNodeName}");
        
        foreach (IInteractive interactiveObject in interactiveObjects)
        {
            if (interactiveObject != null)
            {
                interactiveObject.OnDialogueCompleted(completedNodeName);
            }
        }
        
        // 对话完成后，保存所有交互物体的当前状态
        SaveAllInteractiveObjectsState();
    }
    
    /// <summary>
    /// 保存所有交互物体的当前状态
    /// </summary>
    private void SaveAllInteractiveObjectsState()
    {
        if (SaveManager.Instance == null) return;
        
        // 查找场景中所有的交互物体
        IInteractive[] interactiveObjects = FindObjectsOfType<IInteractive>();
        
        LogDebug($"Saving state for {interactiveObjects.Length} interactive objects");
        
        foreach (IInteractive interactiveObject in interactiveObjects)
        {
            if (interactiveObject != null)
            {
                string interactiveID = interactiveObject.GetInteractiveID();
                if (!string.IsNullOrEmpty(interactiveID))
                {
                    // 保存当前状态：CanInteract 和 IsActivated (gameObject.activeSelf)
                    SaveManager.Instance.SetInteractiveObjectState(
                        interactiveID, 
                        interactiveObject.CanInteract, 
                        interactiveObject.gameObject.activeSelf
                    );
                    
                    LogDebug($"Saved state for {interactiveObject.gameObject.name}: CanInteract={interactiveObject.CanInteract}, IsActivated={interactiveObject.gameObject.activeSelf}");
                }
            }
        }
    }
    
    /// <summary>
    /// 检查对话是否应该自动开始
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    /// <returns>是否应该开始对话</returns>
    public bool ShouldStartDialogueAutomatically(string nodeName)
    {
        // 如果存档系统存在且对话已完成，则不自动开始
        if (SaveManager.Instance != null && SaveManager.Instance.IsDialogueCompleted(nodeName))
        {
            LogDebug($"Dialogue node '{nodeName}' already completed, skipping auto-start");
            return false;
        }
        
        // 如果SaveManager不存在，记录警告但允许对话开始
        if (SaveManager.Instance == null)
        {
            LogError("SaveManager.Instance is null! Cannot check dialogue completion status. Make sure SaveManager exists in the scene.");
        }
        
        return true;
    }
    
    /// <summary>
    /// 安全地开始对话（检查存档状态）
    /// </summary>
    /// <param name="nodeName">对话节点名称</param>
    /// <param name="checkSaveState">是否检查存档状态</param>
    public void StartDialogueSafe(string nodeName, bool checkSaveState = true)
    {
        if (checkSaveState && !ShouldStartDialogueAutomatically(nodeName))
        {
            return;
        }
        
        StartDialogue(nodeName);
    }
    
    // Public API methods
    public void StartDialogue(string nodeName)
    {
        if (dialogueRunner == null)
        {
            LogError("DialogueRunner is null, cannot start dialogue");
            return;
        }
        
        if (dialogueRunner.Dialogue.NodeExists(nodeName))
        {
            LogDebug($"Starting dialogue node: {nodeName}");
            dialogueRunner.StartDialogue(nodeName);
        }
        else
        {
            LogError($"Dialogue node '{nodeName}' does not exist!");
        }
    }
    
    public void StopDialogue()
    {
        if (dialogueRunner != null && isDialogueActive)
        {
            LogDebug("Stopping dialogue");
            dialogueRunner.Stop();
        }
    }
    
    public bool IsDialogueActive => isDialogueActive;
    
    public bool NodeExists(string nodeName)
    {
        return dialogueRunner != null && dialogueRunner.Dialogue.NodeExists(nodeName);
    }
    
    /// <summary>
    /// 检查SaveManager是否可用
    /// </summary>
    /// <returns>SaveManager是否可用</returns>
    public bool IsSaveManagerAvailable()
    {
        return SaveManager.Instance != null;
    }
    
    public List<string> GetAvailableNodesForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (sceneToNodesMap.ContainsKey(currentSceneName))
        {
            return new List<string>(sceneToNodesMap[currentSceneName]);
        }
        return new List<string>();
    }
    
    // Configuration methods
    public void AddSceneConfiguration(string sceneName, List<string> nodeNames)
    {
        if (!string.IsNullOrEmpty(sceneName) && nodeNames != null && nodeNames.Count > 0)
        {
            sceneToNodesMap[sceneName] = new List<string>(nodeNames);
            LogDebug($"Added configuration for scene '{sceneName}' with {nodeNames.Count} nodes");
        }
    }
    
    public void RemoveSceneConfiguration(string sceneName)
    {
        if (sceneToNodesMap.ContainsKey(sceneName))
        {
            sceneToNodesMap.Remove(sceneName);
            LogDebug($"Removed configuration for scene '{sceneName}'");
        }
    }
    
    // Debug methods
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[YarnSpinnerManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[YarnSpinnerManager] {message}");
    }
    
    // Inspector validation
    void OnValidate()
    {
        // Ensure DialogueRunner is assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = GetComponent<DialogueRunner>();
        }
    }
}
