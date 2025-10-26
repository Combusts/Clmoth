using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPassword : PanelBase
{
    public List<int> password = new(4);

    [SerializeField] private TextMeshProUGUI[] passwordNumTexts;

    public event Action OnPasswordCorrect;

    private int currentIndex = 0;



    override public void Show()
    {
        base.Show();
        ResetPasswordInput();
    }
    
    public void SetPassword(int num1, int num2, int num3, int num4)
    {
        this.password = new List<int> { num1, num2, num3, num4 };
    }

    public bool CheckPassword()
    {
        for (int i = 0; i < password.Count; i++)
        {
            if (passwordNumTexts[i].text != password[i].ToString()){
                // 密码错误
                return false;
            }
        }

        // 密码正确
        return true;
    }

    public void OnEnterPassword(int num)
    {
        // 输入密码
        passwordNumTexts[currentIndex].text = num.ToString();
        currentIndex++;

        // 判断是否输入完成
        if (currentIndex >= password.Count)
        {
            // 输入完成
            currentIndex = 0;
            if (CheckPassword())
            {
                // 密码正确
                Debug.Log("密码正确");
                OnPasswordCorrect?.Invoke();
            }
            else
            {
                // 密码错误
                Debug.Log("密码错误");
                ResetPasswordInput();
            }
        }
    }

    // 重置密码输入
    public void ResetPasswordInput()
    {
        for (int i = 0; i < passwordNumTexts.Length; i++)
        {
            passwordNumTexts[i].text = "-";
        }
        currentIndex = 0;
    }

    public void OnBackspace()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            passwordNumTexts[currentIndex].text = "-";
        }
    }
}
