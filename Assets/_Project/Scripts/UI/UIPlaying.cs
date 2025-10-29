using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlaying : PanelBase
{
    public void OnPaused(){
        AudioManager.Instance.PlayAudio("Click");
        UIManager.Instance.HideUI("Playing");
        UIManager.Instance.ShowUI("Pause");
        GameManager.Instance.PauseGame();
    }
}
