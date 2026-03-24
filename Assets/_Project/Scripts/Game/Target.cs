using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Target : ColoredItem
{
    [Header("移动属性")]
    [SerializeField]
    private float maxMoveSpeed = 2f;

    [SerializeField]
    private float minMoveSpeed = 4f;
    private float moveSpeed;
    [SerializeField]
    private float upperBound = 0;
    [SerializeField]
    private float lowerBound = -4;

    [Header("耐心属性")]
    [SerializeField]
    private int patience = 4;

    [SerializeField]
    private GameObject patienceIcon_1;
    [SerializeField]
    private GameObject patienceIcon_2;
    [SerializeField]
    private GameObject patienceIcon_3;



    int moveDirection = 1;
    private new void Start()
    {
        base.Start();

        // 随机移动速度
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // 随机颜色
        // color = GetRandomEnumColor();
        // spriteRenderer.color = GetColor(color);

        // 初始化耐心图标
        patienceIcon_1.SetActive(false);
        patienceIcon_2.SetActive(false);
        patienceIcon_3.SetActive(false);

        // 调试用，显示耐心值
        // TextMeshProUGUI patienceText = GetComponentInChildren<TextMeshProUGUI>();
        // patienceText.text = patience.ToString();

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
                AudioManager.Instance.PlayAudio("Correct");
                GameBase.Instance.RemoveEnemyCount();
                GameBase.Instance.CheckGameWin();
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Destroy(collision.gameObject);
                patience--;
                AudioManager.Instance.PlayAudio("Wrong");

                // 更新耐心图标
                switch (patience)
                {
                    case 3:
                        patienceIcon_3.SetActive(true);
                        break;
                    case 2:
                        patienceIcon_2.SetActive(true);
                        break;
                    case 1:
                        patienceIcon_1.SetActive(true);
                        break;
                }
                // 调试用
                // TextMeshProUGUI patienceText = GetComponentInChildren<TextMeshProUGUI>();
                // patienceText.text = patience.ToString();

                if (patience <= 0)
                {
                    // 游戏结束
                    Destroy(gameObject);
                    GameBase.Instance.GameOver();
                }
            }
        }
    }
}
