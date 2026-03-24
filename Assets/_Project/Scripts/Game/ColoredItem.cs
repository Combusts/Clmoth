using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetColor
{
    Red,
    Yellow,
    Blue,
}
public class ColoredItem : MonoBehaviour
{
    [Header("颜色属性")]
    [SerializeField]
    public TargetColor color = TargetColor.Red;

    [Header("颜色图标")]
    [SerializeField]
    private GameObject redIcon;
    [SerializeField]
    private GameObject yellowIcon;
    [SerializeField]
    private GameObject blueIcon;
    
    protected SpriteRenderer spriteRenderer;

    protected void Start()
    {
        switch (color)
        {
            case TargetColor.Red:
                redIcon.SetActive(true);
                break;
            case TargetColor.Yellow:
                  yellowIcon.SetActive(true);
                break;
            case TargetColor.Blue:
                blueIcon.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SwitchToColor(TargetColor newColor)
    {
        color = newColor;
        switch (color)
        {
            case TargetColor.Red:
                redIcon.SetActive(true);
                yellowIcon.SetActive(false);
                blueIcon.SetActive(false);
                break;
            case TargetColor.Yellow:
                redIcon.SetActive(false);
                yellowIcon.SetActive(true);
                blueIcon.SetActive(false);
                break;
            case TargetColor.Blue:
                redIcon.SetActive(false);
                yellowIcon.SetActive(false);
                blueIcon.SetActive(true);
                break;
            default:
                break;
        }
    }

    public static TargetColor GetRandomEnumColor()
    {
        return (TargetColor)Random.Range(0, System.Enum.GetValues(typeof(TargetColor)).Length);
    }
}
