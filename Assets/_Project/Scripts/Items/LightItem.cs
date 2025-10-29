using UnityEngine;

public class LightItem : MonoBehaviour
{
    public float lightRadius = 1;
    public float lightIntensity = 1;
    public Color lightColor = Color.white;
    
    // 自动注册管理
    private static LightManager2D lightManager;
    
    private void OnEnable()
    {
        // 获取或创建 LightManager
        if (lightManager == null)
        {
            lightManager = FindObjectOfType<LightManager2D>();
            
            if (lightManager == null)
            {
                GameObject managerObj = new("LightManager");
                lightManager = managerObj.AddComponent<LightManager2D>();
            }
        }
        
    }

    private void Start()
    {
        if (lightManager != null)
        {
            lightManager.AddLight(this);
            
        }
        else
        {
            Debug.LogError("LightManager not found");
        }
    }

    private void OnValidate()
    {
        if (lightManager != null)
        {
            lightManager.UpdateLight(this);
        }
    }

    private void Update()
    {
        if (lightManager != null)
        {
            lightManager.UpdateLight(this);
        }
        else
        {
            Debug.LogError("LightManager not found");
        }
    }
    
    private void OnDestroy()
    {
        if (lightManager != null)
            lightManager.RemoveLight(this);
        else
            Debug.LogError("LightManager not found");
    }
    
}