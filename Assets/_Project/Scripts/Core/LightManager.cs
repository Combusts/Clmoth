using UnityEngine;
using System.Collections.Generic;

public class LightManager2D : MonoBehaviour
{
    private static readonly int LightCount = Shader.PropertyToID("_LightCount");
    private static readonly int LightPosArray = Shader.PropertyToID("_LightPosArray");
    private static readonly int LightColorArray = Shader.PropertyToID("_LightColorArray");
    private static readonly int LightRadiusArray = Shader.PropertyToID("_LightRadiusArray");

    // 保留旧的手动配置支持
    public Transform[] lights; // 光源物体
    public Color[] colors;     // 光颜色
    public float[] radii;      // 光半径
    public float[] intensities;// 光强度
    
    // 动态光源列表
    private readonly List<LightItem> dynamicLights = new();
    
    public void AddLight(LightItem lightItem)
    {
        if (!dynamicLights.Contains(lightItem))
        {
            dynamicLights.Add(lightItem);
        }
    }
    
    public void RemoveLight(LightItem lightItem)
    {
        dynamicLights.Remove(lightItem);
    }

    void Update()
    {
        // 优先使用动态光源列表
        List<LightItem> allLights = new(dynamicLights);
        
        // 如果没有动态光源，使用手动配置的光源
        if (allLights.Count == 0)
        {
            for (int i = 0; i < lights.Length && allLights.Count < 8; i++)
            {
                if (lights[i] != null)
                {
                    // 创建临时 LightItem 来存储数据
                    LightItem tempLight = lights[i].GetComponent<LightItem>();
                    if (tempLight != null)
                    {
                        allLights.Add(tempLight);
                    }
                    else
                    {
                        // 从手动配置创建
                        tempLight = lights[i].gameObject.AddComponent<LightItem>();
                        tempLight.lightRadius = radii[i];
                        tempLight.lightIntensity = intensities[i];
                        tempLight.lightColor = colors[i];
                        allLights.Add(tempLight);
                    }
                }
            }
        }
        
        int count = Mathf.Min(allLights.Count, 8);

        Vector4[] positions = new Vector4[count];
        Vector4[] colorArray = new Vector4[count];
        float[] radiusArray = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (allLights[i] == null) continue;
            
            LightItem light = allLights[i];
            positions[i] = light.transform.position;
            colorArray[i] = new Vector4(light.lightColor.r, light.lightColor.g, light.lightColor.b, light.lightIntensity);
            radiusArray[i] = light.lightRadius;
        }

        Shader.SetGlobalInt(LightCount, count);
        Shader.SetGlobalVectorArray(LightPosArray, positions);
        Shader.SetGlobalVectorArray(LightColorArray, colorArray);
        Shader.SetGlobalFloatArray(LightRadiusArray, radiusArray);
    }
}