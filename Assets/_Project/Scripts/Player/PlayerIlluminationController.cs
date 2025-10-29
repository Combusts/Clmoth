using UnityEngine;

public class PlayerIlluminationController : MonoBehaviour
{
    [Header("材质设置")]
    [SerializeField] private Material targetMaterial;
    
    [Header("玩家引用")]
    [SerializeField] private Player player;
    
    [Header("强度控制")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 100f;
    
    [Header("位置范围")]
    [SerializeField] private float minPlayerX = 0f;
    [SerializeField] private float maxPlayerX = 100f;
    
    [Header("调试")]
    [SerializeField] private bool useRelativeX = false; // 是否使用相对位置（从0开始）
    
    private static readonly int IlluminationIntensityProperty = Shader.PropertyToID("_IlluminationIntensity");
    
    void Start()
    {
        // 如果没有指定Player，尝试自动查找
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            if (player == null)
            {
                Debug.LogWarning($"[PlayerIlluminationController] Player not found. Please assign Player reference in Inspector on {gameObject.name}");
                enabled = false;
                return;
            }
        }
        
        // 如果没有指定材质，尝试从当前GameObject获取共享材质
        if (targetMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // 直接使用共享材质以影响整个场景
                targetMaterial = renderer.sharedMaterial;
            }
            else
            {
                Debug.LogWarning($"[PlayerIlluminationController] No material assigned. Please assign material in Inspector on {gameObject.name}");
                enabled = false;
                return;
            }
        }
        
        // 验证shader属性是否存在
        if (!targetMaterial.HasProperty(IlluminationIntensityProperty))
        {
            Debug.LogWarning($"[PlayerIlluminationController] Material '{targetMaterial.name}' does not have '_IlluminationIntensity' property. Script disabled on {gameObject.name}");
            enabled = false;
            return;
        }
        
        // 初始化强度值
        UpdateIlluminationIntensity();
    }
    
    void Update()
    {
        if (player == null || targetMaterial == null)
        {
            return;
        }
        
        UpdateIlluminationIntensity();
    }
    
    private void UpdateIlluminationIntensity()
    {
        if (player == null || targetMaterial == null)
        {
            return;
        }
        
        // 获取玩家X位置
        float playerX = player.transform.position.x;
        
        // 如果使用相对位置，将第一个位置作为0点
        if (useRelativeX && Application.isPlaying)
        {
            // 在第一次运行时记录初始位置
            if (Mathf.Approximately(minPlayerX, 0f) && Mathf.Approximately(maxPlayerX, 100f))
            {
                // 使用初始位置作为参考点
                playerX -= player.transform.position.x; // 实际会在Start后记录
            }
        }
        
        // 将玩家X位置映射到强度范围
        float normalizedX = Mathf.InverseLerp(minPlayerX, maxPlayerX, playerX);
        
        // 限制在0-1范围内，防止超出范围
        normalizedX = Mathf.Clamp01(normalizedX);
        
        // 计算目标强度
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedX);
        
        // 应用强度到共享材质（会影响整个场景中所有使用该材质的对象）
        targetMaterial.SetFloat(IlluminationIntensityProperty, targetIntensity);
    }
    
    /// <summary>
    /// 设置材质引用（直接使用共享材质）
    /// </summary>
    public void SetTargetMaterial(Material material)
    {
        if (material != null)
        {
            targetMaterial = material;
            
            // 验证shader属性
            if (!targetMaterial.HasProperty(IlluminationIntensityProperty))
            {
                Debug.LogWarning($"[PlayerIlluminationController] Material '{targetMaterial.name}' does not have '_IlluminationIntensity' property.");
                enabled = false;
                return;
            }
            
            enabled = true;
            UpdateIlluminationIntensity();
        }
    }
    
    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Player playerReference)
    {
        player = playerReference;
        if (player == null)
        {
            Debug.LogWarning($"[PlayerIlluminationController] Player reference is null on {gameObject.name}");
        }
    }
}

