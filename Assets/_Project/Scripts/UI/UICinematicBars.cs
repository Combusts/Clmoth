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

        // 将mainCamera的Transform PostionY向上偏移 BottomBlackBar的高度
        mainCamera.transform.position = new Vector3(0, 0 - bottomBarHeight*10, -10);

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
}
