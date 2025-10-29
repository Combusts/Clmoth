using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : PanelBase
{
    public void OnMainButtonClick(){
        Time.timeScale = 1;
        // 需要通过对话进度去判断跳转场景的状态
        AudioManager.Instance.PlayAudio("Click");
        // 在第一章玩完 
        GameManager.Instance.ToLevel("Level_01");
    }
}
