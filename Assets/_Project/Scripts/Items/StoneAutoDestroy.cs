using UnityEngine;

/// <summary>
/// 石头自动销毁 - 当石头离摄像机超过指定距离时自动销毁
/// </summary>
public class StoneAutoDestroy : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool useMainCameraIfNull = true;
    
    [Header("Destroy Settings")]
    [SerializeField] private float destroyDistance = 20f; // 销毁距离（单位）
    [SerializeField] private float checkInterval = 0.5f; // 检查间隔（秒），用于性能优化
    
    private float sqrDestroyDistance; // 平方距离，用于性能优化
    private float lastCheckTime = 0f;
    
    private void Start()
    {
        // 计算平方距离，避免每次都开平方根
        sqrDestroyDistance = destroyDistance * destroyDistance;
        
        // 如果没有指定摄像机，尝试获取主摄像机
        if (targetCamera == null && useMainCameraIfNull)
        {
            targetCamera = Camera.main;
        }
        
        // 如果还是没有摄像机，输出错误
        if (targetCamera == null)
        {
            Debug.LogError($"[StoneAutoDestroy] 在 {gameObject.name} 上未找到摄像机！请分配摄像机或确保场景中有主摄像机。");
            enabled = false; // 禁用脚本
            return;
        }
    }
    
    private void Update()
    {
        // 性能优化：按间隔检查而不是每帧检查
        if (Time.time - lastCheckTime < checkInterval)
        {
            return;
        }
        
        lastCheckTime = Time.time;
        
        // 计算与摄像机的距离（使用平方距离优化性能）
        if (targetCamera != null)
        {
            float sqrDistance = (transform.position - targetCamera.transform.position).sqrMagnitude;
            
            // 如果距离超过阈值，销毁石头
            if (sqrDistance > sqrDestroyDistance)
            {
                Destroy(gameObject);
                Debug.Log($"[StoneAutoDestroy] 石头 {gameObject.name} 因距离摄像机过远而被销毁。距离: {Mathf.Sqrt(sqrDistance):F2}");
            }
        }
    }
    
    /// <summary>
    /// 设置销毁距离
    /// </summary>
    public void SetDestroyDistance(float distance)
    {
        destroyDistance = distance;
        sqrDestroyDistance = distance * distance;
    }
    
    /// <summary>
    /// 获取当前与摄像机的距离
    /// </summary>
    public float GetDistanceToCamera()
    {
        if (targetCamera == null)
        {
            return float.MaxValue;
        }
        
        return Vector3.Distance(transform.position, targetCamera.transform.position);
    }
}
