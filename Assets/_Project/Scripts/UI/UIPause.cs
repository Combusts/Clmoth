using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : PanelBase
{
    public void OnResume(){
        UIManager.Instance.HideUI("Pause");
        UIManager.Instance.ShowUI("Playing");
        GameManager.Instance.ResumeGame();
    }

    public void OnSetting(){
        UIManager.Instance.ShowUI("Setting");
    }

    public void OnMainMenu(){
        GameManager.Instance.ResumeGame();
        GameManager.Instance.ToLevel("Main");
    }
}
