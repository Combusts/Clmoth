using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Level1 : Level
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject boss;

    [SerializeField] private GameObject tableWithDoc;

    [SerializeField] private GameObject tableWithoutDoc;
    
    private Level1 Instance;
    private Vector2 playerPosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void InitializeLevel()
    {
        Debug.Log("[Level1]" + String.Join(",", SaveManager.Instance.GetSaveData().dialogueProcessData.completedNodes));

        if(SaveManager.Instance.IsDialogueCompleted("Start") == false){
            // 新游戏
            Debug.Log("Start Dialogue");
            YarnSpinnerManager.Instance.StartDialogue("Start");
        } else if (SaveManager.Instance.IsDialogueCompleted("window") == false){
            // 第一章小游戏结束
            Debug.Log("Level1 Mini Game Completed");
            
            tableWithDoc.SetActive(true);
            tableWithoutDoc.SetActive(false);
            boss.SetActive(false);
        }
    }
}
