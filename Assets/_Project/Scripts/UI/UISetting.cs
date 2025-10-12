using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetting : PanelBase
{
    private void OnEnable()
    {
        PlayerInputManager.Instance.OnEseActionPerformed += OnEseActionPerformed;
    }

    private void OnDestroy()
    {
        PlayerInputManager.Instance.OnEseActionPerformed -= OnEseActionPerformed;
    }

    private void OnEseActionPerformed()
    {
        if(UIManager.Instance.IsLastUI("Setting"))
        {
            UIManager.Instance.HideUI("Setting");
        }
    }
}
