using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private float direction = 0f;

    void OnEnable()
    {
        PlayerInputManager.Instance.OnMoveActionPerformed += Moveperformed;
        PlayerInputManager.Instance.OnMoveActionCanceled += Movecanceled;
    }

    void Update()
    {
        transform.Translate(new Vector3(direction, 0, 0) * speed * Time.deltaTime);
    }

    void OnDisable()
    {
        PlayerInputManager.Instance.OnMoveActionPerformed -= Moveperformed;
        PlayerInputManager.Instance.OnMoveActionCanceled -= Movecanceled;
    }

    void Moveperformed(float direction)
    {
        this.direction = direction;
    }

    void Movecanceled(float direction)
    {
        this.direction = 0f;
    }
}
