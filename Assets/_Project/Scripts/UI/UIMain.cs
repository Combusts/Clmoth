using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class UIMain : PanelBase
{
    void OnEnable()
    {   
        // 获取激活的相机，获取VideoPlayer组件，将VideoPlayer组件赋值给targetCamera变量
        VideoPlayer videoPlayer = GetComponentInChildren<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.targetCamera = Camera.main;
        }
    }

    public void OnStartGame()
    {
        AudioManager.Instance.PlayAudio("Click");
        GameManager.Instance.StartGame();
    }

    public void OnExitGame()
    {
        AudioManager.Instance.PlayAudio("Click");   
        Application.Quit();

    }
    public void OnSetting()
    {
        AudioManager.Instance.PlayAudio("Click");   
        UIManager.Instance.ShowUI("Setting");
    }
}
