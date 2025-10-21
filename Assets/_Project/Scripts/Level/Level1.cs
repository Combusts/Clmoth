using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Level1 : Level
{
    [SerializeField] private GameObject player;
    
    private Level1 Instance;
    private Vector2 playerPosition;

    int currentDialogueProcess = 0;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;    
    }

    public override void InitializeLevel(Vector2 playerPosition, int dialogueProgress)
    {
        // 初始化玩家位置
        if (player != null)
        {
            if(playerPosition != Vector2.zero){
                this.playerPosition = playerPosition;
            }
            player.transform.position = playerPosition;
        }

        // 初始化对话进度
        switch (dialogueProgress)
        {
            case 0:
                // 新游戏
                Debug.Log("Start Dialogue");
                YarnSpinnerManager.Instance.StartDialogue("Start");
                break;
            case 1:
                // 已经完成初始对话
                break;
            case 2:
                // 完成小游戏
                Debug.Log("complete mini game");
                break;
            default:
                // 其他情况，默认处理
                break;
        }
    }

    [YarnCommand("SetDialogueProcess")]
    public void SetDialogueProcess(int process)
    {
        currentDialogueProcess = process;
        SaveLevelData();
    }

    public void SaveLevelData()
    {
        GameManager.SetGameProcess(1, player.transform.position, currentDialogueProcess);
    }
}
