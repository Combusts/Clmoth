using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : InteractiveWithActions
{
    private Renderer _renderer;
    
    protected override void Awake()
    {
        base.Awake();
        _renderer = GetComponent<Renderer>();
    }
    
    protected override void StoreOriginalVisualState()
    {
        if (_renderer != null)
        {
            originalColor = _renderer.material.color;
        }
    }
    
    protected override void SetVisualState(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }
    
    public override void ShowHint()
    {
        if (IsShowHint) return;
        IsShowHint = true;
        
        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
        }
    }
    
    public override void HideHint()
    {
        if (!IsShowHint) return;
        IsShowHint = false;
        
        if (_renderer != null)
        {
            _renderer.material.color = originalColor;
        }
    }
}
