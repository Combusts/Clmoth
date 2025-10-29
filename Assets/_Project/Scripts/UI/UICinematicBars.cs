using UnityEngine;

public class UICinematicBars : PanelBase
{
    [Header("黑边设置")]
    [Range(0f, 0.5f)]
    [SerializeField] private float topBarHeight = 0.2f;    // 上黑边占屏幕的百分比

    [Range(0f, 0.5f)]
    [SerializeField] private float bottomBarHeight = 0.3f; // 下黑边占屏幕的百分比

    private Camera mainCamera;
    private GameObject TopBlackBar;
    private GameObject BottomBlackBar;
    private RectTransform gameViewPort;

    void OnEnable()
    {
        mainCamera = Camera.main;

        TopBlackBar = transform.Find("TopBlackBar").gameObject;
        BottomBlackBar = transform.Find("BottomBlackBar").gameObject;
        TopBlackBar.SetActive(true);
        BottomBlackBar.SetActive(true);


        // 设置上黑边：anchor到顶部，offsetMin/offsetMax都设为0来平铺整个anchor范围
        RectTransform topRect = TopBlackBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1 - topBarHeight);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;

        // 设置下黑边：anchor到底部，offsetMin/offsetMax都设为0来平铺整个anchor范围
        RectTransform bottomRect = BottomBlackBar.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, bottomBarHeight);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;

        // 已取消相机位置更改
        // SetCameraPosition(); // 不再更改相机位置

        Debug.Log("mainCamera.transform.position: " + mainCamera.transform.position);

    }

    public void SetCameraPosition()
    {
        // 已取消相机位置更改，保留方法以避免编译错误
        // 相机位置不再被此系统修改
    }

    void OnDisable()
    {
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        TopBlackBar.SetActive(false);
        BottomBlackBar.SetActive(false);
    }

    /// <summary>
    /// 在Inspector中值改变时自动调用，用于实时更新黑边设置
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying && TopBlackBar != null && BottomBlackBar != null)
        {
            ReloadCinematicBars();
        }
    }

    /// <summary>
    /// 重新加载黑边设置，更新黑边位置和相机位置
    /// </summary>
    public void ReloadCinematicBars()
    {
        if (TopBlackBar == null || BottomBlackBar == null)
        {
            Debug.LogWarning("Black bars not found, cannot reload cinematic bars");
            return;
        }

        // 重新设置上黑边
        RectTransform topRect = TopBlackBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1 - topBarHeight);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;

        // 重新设置下黑边
        RectTransform bottomRect = BottomBlackBar.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, bottomBarHeight);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;

        // 已取消相机位置更改
        // SetCameraPosition(); // 不再更改相机位置

        Debug.Log($"Cinematic bars reloaded - Top: {topBarHeight:F2}, Bottom: {bottomBarHeight:F2}");
    }
}
