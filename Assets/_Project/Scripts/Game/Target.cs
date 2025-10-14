using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Target : ColoredItem
{
    [SerializeField]
    private float maxMoveSpeed = 2f;

    [SerializeField]
    private float minMoveSpeed = 4f;
    private float moveSpeed;
    [SerializeField]
    private float upperBound = 0;
    [SerializeField]
    private float lowerBound = -4;

    [SerializeField]
    private int patience = 5;
    int moveDirection = 1;
    private new void Start()
    {
        base.Start();
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // 随机颜色
        // color = GetRandomEnumColor();
        // spriteRenderer.color = GetColor(color);

        TextMeshProUGUI patienceText = GetComponentInChildren<TextMeshProUGUI>();
        patienceText.text = patience.ToString();

        GameBase.Instance.AddEnemyCount();
    }
    void Update()
    {
        if (transform.position.y >= upperBound)
        {
            moveDirection = -1;
        }
        else if (transform.position.y <= lowerBound)
        {
            moveDirection = 1;
        }
        
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime * Vector3.up);
    }

    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<ColoredItem>(out var otherColoredItem))
        {
            if (otherColoredItem.color == color)
            {   
                GameBase.Instance.RemoveEnemyCount();
                GameBase.Instance.CheckGameWin();
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Destroy(collision.gameObject);
                patience--;
                TextMeshProUGUI patienceText = GetComponentInChildren<TextMeshProUGUI>();
                patienceText.text = patience.ToString();
                if (patience <= 0)
                {
                    // 游戏结束
                    Destroy(gameObject);
                    GameBase.Instance.GameOver();
                }
                // moveSpeed = 0;
            }
        }
    }

    
}
