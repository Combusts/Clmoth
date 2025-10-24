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
        levelDic["MiniGame"] = 2;

        SceneManager.sceneLoaded += (scene, mode)=>{
            Debug.Log($"Scene Loaded: {scene.name}");
            
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
            } 
            else if (scene.name == "Game"){
                UIManager.Instance.ShowUI("Playing");
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
        SaveManager.Instance.DeleteSaveData();
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
                cinematicBars.SetCameraPosition();
                Debug.Log("Camera position set after scene load");
            }
            else
            {
                Debug.LogWarning("UICinematicBars not found after scene load");
            }
        }
    }
}
