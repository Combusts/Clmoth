using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScence : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.HideAllUI();
        UIManager.Instance.ShowUI("Main");
    }
}
