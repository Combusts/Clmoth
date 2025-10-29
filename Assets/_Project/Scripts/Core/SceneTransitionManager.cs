using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    public event System.Action OnSceneLoaded; // 场景加载完成后触发(还处于黑屏)

    public event System.Action OnSceneLoadComplete; // 场景加载完成后触发(黑屏已关闭)
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithFade(int sceneIndex)
    {
        StartCoroutine(FadeAndLoadScene(sceneIndex));
    }

    private System.Collections.IEnumerator FadeAndLoadScene(int sceneIndex)
    {

        // 渐变到黑屏
        yield return StartCoroutine(FadeToBlack());

        // 隐藏所有UI
        UIManager.Instance.HideAllUI();

        // 加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 初始化新场景中的Level脚本
        Level newLevel = FindObjectOfType<Level>();
        if (newLevel != null)
        {
            newLevel.InitializeLevel();
        }
        
        // 触发场景加载事件
        OnSceneLoaded?.Invoke();

        OnSceneLoaded = null;

        // 渐变关闭黑屏
        yield return StartCoroutine(FadeFromBlack());
        
        OnSceneLoadComplete?.Invoke();
        OnSceneLoadComplete = null;
    }

    private System.Collections.IEnumerator FadeToBlack()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private System.Collections.IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}
