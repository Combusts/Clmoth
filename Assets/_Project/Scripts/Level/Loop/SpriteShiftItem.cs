using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteShiftItem : MonoBehaviour
{
    public SpriteVariant[] Variants;


    private void Awake()
    {
        for (int i = 0; i < Variants.Length; i++)
        {
            if (Variants[i].isDefault)
            {
                ShiftSprite(i);
                return;
            }
        }
    }

    public void ShiftSprite(int index)
    {
        if (index < 0 || index >= Variants.Length)
        {
            Debug.LogError($"SpriteShiftItem: Index {index} is out of range.");
            return;
        }
        foreach (var variant in Variants)
        {
            variant.gameObject.SetActive(false);
        }
        Variants[index].gameObject.SetActive(true);
    }

    public void ShiftSprite(string name)
    {
        for (int i = 0; i < Variants.Length; i++)
        {
            if (Variants[i].Name == name)
            {
                ShiftSprite(i);
                return;
            }
        }
        Debug.LogError($"SpriteShiftItem: Name {name} is not found.");
    }
}

[Serializable]
public class SpriteVariant
{
    public string Name;
    public GameObject gameObject;
    public bool isDefault = false;
}

