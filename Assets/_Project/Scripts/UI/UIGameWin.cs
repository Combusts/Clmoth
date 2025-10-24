using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameWin : PanelBase
{
    public void OnMainButtonClick(){
        Time.timeScale = 1;
        GameManager.Instance.ToLevel("Level_01");
    }
}
