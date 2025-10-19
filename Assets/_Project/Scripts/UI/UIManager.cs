using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        uiPathDic["Main"] = "Assets/_Project/Prefabs/UI/UIMain.prefab";
        uiPathDic["Setting"] = "Assets/_Project/Prefabs/UI/UISetting.prefab";
        uiPathDic["Playing"] = "Assets/_Project/Prefabs/UI/UIPlaying.prefab";
        uiPathDic["Pause"] = "Assets/_Project/Prefabs/UI/UIPause.prefab";
        uiPathDic["CinematicBars"] = "Assets/_Project/Prefabs/UI/UICinematicBars.prefab";

    }

    void Start()
    {
        GameManager.Instance.OnStartGame += () =>
        {
            HideAllUI();
            ShowUI("CinematicBars");
            ShowUI("Playing");
        };
    }

    public void ShowUI(string uiName)
    {
        if (uiPathDic.ContainsKey(uiName) && !uiDic.ContainsKey(uiName))
        {
            GameObject ui = AssetDatabase.LoadAssetAtPath(uiPathDic[uiName], typeof(GameObject)) as GameObject;;

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
}
