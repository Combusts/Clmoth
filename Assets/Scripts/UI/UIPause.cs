using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : PanelBase
{
    public void OnResume(){
        UIManager.Instance.HideAllUI();
        UIManager.Instance.ShowUI("Playing");
        GameManager.Instance.ResumeGame();
    }

    public void OnSetting(){
        UIManager.Instance.ShowUI("Setting");
    }

    public void OnMainMenu(){
        UIManager.Instance.HideAllUI();
        UIManager.Instance.ShowUI("Main");
        GameManager.Instance.ResumeGame();
    }
}
