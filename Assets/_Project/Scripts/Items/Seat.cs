using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour, IInteractive
{
    public bool IsShowHint { get; set; }
    public bool CanInteract { get; set; } = true;

    public void HideHint()
    {
        if (!IsShowHint) return;
        IsShowHint = false;

        // 获取渲染组件并将颜色设置为红色
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;     
        }
    }

    public void Interact()
    {
        Debug.Log("Interact Seat");
        // CanInteract = false;
    }

    public void ShowHint()
    {
        if (IsShowHint) return;
        IsShowHint = true;

        // 获取渲染组件并将颜色设置为红色
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }
}
