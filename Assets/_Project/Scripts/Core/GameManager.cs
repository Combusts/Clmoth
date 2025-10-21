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

    public static Vector2 playerPosition;

    public static int level;

    public static int dialogueProcess;

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
        levelDic["Game"] = 2;

        SceneManager.sceneLoaded += (scene, mode)=>{
            Debug.Log($"Scene Loaded: {scene.name}");
            
            // 如果是游戏场景，设置相机位置
            if (scene.name == "Level_01")
            {
                SetCameraPositionAfterSceneLoad();
                UIManager.Instance.ShowUI("Playing");
                UIManager.Instance.ShowUI("CinematicBars");
            }
        };
    }

    void Start()
    {
        UIManager.Instance.ShowUI("Main");
    }

    public void StartGame()
    {
        OnStartGame?.Invoke();

        ToLevel("Level_01", Vector2.zero, 0);
    }

    public void PauseGame()
    {
        GameObject dialogueSystemVariant = GameObject.Find("Dialogue System Variant");
        if (dialogueSystemVariant != null)
        {
            dialogueSystemVariant.transform.Find("Line Advancer").gameObject.SetActive(false);
        }else
        {
            Debug.LogError("Dialogue System Variant not found");
        }
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        GameObject dialogueSystemVariant = GameObject.Find("Dialogue System Variant");
        if (dialogueSystemVariant != null)
        {
            dialogueSystemVariant.transform.Find("Line Advancer").gameObject.SetActive(true);
        }else
        {
            Debug.LogError("Dialogue System Variant not found");
        }
        Time.timeScale = 1;
    }

    public void ToLevel(string levelName, Vector2 playerPostion, int dialogueProcess){
        SceneTransitionManager.Instance.LoadSceneWithFade(levelDic[levelName]);  

        SceneTransitionManager.Instance.OnSceneLoaded += () =>
        {
            // 初始化场景
            Level level = FindObjectOfType<Level>();
            if (level != null)
            {
                Debug.Log($"场景初始化: playerPostion={playerPostion}, dialogueProcess={dialogueProcess}");
                level.InitializeLevel(playerPostion, dialogueProcess);
            } else 
            {
                Debug.Log($"Level component not found in scene {levelName}");
            }
        };


    }

    [YarnCommand("PlayGame")]
    public void PlayGame()
    {
        ToLevel("Game", Vector2.zero, 0);
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

    public static void SetGameProcess(int level, Vector2 playerPosition, int process)
    {
        Debug.Log($"SetGameProcess: level={level}, playerPosition={playerPosition}, process={process}");
        GameManager.level = level;
        GameManager.playerPosition = playerPosition;
        GameManager.dialogueProcess = process;
    }
}
