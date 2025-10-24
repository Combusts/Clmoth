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
            leftIllustration.GetComponent<Image>().sprite = illustrations.Find(x => x.name == name)?.sprite;
            leftIllustration.SetActive(true);
        }
        else
        {
            rightIllustration.GetComponent<Image>().sprite = illustrations.Find(x => x.name == name)?.sprite;
            rightIllustration.SetActive(true);
        }
    }

    public void HideIllustration()
    {
        leftIllustration.SetActive(false);
        rightIllustration.SetActive(false);
    }
}
