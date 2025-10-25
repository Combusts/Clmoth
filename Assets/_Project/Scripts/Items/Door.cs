using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测是否为Player进入
        if (collision.CompareTag("Player"))
        {
            OpenDoor();
        }
    }
    
    
    private void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");
    }

    private void CloseDoor()
    {
        animator.SetTrigger("CloseDoor");
    }
}
