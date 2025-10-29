using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class Flash : MonoBehaviour
{
    private static float FlashDuration => 0.2f; // 闪烁持续时间
    private static float FadeOutDuration => 0.3f; // 淡出持续时间
    
    private static Canvas flashCanvas;
    private static Image flashPanel;
    private static Coroutine currentFlashCoroutine;
    private static MonoBehaviour coroutineRunner;

    /// <summary>
    /// 闪烁
    /// </summary>
    [YarnCommand("flash")]
    public void FlashScreen()
    {
        if (flashCanvas == null) CreateFlashUI();
        if (currentFlashCoroutine != null) coroutineRunner.StopCoroutine(currentFlashCoroutine);
        currentFlashCoroutine = coroutineRunner.StartCoroutine(FlashCoroutine());
    }

    private static void CreateFlashUI()
    {
        // 创建Canvas
        GameObject canvasObj = new("FlashCanvas");
        flashCanvas = canvasObj.AddComponent<Canvas>();
        flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        flashCanvas.sortingOrder = 10;
        
        // 添加Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // 添加GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建全屏白色面板
        GameObject panelObj = new GameObject("FlashPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        flashPanel = panelObj.AddComponent<Image>();
        flashPanel.color = Color.white;
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
        flashPanel.color = new Color(1, 1, 1, 0);
    }

    private static IEnumerator FlashCoroutine()
    {        
        // 快速变为完全白色
        flashPanel.color = new Color(1, 1, 1, 1); // 立即变为白色
        yield return new WaitForSeconds(FlashDuration);
        
        // 淡出回透明
        float elapsedTime = 0f;
        while (elapsedTime < FadeOutDuration)
        {            
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeOutDuration);
            flashPanel.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        // 确保完全透明
        flashPanel.color = new Color(1, 1, 1, 0);
        currentFlashCoroutine = null;
    }
}
