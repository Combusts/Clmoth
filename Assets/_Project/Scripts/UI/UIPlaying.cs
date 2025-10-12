using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlaying : PanelBase
{
    public void OnPaused(){
        UIManager.Instance.HideAllUI();
        UIManager.Instance.ShowUI("Pause");
        GameManager.Instance.PauseGame();
    }
}
