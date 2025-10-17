using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [Header("移动状态")]
    [SerializeField] private bool onMove = false;
    
    private Animator animator;
    private bool lastOnMoveValue;
    
    public bool OnMove
    {
        get => onMove;
        set
        {
            onMove = value;
            UpdateAnimator();
        }
    }
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        lastOnMoveValue = onMove;
        UpdateAnimator();
    }
    
    private void Update()
    {
        // 检查onMove值是否在运行时被改变
        if (onMove != lastOnMoveValue)
        {
            UpdateAnimator();
            lastOnMoveValue = onMove;
        }
    }
    
    private void OnValidate()
    {
        UpdateAnimator();
    }
    
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("OnMove", onMove);
        }
    }
}
