using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 单例
    public static UIManager Instance { get; private set; }
    // 所有UIprefab路径字典
    private Dictionary<string, string> uiPathDic = new Dictionary<string, string>();

    // 所有实例化出来的UI
    private Dictionary<string, PanelBase> uiDic = new Dictionary<string, PanelBase>();

    // 打开的UI
    private List<PanelBase> openPanels = new List<PanelBase>();

    // 根节点
    [SerializeField] public GameObject uiRoot;

    [SerializeField] private Image fadeImage;

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化UI路径字典
        uiPathDic["Main"] = "Prefabs/UI/UIMain";
        uiPathDic["Setting"] = "Prefabs/UI/UISetting";
        uiPathDic["Playing"] = "Prefabs/UI/UIPlaying";
        uiPathDic["Pause"] = "Prefabs/UI/UIPause";
        uiPathDic["CinematicBars"] = "Prefabs/UI/UICinematicBars";
        uiPathDic["MiniGameWin"] = "Prefabs/UI/UIGameWin";
        uiPathDic["MiniGameOver"] = "Prefabs/UI/UIGameOver";
        uiPathDic["Password"] = "Prefabs/UI/UIPassword";


    }

    void Start()
    {
        GameManager.Instance.OnStartGame += () =>
        {
            // HideAllUI();
            // ShowUI("CinematicBars");
            // ShowUI("Playing");
        };
    }

    public void ShowUI(string uiName)
    {
        if (uiPathDic.ContainsKey(uiName) && !uiDic.ContainsKey(uiName))
        {
            GameObject ui = Resources.Load<GameObject>(uiPathDic[uiName]);

            if (ui == null)
            {
                Debug.LogError(uiName + "的UI资源为空");
            }
            
            GameObject uiInstance = Instantiate(ui, uiRoot.transform);
            uiInstance.name = uiName;
            uiDic.Add(uiName, uiInstance.GetComponent<PanelBase>());
            openPanels.Add(uiInstance.GetComponent<PanelBase>());

        } else if (uiDic.ContainsKey(uiName)) {
            uiDic[uiName].Show();
            openPanels.Add(uiDic[uiName]);
        } else {
            Debug.LogError("UI " + uiName + " not found");
        }
    }

    public void HideUI(string uiName)
    {
        if (uiDic.ContainsKey(uiName))
        {
            uiDic[uiName].Hide();
            openPanels.Remove(uiDic[uiName]);
        } else {
            Debug.LogError("UI " + uiName + " not found");
        }
    }

    public void HideAllUI()
    {
        foreach (var panel in openPanels)
        {
            panel.Hide();
        }
        openPanels.Clear();
    }

    public void HideLastUI()
    {
        if (openPanels.Count > 0)
        {
            PanelBase panel = openPanels[openPanels.Count - 1];
            panel.Hide();
            openPanels.Remove(panel);
        }
    }   

    public bool IsLastUI(string uiName)
    {
        if (openPanels.Count == 0) return false;
        return openPanels[openPanels.Count - 1].gameObject.name == uiName;
    }

    public bool IsUIOpen(string uiName)
    {
        if(uiDic.ContainsKey(uiName))
        {
            return openPanels.Contains(uiDic[uiName]);
        }
        return false;
    }

    public PanelBase GetPanelByName(string uiName)
    {
        if (uiDic.ContainsKey(uiName))
        {
            return uiDic[uiName];
        }
        Debug.LogError($"[UIManager] 没有找到对应UI:{uiName}");
        return null;
    }

    public void DoInDark(Action action, float fadeDuration = 0.5f, float fadeTransTime = 0.5f)
    {
        StartCoroutine(DoInDarkCoroutine(action, fadeTransTime, fadeDuration));
    }

    System.Collections.IEnumerator DoInDarkCoroutine(Action action, float fadeTransTime, float fadeDuration)
    {
        yield return StartCoroutine(FadeToBlack(fadeTransTime));
        action?.Invoke();
        yield return new WaitForSeconds(fadeDuration);
        yield return StartCoroutine(FadeFromBlack(fadeTransTime));
    }

    private System.Collections.IEnumerator FadeToBlack(float fadeTransTime)
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeTransTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeTransTime);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private System.Collections.IEnumerator FadeFromBlack(float fadeTransTime)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeTransTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeTransTime);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}
