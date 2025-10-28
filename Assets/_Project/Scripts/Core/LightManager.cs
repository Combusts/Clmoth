using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LightManager2D : MonoBehaviour
{
    private static readonly int LightCount = Shader.PropertyToID("_LightCount");
    private static readonly int LightPosArray = Shader.PropertyToID("_LightPosArray");
    private static readonly int LightColorArray = Shader.PropertyToID("_LightColorArray");
    private static readonly int LightRadiusArray = Shader.PropertyToID("_LightRadiusArray");
    private static bool shaderArraysInitialized = false;

    // 保留旧的手动配置支持
    public Transform[] lights; // 光源物体
    public Color[] colors;     // 光颜色
    public float[] radii;      // 光半径
    public float[] intensities;// 光强度

    // 动态光源列表
    private readonly List<LightItem> dynamicLights = new();

    // 摄像机引用
    [SerializeField] private Camera mainCamera;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        InitializeShaderArrays();
    }

    void OnEnable()
    {
        // 确保在启用时也初始化
        InitializeShaderArrays();
    }

    private void InitializeShaderArrays()
    {
        if (shaderArraysInitialized && Application.isPlaying)
        {
            // 已经在运行中初始化过了，跳过
            return;
        }

        // 在启动时初始化固定大小的 shader 数组
        Vector4[] initPositions = new Vector4[8];
        Vector4[] initColorArray = new Vector4[8];
        float[] initRadiusArray = new float[8];

        Shader.SetGlobalInt(LightCount, 0);

        try
        {
            Shader.SetGlobalVectorArray(LightPosArray, initPositions);
            Shader.SetGlobalVectorArray(LightColorArray, initColorArray);
            Shader.SetGlobalFloatArray(LightRadiusArray, initRadiusArray);

            shaderArraysInitialized = true;
            Debug.Log("Shader arrays initialized to size 8.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to initialize shader arrays: {e.Message}. Please restart Unity.");
        }
    }

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
        if (mainCamera == null) return;

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

        // 如果没有任何光源，设置光源数量为0并返回
        if (allLights.Count == 0)
        {
            Shader.SetGlobalInt(LightCount, 0);
            return;
        }

        // 获取摄像机位置
        Vector3 cameraPos = mainCamera.transform.position;

        // 过滤null并排序，只取最近的8个（或更少）
        List<LightItem> sortedLights = allLights
            .Where(light => light != null)
            .OrderBy(light => Vector3.SqrMagnitude(light.transform.position - cameraPos))
            .Take(8)
            .ToList();

        int count = sortedLights.Count;

        // 始终创建固定大小的数组 (8) 以避免 shader 数组大小变化错误
        Vector4[] positions = new Vector4[8];
        Vector4[] colorArray = new Vector4[8];
        float[] radiusArray = new float[8];

        for (int i = 0; i < count; i++)
        {
            // 使用 sortedLights（已排序并过滤null的列表）
            LightItem light = sortedLights[i];
            positions[i] = light.transform.position;
            colorArray[i] = new Vector4(light.lightColor.r, light.lightColor.g, light.lightColor.b, light.lightIntensity);
            radiusArray[i] = light.lightRadius;
        }

        Shader.SetGlobalInt(LightCount, count);
        Shader.SetGlobalVectorArray(LightPosArray, positions);
        Shader.SetGlobalVectorArray(LightColorArray, colorArray);
        Shader.SetGlobalFloatArray(LightRadiusArray, radiusArray);
    }

    internal void UpdateLight(LightItem lightItem)
    {
        if (dynamicLights.Contains(lightItem))
        {
            dynamicLights.Remove(lightItem);
            dynamicLights.Add(lightItem);
        }
    }

    [ContextMenu("Reset Shader Arrays")]
    private void ResetShaderArrays()
    {
        Debug.Log("Resetting shader arrays to size 8...");
        shaderArraysInitialized = false;
        InitializeShaderArrays();
        Debug.Log("Shader arrays reset complete.");
    }
}