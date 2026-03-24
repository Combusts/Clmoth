using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueBackgroundCommands : MonoBehaviour
{
    private static float FadeDuration => 0.1f;
    
    private static Canvas backgroundCanvas;
    private static Image backgroundImage;
    private static Coroutine currentFadeCoroutine;
    private static MonoBehaviour coroutineRunner;

    [YarnCommand("set_background")]
    public static void SetBackgroundCommand(string spritePath)
    {
        if (backgroundCanvas == null) CreateBackgroundUI();
        
        // 加载指定路径的精灵
        Sprite backgroundSprite = Resources.Load<Sprite>(spritePath);
        if (backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
        }
        else
        {
            Debug.LogWarning($"Background sprite not found at path: {spritePath}");
        }
    }

    [YarnCommand("show_background")]
    public static void ShowBackgroundCommand()
    {
        if (backgroundCanvas == null) CreateBackgroundUI();
        
        // 停止当前正在运行的协程
        if (currentFadeCoroutine != null)
        {
            coroutineRunner.StopCoroutine(currentFadeCoroutine);
        }
        
        // 启动淡入协程
        currentFadeCoroutine = coroutineRunner.StartCoroutine(FadeBackgroundCoroutine(0f, 1f));
    }

    [YarnCommand("hide_background")]
    public static void HideBackgroundCommand()
    {
        if (backgroundCanvas == null) CreateBackgroundUI();
        
        // 停止当前正在运行的协程
        if (currentFadeCoroutine != null)
        {
            coroutineRunner.StopCoroutine(currentFadeCoroutine);
        }
        
        // 启动淡出协程
        currentFadeCoroutine = coroutineRunner.StartCoroutine(FadeBackgroundCoroutine(1f, 0f));
    }

    private static void CreateBackgroundUI()
    {
        // 创建Canvas
        GameObject canvasObj = new("BackgroundCanvas");
        backgroundCanvas = canvasObj.AddComponent<Canvas>();
        backgroundCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        backgroundCanvas.sortingOrder = 9; // 确保背景在对话UI之后，但在前景效果之前
        
        // 添加Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // 添加GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建全屏背景图片
        GameObject imageObj = new GameObject("BackgroundImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        backgroundImage = imageObj.AddComponent<Image>();
        backgroundImage.raycastTarget = false;
        
        // 设置RectTransform为全屏
        RectTransform rectTransform = backgroundImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 创建协程运行器
        coroutineRunner = canvasObj.AddComponent<DialogueBackgroundCommands>();
        
        // 使Canvas在场景之间保持持久
        DontDestroyOnLoad(canvasObj);
        
        // 初始隐藏背景
        backgroundImage.color = new Color(1, 1, 1, 0);
    }

    private static IEnumerator FadeBackgroundCoroutine(float startAlpha, float endAlpha)
    {
        // 淡入或淡出背景图片
        float elapsedTime = 0f;
        Color startColor = backgroundImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
        
        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / FadeDuration);
            backgroundImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        // 确保达到目标透明度
        backgroundImage.color = endColor;
        currentFadeCoroutine = null;
    }
}
