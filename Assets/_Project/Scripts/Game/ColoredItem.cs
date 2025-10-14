using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetColor
{
    Red,
    Green,
    Blue,
}
public class ColoredItem : MonoBehaviour
{
    [SerializeField]
    public TargetColor color = TargetColor.Red;
    
    protected SpriteRenderer spriteRenderer;
    protected void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    protected void Start()
    {
        spriteRenderer.color = GetColor(color);
    }

    public static Color GetColor(TargetColor newColor) 
    {
        return newColor switch
        {
            TargetColor.Red => Color.red,
            TargetColor.Green => Color.green,
            TargetColor.Blue => Color.blue,
            _ => Color.white,
        };
    }

    public static TargetColor GetRandomEnumColor()
    {
        return (TargetColor)Random.Range(0, System.Enum.GetValues(typeof(TargetColor)).Length);
    }
}
