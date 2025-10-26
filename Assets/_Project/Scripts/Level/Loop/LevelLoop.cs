using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class LevelLoop : MonoBehaviour
{
    [SerializeField] private Door[] Doors;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject waterFountainBefore;
    [SerializeField] private GameObject waterFountainAfter;

    private int curLoopNum = 0;

    void Awake()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        }
    }

    [YarnCommand("OpenPasswordPanelWithLoop")]
    public void OpenPasswordPanelWithLoop(int loopNum)
    {
        curLoopNum = loopNum;
        
        UIManager.Instance.ShowUI("Password");
        UIPassword passwordPanel = UIManager.Instance.GetPanelByName("Password") as UIPassword;
        switch (loopNum)
        {
            case 0:
                passwordPanel.SetPassword(0,0,0,0);
                break;
            case 1:
                passwordPanel.SetPassword(1,0,3,0);
                break;
            case 2:
                passwordPanel.SetPassword(5,1,1,5);
                break;
            case 3:
                passwordPanel.SetPassword(1,4,6,7);
                break;
            case 4:
                passwordPanel.SetPassword(2,2,4,2);
                break;
            default:
                break;
        }
        passwordPanel.OnPasswordCorrect += OnPasswordCorrect;
    }

    [YarnCommand("OpenDoorOfLoop")]
    public void OpenDoorOfLoop(int loopNum)
    {
        switch (loopNum)
        {
            case 0:
                Doors[0].gameObject.GetComponent<BoxCollider2D>().enabled = false;
                Doors[0].OpenDoor();
                break;
            case 1:
                Doors[1].gameObject.GetComponent<BoxCollider2D>().enabled = false;
                Doors[1].OpenDoor();
                break;
            case 2:
                Doors[2].gameObject.GetComponent<BoxCollider2D>().enabled = false;  
                Doors[2].OpenDoor();
                break;
            case 3:
                Doors[3].gameObject.GetComponent<BoxCollider2D>().enabled = false;              
                Doors[3].OpenDoor();
                break;
            case 4:
                Doors[4].gameObject.GetComponent<BoxCollider2D>().enabled = false;
                Doors[4].OpenDoor();
                break;
            default:
                break;
        }
    }

    void OnPasswordCorrect()
    {   
        Debug.Log("[LevelLoop] 密码正确");
        UIManager.Instance.HideUI("Password");
        dialogueRunner.VariableStorage.SetValue($"$Loop{curLoopNum}RightPassw", true);
        YarnSpinnerManager.Instance.StartDialogue($"Loop{curLoopNum}FileSender"); //Loop0FileSender
    }

    [YarnCommand("WaterFountainChange")]
    public void WaterFountainChange()
    {
        if (waterFountainBefore == null){
            Debug.LogError("[LevelLoop] 未分配水 FountainBefore");
            return;
        }
        if (waterFountainAfter == null){
            Debug.LogError("[LevelLoop] 未分配水 FountainAfter");
            return;
        }

        UIManager.Instance.DoInDark(() =>
        {
            waterFountainBefore.SetActive(false);
            waterFountainAfter.SetActive(true);
        }, 0.5f, 0.5f);
    }
}
