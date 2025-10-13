using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    bool IsShowHint { get; set; }
    bool CanInteract { get; set; }
    public abstract void Interact(); 

    public abstract void ShowHint();

    public abstract void HideHint();
}
