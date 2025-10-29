using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    // 单例
    public static GameManager Instance { get; private set; }

    public Dictionary<string, int> levelDic = new();  

    public Action OnStartGame;

    #region Debug Tools - Level Switching
    [Header("Debug Settings - Scene Switching")]
    [Tooltip("输入场景名称或索引来切换场景（留空则保持原样）")]
    [SerializeField] private string debugSwitchToScene = "";
    
    private string lastProcessedValue = "";
    #endregion

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

        // 初始化场景字典
        levelDic["Main"] = 0;
        levelDic["Level_01"] = 1;
        levelDic["Level_01_01"] = 1;
        levelDic["Game"] = 2;
        levelDic["MiniGame"] = 2;
        levelDic["FinalLoop"] = 3;
        levelDic["Ending"] = 4;

        SceneManager.sceneLoaded += (scene, mode)=>{
            Debug.Log($"Scene Loaded: {scene.name}");
            
            // 切换场景时清除玩家位置存档
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ClearPlayerPosition();
            }
            
            // 初始化UI
            
            UIManager.Instance.HideAllUI();

            if (scene.name == "Main")
            {
                UIManager.Instance.ShowUI("Main");
            } 
            else if (scene.name == "Level_01" || scene.name == "Level_01_01")
            {
                SetCameraPositionAfterSceneLoad();
                InitializeCommandManager();
                UIManager.Instance.ShowUI("Playing");
                UIManager.Instance.ShowUI("CinematicBars");
            } 
            else if (scene.name == "Game"){
                UIManager.Instance.ShowUI("Playing");
            }

            if (scene.name == "FinalLoop")
            {
                UIManager.Instance.ShowUI("Playing");
                UIManager.Instance.ShowUI("CinematicBars");
                YarnSpinnerManager.Instance.StartDialogueSafe("FinalLoop", true);
            }

            if (scene.name == "Ending")
            {
                // 禁用Player的摄像机跟随，避免覆盖设置的摄像机位置
                StartCoroutine(DisablePlayerCameraFollow());
                UIManager.Instance.ShowUI("Playing");
                UIManager.Instance.ShowUI("CinematicBars");
                // 进入结局对话
                YarnSpinnerManager.Instance.StartDialogueSafe("Ending", true);
            }
        };
    }

    void Start()
    {
        UIManager.Instance.ShowUI("Main");
        
        // 初始化存档系统
        InitializeSaveSystem();
    }
    
    /// <summary>
    /// 初始化存档系统
    /// </summary>
    private void InitializeSaveSystem()
    {
        // 确保SaveManager存在
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerGO = new GameObject("SaveManager");
            saveManagerGO.AddComponent<SaveManager>();
            DontDestroyOnLoad(saveManagerGO);
        }
        
        Debug.Log("[GameManager] Save system initialized");
    }
    
    /// <summary>
    /// 初始化命令管理器
    /// </summary>
    private void InitializeCommandManager()
    {
        // 检查场景中是否已有CommandManager
        GameObject existingCommandManager = GameObject.Find("CommandManager");
        if (existingCommandManager == null)
        {
            // 创建CommandManager
            GameObject commandManagerGO = new GameObject("CommandManager");
            
            // 添加GameObjectCommands组件
            commandManagerGO.AddComponent<GameObjectCommands>();
            
            // 添加InteractiveCommands组件
            commandManagerGO.AddComponent<InteractiveCommands>();
            
            Debug.Log("[GameManager] CommandManager created with GameObjectCommands and InteractiveCommands");
        }
        else
        {
            Debug.Log("[GameManager] CommandManager already exists in scene");
        }
        
        // 检查Start节点是否完成，如果未完成则自动开始对话
        StartCoroutine(CheckAndStartDialogueIfNeeded());
    }
    
    /// <summary>
    /// 检查Start节点是否完成，如果未完成则自动开始对话
    /// </summary>
    private IEnumerator CheckAndStartDialogueIfNeeded()
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        // 检查Start节点是否已完成
        if (SaveManager.Instance != null && YarnSpinnerManager.Instance != null)
        {
            // 使用YarnSpinnerManager的StartDialogueSafe方法，它会自动检查存档状态
            Debug.Log("[GameManager] Checking Start dialogue completion status...");
            YarnSpinnerManager.Instance.StartDialogueSafe("Start", true);
        }
        else
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[GameManager] SaveManager.Instance is null! Cannot check dialogue completion status.");
            }
            if (YarnSpinnerManager.Instance == null)
            {
                Debug.LogWarning("[GameManager] YarnSpinnerManager.Instance is null! Cannot start dialogue.");
            }
        }
    }

    public void StartGame()
    {
        OnStartGame?.Invoke();
        SceneTransitionManager.Instance.LoadSceneWithFade(levelDic["Level_01"]);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    [YarnCommand("ToLevel")]
    public void ToLevel(string levelName){
        SceneTransitionManager.Instance.LoadSceneWithFade(levelDic[levelName]);
    }

    private void SetCameraPositionAfterSceneLoad()
    {
        // 等待一帧确保UI已经创建
        StartCoroutine(SetCameraPositionCoroutine());
    }

    private IEnumerator SetCameraPositionCoroutine()
    {
        // 等待一帧
        yield return null;
        
        // 通过UIManager获取UICinematicBars并设置相机位置
        if (UIManager.Instance != null)
        {
            // 尝试获取UICinematicBars组件
            var cinematicBars = FindObjectOfType<UICinematicBars>();
            if (cinematicBars != null)
            {
                // 已取消相机位置更改
                // cinematicBars.SetCameraPosition(); // 不再更改相机位置
                Debug.Log("Cinematic bars found after scene load (camera position unchanged)");
            }
            else
            {
                Debug.LogWarning("UICinematicBars not found after scene load");
            }
        }
    }

    /// <summary>
    /// 禁用Player的摄像机跟随（用于Ending等场景）
    /// </summary>
    private IEnumerator DisablePlayerCameraFollow()
    {
        // 等待一帧确保Player已初始化
        yield return null;
        
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.DisableCameraFollow();
            Debug.Log("[GameManager] Player camera follow disabled for Ending scene");
        }
        else
        {
            Debug.LogWarning("[GameManager] Player not found in Ending scene");
        }
    }

    #region Debug Tools Implementation

    /// <summary>
    /// 当 Inspector 中的值发生变化时调用（仅在 Editor 中）
    /// </summary>
    private void OnValidate()
    {
        // 只在 Editor 中且运行时执行
        #if UNITY_EDITOR
        if (Application.isPlaying && !string.IsNullOrWhiteSpace(debugSwitchToScene) && debugSwitchToScene != lastProcessedValue)
        {
            string sceneToSwitch = debugSwitchToScene.Trim();
            Debug.Log($"[Debug] Scene switch request detected: {sceneToSwitch}");
            
            // 标记为已处理，防止重复触发
            lastProcessedValue = sceneToSwitch;
            
            // 使用协程延迟执行，确保在下一帧处理，并清空字段
            StartCoroutine(ProcessSceneSwitchDelayed(sceneToSwitch));
        }
        // 如果字段被清空，重置处理标记
        else if (string.IsNullOrWhiteSpace(debugSwitchToScene) && !string.IsNullOrEmpty(lastProcessedValue))
        {
            lastProcessedValue = "";
        }
        #endif
    }

    /// <summary>
    /// 延迟处理场景切换（协程）
    /// </summary>
    private IEnumerator ProcessSceneSwitchDelayed(string sceneName)
    {
        // 等待一帧，确保 Inspector 更新完成
        yield return null;
        
        // 执行场景切换
        HandleDebugSceneSwitch(sceneName);
        
        // 清空字段
        debugSwitchToScene = "";
        
        // 重置处理标记，允许下次输入
        lastProcessedValue = "";
    }

    /// <summary>
    /// 处理调试场景切换
    /// </summary>
    /// <param name="sceneInput">场景名称或索引（字符串格式）</param>
    private void HandleDebugSceneSwitch(string sceneInput)
    {
        if (string.IsNullOrWhiteSpace(sceneInput))
        {
            return;
        }

        sceneInput = sceneInput.Trim();

        // 尝试解析为场景索引（数字）
        if (int.TryParse(sceneInput, out int sceneIndex))
        {
            SwitchToSceneByIndex(sceneIndex);
            return;
        }

        // 尝试作为场景名称查找
        if (levelDic.ContainsKey(sceneInput))
        {
            int index = levelDic[sceneInput];
            SwitchToSceneByIndex(index);
            return;
        }

        // 如果都没找到，输出错误信息
        Debug.LogError($"[Debug] Scene '{sceneInput}' not found. Available scenes:");
        foreach (var kvp in levelDic)
        {
            Debug.Log($"  - {kvp.Key} (Index: {kvp.Value})");
        }
        Debug.Log($"Or use scene index: 0-{SceneManager.sceneCountInBuildSettings - 1}");
    }

    /// <summary>
    /// 通过索引切换场景
    /// </summary>
    /// <param name="sceneIndex">场景索引</param>
    private void SwitchToSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[Debug] Invalid scene index: {sceneIndex}. Available range: 0-{SceneManager.sceneCountInBuildSettings - 1}");
            return;
        }

        string sceneName = GetSceneNameByIndex(sceneIndex);
        Debug.Log($"[Debug] Switching to scene [{sceneIndex}] {sceneName}");

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(sceneIndex);
        }
        else
        {
            Debug.LogWarning("[Debug] SceneTransitionManager not found, using direct scene load");
            SceneManager.LoadScene(sceneIndex);
        }
    }

    /// <summary>
    /// 根据场景索引获取场景名称
    /// </summary>
    private string GetSceneNameByIndex(int sceneIndex)
    {
        // 从 levelDic 中查找
        foreach (var kvp in levelDic)
        {
            if (kvp.Value == sceneIndex)
            {
                return kvp.Key;
            }
        }
        return $"Scene_{sceneIndex}";
    }

    #endregion
}
