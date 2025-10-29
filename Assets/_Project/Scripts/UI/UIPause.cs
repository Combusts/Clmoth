using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : PanelBase
{
    public void OnResume(){
        AudioManager.Instance.PlayAudio("Click");
        UIManager.Instance.HideUI("Pause");
        UIManager.Instance.ShowUI("Playing");
        GameManager.Instance.ResumeGame();
    }

    public void OnSetting(){
        AudioManager.Instance.PlayAudio("Click");
        UIManager.Instance.ShowUI("Setting");
        UIManager.Instance.HideUI("Pause");
    }

    public void OnMainMenu(){
        AudioManager.Instance.PlayAudio("Click");
        GameManager.Instance.ResumeGame();
        YarnSpinnerManager.Instance.StopDialogue();

        GameObject LeftIllustration = GameObject.Find("Left Illustration");
        if (LeftIllustration != null)
        {
            LeftIllustration.SetActive(false);
        }
        GameObject RightIllustration = GameObject.Find("Right Illustration");
        if (RightIllustration != null)
        {
            RightIllustration.SetActive(false);
        }
        GameManager.Instance.ToLevel("Main");
    }
}
