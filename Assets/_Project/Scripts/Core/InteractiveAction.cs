using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InteractiveAction
{
    [Header("Action Configuration")]
    public string actionName;
    
    [Header("Action Execution")]
    public UnityEvent onExecute;
}
