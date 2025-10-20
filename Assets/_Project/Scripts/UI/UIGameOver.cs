using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : PanelBase
{
    public void OnMainButtonClick(){
        Time.timeScale = 1;
        GameManager.Instance.ToLevel("Main");
    }
}
