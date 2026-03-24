using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] bool autoOpen = false;

    [SerializeField] GameObject doorCollider;

    [SerializeField] GameObject shelter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测是否为Player进入
        if (collision.CompareTag("Player") && autoOpen)
        {
            OpenDoor();
        }
    }
    
    
    public void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");

        if (doorCollider != null)
        {
            doorCollider.GetComponent<BoxCollider2D>().enabled = false;
        }
        if (shelter != null)
        {
            shelter.SetActive(false);
        }   
    }

    public void CloseDoor()
    {
        animator.SetTrigger("CloseDoor");
    }
}
