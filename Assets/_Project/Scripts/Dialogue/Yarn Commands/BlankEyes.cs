using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class BlankEyes : MonoBehaviour
{
    private static float FadeOutDuration => 0.5f;
    
    private static Canvas blinkCanvas;
    private static Image blinkPanel;
    private static Coroutine currentBlinkCoroutine;
    private static MonoBehaviour coroutineRunner;

    [YarnCommand("sleepy")]
    public static void SleepyCommand()
    {
        if (blinkCanvas == null) CreateBlinkUI();
        blinkPanel.color = new Color(0, 0, 0, 1); // Instant black
    }

    [YarnCommand("wake_up")]
    public static void WakeUpCommand()
    {
        if (blinkCanvas == null) CreateBlinkUI();
        if (currentBlinkCoroutine != null) coroutineRunner.StopCoroutine(currentBlinkCoroutine);
        currentBlinkCoroutine = coroutineRunner.StartCoroutine(WakeUpCoroutine());
    }

    private static void CreateBlinkUI()
    {
        // Create Canvas
        GameObject canvasObj = new("BlinkCanvas");
        blinkCanvas = canvasObj.AddComponent<Canvas>();
        blinkCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blinkCanvas.sortingOrder = 9;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create full-screen black panel
        GameObject panelObj = new GameObject("BlinkPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        blinkPanel = panelObj.AddComponent<Image>();
        blinkPanel.color = Color.black;
        blinkPanel.raycastTarget = false;
        
        // Set RectTransform to full screen
        RectTransform rectTransform = blinkPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Create coroutine runner
        coroutineRunner = canvasObj.AddComponent<BlankEyes>();
        
        // Make canvas persistent across scenes
        DontDestroyOnLoad(canvasObj);
        
        // Initially hide the panel
        blinkPanel.color = new Color(0, 0, 0, 0);
    }

    private static IEnumerator WakeUpCoroutine()
    {
        // Fade out from black to transparent (1 → 0)
        float elapsedTime = 0f;
        while (elapsedTime < FadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeOutDuration);
            blinkPanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // Ensure we're at full transparency
        blinkPanel.color = new Color(0, 0, 0, 0);
        
        currentBlinkCoroutine = null;
    }
}
