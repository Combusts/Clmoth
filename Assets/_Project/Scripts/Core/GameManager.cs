using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += (scene, mode)=>{
            Debug.Log($"Scene Loaded: {scene.name}");
            
            // 如果是游戏场景，设置相机位置
            if (scene.name == "Level_01")
            {
                SetCameraPositionAfterSceneLoad();
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

        SceneManager.LoadScene(levelDic["Level_01"]);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public void ToLevel(string levelName){
        SceneManager.LoadScene(levelDic[levelName]);  
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
