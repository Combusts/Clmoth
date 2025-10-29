using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[System.Serializable]
public class Illustration
{
    public string name;
    public Sprite sprite;
}

public class IllustrationController : MonoBehaviour
{
    [Header("Illustration")]
    [SerializeField] private List<Illustration> illustrations;

    private GameObject leftIllustration;
    private GameObject rightIllustration;

    void Start()
    {
        leftIllustration = transform.Find("Left Illustration").gameObject;
        rightIllustration = transform.Find("Right Illustration").gameObject;
        leftIllustration.SetActive(false);
        rightIllustration.SetActive(false);
    }

    public void ShowIllustration(string name, bool isLeft)
    {
        Debug.Log("ShowIllustration: " + name + " " + isLeft);
        if (isLeft)
        {
            // 将左边Illustration的颜色变亮
            leftIllustration.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            leftIllustration.GetComponent<Image>().sprite = illustrations.Find(x => x.name == name)?.sprite;
            leftIllustration.SetActive(true);
            // 将右边Illustration的颜色变灰
            rightIllustration.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        else
        {
            // 将右边Illustration的颜色变亮
            rightIllustration.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            rightIllustration.GetComponent<Image>().sprite = illustrations.Find(x => x.name == name)?.sprite;
            rightIllustration.SetActive(true);
            // 将左边Illustration的颜色变灰
            leftIllustration.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }

    public void HideIllustration()
    {
        leftIllustration.SetActive(false);
        rightIllustration.SetActive(false);
    }
}
