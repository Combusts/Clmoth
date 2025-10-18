using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
        levelDic["Game"] = 1;

        // 初始化UI
        SceneManager.sceneLoaded += (scene, mode)=>{
            if (scene.name == "Main")
            {
                UIManager.Instance.HideAllUI();
                UIManager.Instance.ShowUI("Main");
            } 
            else if (scene.name == "Game")
            {
                UIManager.Instance.HideAllUI();
                UIManager.Instance.ShowUI("Playing");
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
        SceneTransitionManager.Instance.LoadSceneWithFade("Game");
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
        Debug.Log($"ToLevel: {levelName}");
        SceneTransitionManager.Instance.LoadSceneWithFade(levelName);
    }
}
