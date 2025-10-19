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
    [SerializeField] private List<SceneDialogueConfig> sceneConfigs = new List<SceneDialogueConfig>();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Dictionary<string, List<string>> sceneToNodesMap = new Dictionary<string, List<string>>();
    private bool isDialogueActive = false;
    
    [System.Serializable]
    public class SceneDialogueConfig
    {
        public string sceneName;
        public List<string> nodeNames = new List<string>();
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
        }
    }
    
    private void UnsubscribeFromDialogueEvents()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
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
