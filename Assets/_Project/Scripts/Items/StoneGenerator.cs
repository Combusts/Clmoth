using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class StoneGenerator : MonoBehaviour
{
    [Header("Stone Prefab")]
    [SerializeField] private GameObject stonePrefab;

    [Header("Stone Sprite List")]
    [SerializeField] private List<Sprite> stoneSprites;

    [Header("Generation Settings")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int generateCount = 1;
    [SerializeField] private Vector3 randomOffset = Vector3.zero;
    [SerializeField] private bool randomRotation = false;
    [SerializeField] private float maxDelay = 1f;
    [SerializeField] private float minDelay = 0f;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateStones();
        }
    }

    /// <summary>
    /// 生成石头
    /// </summary>
    public void GenerateStones()
    {
        // 如果没有指定 prefab，尝试从 Resources 加载
        if (stonePrefab == null)
        {
            stonePrefab = Resources.Load<GameObject>("Prefabs/Environment/Stone");
            if (stonePrefab == null)
            {
                Debug.LogError("Stone prefab not found! Please assign it in the inspector or place it in Resources/Prefabs/Environment/");
                return;
            }
        }

        // 启动生成协程
        StartCoroutine(GenerateStonesCoroutine());
    }

    /// <summary>
    /// 生成石头的协程
    /// </summary>
    private IEnumerator GenerateStonesCoroutine()
    {
        // 生成指定数量的石头,随机选择一个石头精灵
        for (int i = 0; i < generateCount; i++)
        {
            Sprite randomSprite = stoneSprites[Random.Range(0, stoneSprites.Count)];
            GenerateStone(randomSprite);
            
            // 等待随机时间后再生成下一个石头
            if (i < generateCount - 1) // 最后一个不需要等待
            {
                yield return new WaitForSeconds(GetRandomDelay());
            }
        }
    }

    /// <summary>
    /// 生成单个石头
    /// </summary>
    private void GenerateStone(Sprite sprite)
    {
        if (stonePrefab == null) return;

        // 计算生成位置（当前位置 + 随机偏移）
        Vector3 position = transform.position + GetRandomOffset();

        // 实例化石头
        GameObject stone = Instantiate(stonePrefab, position, GetRandomRotation());

        // 设置石头精灵
        stone.GetComponent<SpriteRenderer>().sprite = sprite;

        // 可选：将石头设置为当前对象的子对象
        // stone.transform.SetParent(transform);

        Debug.Log($"Generated stone at position: {position}");
    }

    /// <summary>
    /// 获取随机偏移
    /// </summary>
    private Vector3 GetRandomOffset()
    {
        if (randomOffset == Vector3.zero)
            return Vector3.zero;

        return new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );
    }

    /// <summary>
    /// 获取随机生成延迟
    /// </summary>
    private float GetRandomDelay()
    {
        return Random.Range(minDelay, maxDelay);
    }

    /// <summary>
    /// 获取随机旋转
    /// </summary>
    private Quaternion GetRandomRotation()
    {
        if (randomRotation)
        {
            return Quaternion.Euler(0, 0, Random.Range(0, 360));
        }
        return Quaternion.identity;
    }
}

