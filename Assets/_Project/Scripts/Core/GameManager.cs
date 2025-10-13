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
}
