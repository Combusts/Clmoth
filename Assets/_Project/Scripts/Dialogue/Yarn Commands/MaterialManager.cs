using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[RequireComponent(typeof(RawImage))]
public class MaterialManager : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private List<Material> materials = new List<Material>();

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError($"MaterialManager: RawImage component not found on {gameObject.name}!", this);
        }
    }

    /// <summary>
    /// Yarn指令：通过索引设置RawImage组件的材质
    /// </summary>
    /// <param name="index">材质列表中的索引（从0开始）</param>
    [YarnCommand("setMaterial")]
    public void SetMaterial(int index)
    {
        if (rawImage == null)
        {
            Debug.LogError($"MaterialManager: RawImage component is null on {gameObject.name}!", this);
            return;
        }

        if (materials == null || materials.Count == 0)
        {
            Debug.LogError($"MaterialManager: Materials list is empty on {gameObject.name}!", this);
            return;
        }

        if (index < 0 || index >= materials.Count)
        {
            Debug.LogError($"MaterialManager: Index {index} is out of range. Material list has {materials.Count} items (valid range: 0-{materials.Count - 1}) on {gameObject.name}!", this);
            return;
        }

        Material material = materials[index];
        if (material == null)
        {
            Debug.LogError($"MaterialManager: Material at index {index} is null on {gameObject.name}!", this);
            return;
        }

        // 创建材质实例以避免修改原始材质资源
        Material materialInstance = new Material(material);
        rawImage.material = materialInstance;

        Debug.Log($"MaterialManager: Successfully set material at index {index} ({material.name}) on {gameObject.name}");
    }
}

