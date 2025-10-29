using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class Flash : MonoBehaviour
{
    private static float FlashDuration => 0.2f; // 闪烁持续时间
    private static float FadeOutDuration => 0.3f; // 淡出持续时间
    private static Color DefaultFlashColor = Color.white; // 默认闪烁颜色
    
    private static Canvas flashCanvas;
    private static Image flashPanel;
    private static Coroutine currentFlashCoroutine;
    private static MonoBehaviour coroutineRunner;

    /// <summary>
    /// 默认白色闪烁
    /// </summary>
    [YarnCommand("flash")]
    public void FlashScreen()
    {
        FlashScreenWithColor(DefaultFlashColor);
    }

    /// <summary>
    /// 指定颜色闪烁
    /// </summary>
    [YarnCommand("flash_color")]
    public void FlashScreenWithColorCommand(string colorName) // 支持通过颜色名设置，如 "red", "blue" 等
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(colorName, out color))
        {
            FlashScreenWithColor(color);
        }
        else if (colorName.StartsWith("#")) // 支持十六进制颜色，如 "#FF0000" 
        {
            if (ColorUtility.TryParseHtmlString(colorName, out color))
            {
                FlashScreenWithColor(color);
            }
            else
            {
                Debug.LogWarning($"Invalid color format: {colorName}, using white instead.");
                FlashScreenWithColor(DefaultFlashColor);
            }
        }
        else // 尝试通过颜色名解析
        {
            switch (colorName.ToLower())
            {
                case "red": color = Color.red; break;
                case "green": color = Color.green; break;
                case "blue": color = Color.blue; break;
                case "yellow": color = Color.yellow; break;
                case "cyan": color = Color.cyan; break;
                case "magenta": color = Color.magenta; break;
                case "black": color = Color.black; break;
                case "white": color = Color.white; break;
                case "gray": color = Color.gray; break;
                default:
                    Debug.LogWarning($"Unknown color name: {colorName}, using white instead.");
                    color = DefaultFlashColor;
                    break;
            }
            FlashScreenWithColor(color);
        }
    }

    /// <summary>
    /// 内部方法：使用指定颜色闪烁
    /// </summary>
    private void FlashScreenWithColor(Color color)
    {
        if (flashCanvas == null) CreateFlashUI();
        if (currentFlashCoroutine != null) coroutineRunner.StopCoroutine(currentFlashCoroutine);
        currentFlashCoroutine = coroutineRunner.StartCoroutine(FlashCoroutine(color));
    }

    private static void CreateFlashUI()
    {
        // 创建Canvas
        GameObject canvasObj = new("FlashCanvas");
        flashCanvas = canvasObj.AddComponent<Canvas>();
        flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        flashCanvas.sortingOrder = 20;
        
        // 添加Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // 添加GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建全屏面板
        GameObject panelObj = new GameObject("FlashPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        flashPanel = panelObj.AddComponent<Image>();
        flashPanel.color = DefaultFlashColor;
        flashPanel.raycastTarget = false;
        
        // 设置RectTransform为全屏
        RectTransform rectTransform = flashPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 创建协程运行器
        coroutineRunner = canvasObj.AddComponent<Flash>();
        
        // 使canvas在场景切换时保持
        DontDestroyOnLoad(canvasObj);
        
        // 初始隐藏面板
        flashPanel.color = new Color(DefaultFlashColor.r, DefaultFlashColor.g, DefaultFlashColor.b, 0);
    }

    private static IEnumerator FlashCoroutine(Color flashColor)
    {
        // 快速变为完全指定颜色
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);
        yield return new WaitForSeconds(FlashDuration);
        
        // 淡出回透明
        float elapsedTime = 0f;
        while (elapsedTime < FadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeOutDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }
        
        // 确保完全透明
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        currentFlashCoroutine = null;
    }
}
