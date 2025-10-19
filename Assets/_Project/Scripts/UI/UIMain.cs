using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : PanelBase
{
    public void OnStartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void OnExitGame()
    {
        Application.Quit();

    }
    public void OnSetting()
    {
        UIManager.Instance.ShowUI("Setting");
    }
}
