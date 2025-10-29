using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameExplain : PanelBase
{
    private void OnEnable()
    {
        SceneTransitionManager.Instance.OnSceneLoadComplete += () =>
        {
            GameManager.Instance.PauseGame();
        };
    }

    public void OnCloseButtonClick()
    {
        GameManager.Instance.ResumeGame();
        UIManager.Instance.HideUI("GameExplain");
        UIManager.Instance.ShowUI("Playing");
    }
}
