using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DialoguePositionCommands : MonoBehaviour
{
    // 存储原始位置信息的静态变量
    private static readonly Dictionary<RectTransform, Vector2> originalAnchorMin = new();
    private static readonly Dictionary<RectTransform, Vector2> originalAnchorMax = new();
    private static readonly Dictionary<RectTransform, Vector2> originalAnchoredPosition = new();
    private static readonly Dictionary<RectTransform, Vector2> originalSizeDelta = new();
    
    // 居中位置的配置
    private static readonly Vector2 CENTER_ANCHOR_MIN = new(0f, 0.5f);
    private static readonly Vector2 CENTER_ANCHOR_MAX = new(1f, 0.5f);

    /// <summary>
    /// 将对话框移至屏幕中央
    /// </summary>
    /// <param name="height">可选参数，指定对话框高度。如果不指定则自动获取</param>
    [YarnCommand("center_dialogue")]
    public static void CenterDialogueCommand(float height = -1f)
    {
        var dialogueRunner = GetDialogueRunner();
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialoguePositionCommands] DialogueRunner not found!");
            return;
        }

        // 获取所有对话视图
        var dialogueViews = GetDialogueViews(dialogueRunner);
        if (dialogueViews.Count == 0)
        {
            Debug.LogWarning("[DialoguePositionCommands] No dialogue views found!");
            return;
        }

        Debug.Log($"[DialoguePositionCommands] Centering {dialogueViews.Count} dialogue views");

        foreach (var view in dialogueViews)
        {
            if (!view.TryGetComponent<RectTransform>(out var rectTransform)) continue;

            // 如果是第一次操作，保存原始位置
            if (!originalAnchorMin.ContainsKey(rectTransform))
            {
                SaveOriginalPosition(rectTransform);
            }

            // 移动到中央位置
            MoveToCenter(rectTransform, height);
        }
    }

    /// <summary>
    /// 恢复对话框到原始位置（屏幕底部）
    /// </summary>
    [YarnCommand("restore_dialogue")]
    public static void RestoreDialogueCommand()
    {
        var dialogueRunner = GetDialogueRunner();
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialoguePositionCommands] DialogueRunner not found!");
            return;
        }

        // 获取所有对话视图
        var dialogueViews = GetDialogueViews(dialogueRunner);
        if (dialogueViews.Count == 0)
        {
            Debug.LogWarning("[DialoguePositionCommands] No dialogue views found!");
            return;
        }

        Debug.Log($"[DialoguePositionCommands] Restoring {dialogueViews.Count} dialogue views");

        foreach (var view in dialogueViews)
        {
            if (!view.TryGetComponent<RectTransform>(out var rectTransform)) continue;

            // 恢复原始位置
            RestoreOriginalPosition(rectTransform);
        }
    }

    /// <summary>
    /// 获取DialogueRunner实例
    /// </summary>
    private static DialogueRunner GetDialogueRunner()
    {
        // 首先尝试通过YarnSpinnerManager获取
        if (YarnSpinnerManager.Instance != null)
        {
            var manager = YarnSpinnerManager.Instance;
            var dialogueRunnerField = typeof(YarnSpinnerManager).GetField("dialogueRunner", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (dialogueRunnerField != null)
            {
                return dialogueRunnerField.GetValue(manager) as DialogueRunner;
            }
        }

        // 如果无法通过Manager获取，直接查找场景中的DialogueRunner
        return FindObjectOfType<DialogueRunner>();
    }

    /// <summary>
    /// 获取所有对话视图（Line Presenter 和 Option Presenter）
    /// </summary>
    private static List<GameObject> GetDialogueViews(DialogueRunner dialogueRunner)
    {
        var dialogueViews = new List<GameObject>();

        // 通过反射获取dialoguePresenters字段
        var dialoguePresentersField = typeof(DialogueRunner).GetField("dialoguePresenters", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (dialoguePresentersField != null)
        {
            if (dialoguePresentersField.GetValue(dialogueRunner) is System.Collections.IList presenters)
            {
                foreach (var presenter in presenters)
                {
                    if (presenter is DialoguePresenterBase dialoguePresenter && dialoguePresenter != null)
                    {
                        dialogueViews.Add(dialoguePresenter.gameObject);
                    }
                }
            }
        }

        // 如果无法通过反射获取，尝试查找常见的对话UI组件
        if (dialogueViews.Count == 0)
        {
            // 查找包含"Dialogue"、"Line"、"Option"等关键词的UI对象
            var allUIObjects = FindObjectsOfType<RectTransform>();
            foreach (var rectTransform in allUIObjects)
            {
                var objName = rectTransform.name.ToLower();
                if (objName.Contains("dialogue") || objName.Contains("line") || objName.Contains("option"))
                {
                    dialogueViews.Add(rectTransform.gameObject);
                }
            }
        }

        return dialogueViews;
    }

    /// <summary>
    /// 保存原始位置信息
    /// </summary>
    private static void SaveOriginalPosition(RectTransform rectTransform)
    {
        originalAnchorMin[rectTransform] = rectTransform.anchorMin;
        originalAnchorMax[rectTransform] = rectTransform.anchorMax;
        originalAnchoredPosition[rectTransform] = rectTransform.anchoredPosition;
        originalSizeDelta[rectTransform] = rectTransform.sizeDelta;
    }

    /// <summary>
    /// 移动到中央位置
    /// </summary>
    /// <param name="rectTransform">要移动的RectTransform</param>
    /// <param name="height">指定高度，-1表示自动获取</param>
    private static void MoveToCenter(RectTransform rectTransform, float height = -1f)
    {
        float realHeight;
        
        if (height > 0)
        {
            // 使用指定的高度
            realHeight = height;
            Debug.Log($"[DialoguePositionCommands] Using specified height: {realHeight}");
        }
        else
        {
            // 自动获取高度
            realHeight = GetRealHeight(rectTransform);
            
            // 如果无法获取有效高度，使用备用方案
            if (realHeight <= 0)
            {
                Debug.LogWarning($"[DialoguePositionCommands] Could not get valid height for {rectTransform.name}, using fallback positioning");
                realHeight = GetFallbackHeight(rectTransform);
            }
        }
        
        // 设置居中位置
        rectTransform.anchorMin = CENTER_ANCHOR_MIN;
        rectTransform.anchorMax = CENTER_ANCHOR_MAX;
        
        // PositionY设置为负的对象高度的一半
        float positionY = -realHeight * 0.5f;
        rectTransform.anchoredPosition = new Vector2(0f, positionY);
        
        Debug.Log($"[DialoguePositionCommands] Moved to center - Height: {realHeight}, FinalPosition: {rectTransform.anchoredPosition}");
    }


    /// <summary>
    /// 获取RectTransform的真实高度
    /// </summary>
    private static float GetRealHeight(RectTransform rectTransform)
    {
        // 方法1: 尝试直接获取rect.height
        float height1 = rectTransform.rect.height;
        if (height1 > 0)
        {
            Debug.Log($"[DialoguePositionCommands] Method 1 - Direct rect.height: {height1}");
            return height1;
        }

        // 方法2: 临时设置为stretch模式获取高度
        var originalAnchorMin = rectTransform.anchorMin;
        var originalAnchorMax = rectTransform.anchorMax;
        var originalAnchoredPosition = rectTransform.anchoredPosition;
        var originalSizeDelta = rectTransform.sizeDelta;
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        
        float height2 = rectTransform.rect.height;
        
        // 恢复原始状态
        rectTransform.anchorMin = originalAnchorMin;
        rectTransform.anchorMax = originalAnchorMax;
        rectTransform.anchoredPosition = originalAnchoredPosition;
        rectTransform.sizeDelta = originalSizeDelta;
        
        if (height2 > 0)
        {
            Debug.Log($"[DialoguePositionCommands] Method 2 - Stretch mode height: {height2}");
            return height2;
        }

        // 方法3: 使用LayoutElement或ContentSizeFitter
        var layoutElement = rectTransform.GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null && layoutElement.preferredHeight > 0)
        {
            Debug.Log($"[DialoguePositionCommands] Method 3 - LayoutElement preferredHeight: {layoutElement.preferredHeight}");
            return layoutElement.preferredHeight;
        }

        Debug.LogWarning($"[DialoguePositionCommands] All methods failed to get valid height");
        return 0;
    }

    /// <summary>
    /// 获取备用高度（当无法获取真实高度时使用）
    /// </summary>
    private static float GetFallbackHeight(RectTransform rectTransform)
    {
        // 根据Canvas的缩放因子和屏幕高度计算备用高度
        var canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            float canvasHeight = canvas.pixelRect.height;
            float scaleFactor = canvas.scaleFactor;
            float fallbackHeight = canvasHeight / scaleFactor * 0.2f; // 使用屏幕高度的20%作为备用高度
            Debug.Log($"[DialoguePositionCommands] Fallback height: {fallbackHeight} (Canvas height: {canvasHeight}, Scale: {scaleFactor})");
            return fallbackHeight;
        }
        
        // 如果连Canvas都找不到，使用默认值
        float defaultHeight = 200f;
        Debug.Log($"[DialoguePositionCommands] Using default fallback height: {defaultHeight}");
        return defaultHeight;
    }

    /// <summary>
    /// 恢复原始位置
    /// </summary>
    private static void RestoreOriginalPosition(RectTransform rectTransform)
    {
        if (originalAnchorMin.ContainsKey(rectTransform))
        {
            rectTransform.anchorMin = originalAnchorMin[rectTransform];
            rectTransform.anchorMax = originalAnchorMax[rectTransform];
            rectTransform.anchoredPosition = originalAnchoredPosition[rectTransform];
            rectTransform.sizeDelta = originalSizeDelta[rectTransform];
        }
    }

    /// <summary>
    /// 将对话框移动到屏幕顶部
    /// </summary>
    [YarnCommand("move_dialogue_top")]
    public static void MoveDialogueToTopCommand()
    {
        var dialogueRunner = GetDialogueRunner();
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialoguePositionCommands] DialogueRunner not found!");
            return;
        }

        var dialogueViews = GetDialogueViews(dialogueRunner);
        if (dialogueViews.Count == 0)
        {
            Debug.LogWarning("[DialoguePositionCommands] No dialogue views found!");
            return;
        }

        Debug.Log($"[DialoguePositionCommands] Moving {dialogueViews.Count} dialogue views to top");

        foreach (var view in dialogueViews)
        {
            if (!view.TryGetComponent<RectTransform>(out var rectTransform)) continue;

            if (!originalAnchorMin.ContainsKey(rectTransform))
            {
                SaveOriginalPosition(rectTransform);
            }

            MoveToTop(rectTransform);
        }
    }

    /// <summary>
    /// 将对话框移动到屏幕底部
    /// </summary>
    [YarnCommand("move_dialogue_bottom")]
    public static void MoveDialogueToBottomCommand()
    {
        var dialogueRunner = GetDialogueRunner();
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialoguePositionCommands] DialogueRunner not found!");
            return;
        }

        var dialogueViews = GetDialogueViews(dialogueRunner);
        if (dialogueViews.Count == 0)
        {
            Debug.LogWarning("[DialoguePositionCommands] No dialogue views found!");
            return;
        }

        Debug.Log($"[DialoguePositionCommands] Moving {dialogueViews.Count} dialogue views to bottom");

        foreach (var view in dialogueViews)
        {
            if (!view.TryGetComponent<RectTransform>(out var rectTransform)) continue;

            if (!originalAnchorMin.ContainsKey(rectTransform))
            {
                SaveOriginalPosition(rectTransform);
            }

            MoveToBottom(rectTransform);
        }
    }

    /// <summary>
    /// 移动到顶部位置
    /// </summary>
    private static void MoveToTop(RectTransform rectTransform)
    {
        float realHeight = GetRealHeight(rectTransform);
        if (realHeight <= 0)
        {
            Debug.LogWarning($"[DialoguePositionCommands] Could not get valid height for {rectTransform.name}, using fallback positioning");
            realHeight = GetFallbackHeight(rectTransform);
        }

        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -realHeight * 0.5f);

        Debug.Log($"[DialoguePositionCommands] Moved to top - Height: {realHeight}, Position: {rectTransform.anchoredPosition}");
    }

    /// <summary>
    /// 移动到底部位置
    /// </summary>
    private static void MoveToBottom(RectTransform rectTransform)
    {
        float realHeight = GetRealHeight(rectTransform);
        if (realHeight <= 0)
        {
            Debug.LogWarning($"[DialoguePositionCommands] Could not get valid height for {rectTransform.name}, using fallback positioning");
            realHeight = GetFallbackHeight(rectTransform);
        }

        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, realHeight * 0.5f);

        Debug.Log($"[DialoguePositionCommands] Moved to bottom - Height: {realHeight}, Position: {rectTransform.anchoredPosition}");
    }

    /// <summary>
    /// 清理存储的原始位置信息（可选，用于重置状态）
    /// </summary>
    [YarnCommand("reset_dialogue_position")]
    public static void ResetDialoguePositionCommand()
    {
        originalAnchorMin.Clear();
        originalAnchorMax.Clear();
        originalAnchoredPosition.Clear();
        originalSizeDelta.Clear();
        Debug.Log("[DialoguePositionCommands] Reset dialogue position data");
    }
}
