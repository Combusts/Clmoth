using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    private List<IInteractive> interactiveObjects = new();

    private IInteractive closestInteractive = null;
    private float direction = 0f;

    void OnEnable()
    {
        // 确保 PlayerInputManager 已经初始化
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed += Interact;
        }
        else
        {
            // 如果 PlayerInputManager 还没准备好，延迟订阅
            StartCoroutine(SubscribeToInputManagerWhenReady());
        }
    }

    void Update()
    {
        transform.Translate(speed * Time.deltaTime * new Vector3(direction, 0, 0));

        UpdateClosestInteractive();
    }

    void OnDisable()
    {
        // 安全地取消订阅
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnMoveActionPerformed -= Moveperformed;
            PlayerInputManager.Instance.OnMoveActionCanceled -= Movecanceled;
            PlayerInputManager.Instance.OnInteractActionPerformed -= Interact;
        }
    }

    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        Debug.Log("OnTriggerEnter");
        if (collision.TryGetComponent(out IInteractive interactiveObject) && interactiveObject.CanInteract)
        {
            interactiveObjects.Add(interactiveObject);
        }
    }
    public void OnTriggerExit2D(UnityEngine.Collider2D collision)
    {
        Debug.Log("OnTriggerExit");
        if (collision.TryGetComponent(out IInteractive interactiveObject))
        {
            interactiveObject.HideHint();
            interactiveObjects.Remove(interactiveObject);
        }
    }

    void Moveperformed(float direction)
    {
        this.direction = direction;
    }

    void Movecanceled(float direction)
    {
        this.direction = 0f;
    }

    // 等待 PlayerInputManager 准备就绪的协程
    private IEnumerator SubscribeToInputManagerWhenReady()
    {
        // 等待直到 PlayerInputManager.Instance 不为空
        while (PlayerInputManager.Instance == null)
        {
            yield return null; // 等待一帧
        }
        
        // 现在可以安全地订阅事件
        PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
        PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
        PlayerInputManager.Instance.OnInteractActionPerformed += Interact;
    }


    private IInteractive FindClosestInteractive()
    {
        if (interactiveObjects.Count == 0) return null;

        IInteractive closestInteractive = interactiveObjects[0];
        float closestDistance = Vector3.Distance(transform.position, ((MonoBehaviour)closestInteractive).transform.position);

        for (int i = 1; i < interactiveObjects.Count; i++)
        {
            IInteractive currentInteractive = interactiveObjects[i];

            float currentDistance = Vector3.Distance(transform.position, ((MonoBehaviour)currentInteractive).transform.position);

            if (currentDistance < closestDistance)
            {
                closestInteractive = currentInteractive;
                closestDistance = currentDistance;
            }
        }

        return closestInteractive;
    }

    private void UpdateClosestInteractive()
    {
        closestInteractive = FindClosestInteractive();
        
        foreach (IInteractive interactiveObject in interactiveObjects)
        {
            if (interactiveObject != closestInteractive)
            {
                interactiveObject.HideHint();
            }
            else
            {
                interactiveObject.ShowHint();
            }
        }
    }

    public void Interact()
    {
        if (closestInteractive != null)
        {
            closestInteractive.Interact();
            closestInteractive.HideHint();
            interactiveObjects.Remove(closestInteractive);

            if (closestInteractive.CanInteract)
            {
                interactiveObjects.Add(closestInteractive);
            }

            closestInteractive = null;
        }
    }
}
