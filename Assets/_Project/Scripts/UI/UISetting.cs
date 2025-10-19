using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : PanelBase
{
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private GameObject BGMSliderText;
    [SerializeField] private GameObject SFXSliderText;

    private void OnEnable()
    {
        PlayerInputManager.Instance.OnEseActionPerformed += OnEseActionPerformed;
    }

    private void OnDisable()
    {
        PlayerInputManager.Instance.OnEseActionPerformed -= OnEseActionPerformed;
    }

    private void OnEseActionPerformed()
    {
        if (UIManager.Instance.IsLastUI("Setting"))
        {
            UIManager.Instance.HideUI("Setting");
        }
    }
    
    public void OnClickBackButton()
    {
        UIManager.Instance.HideUI("Setting");
    }

    public void OnClickBGMSlider()
    {
        Debug.Log($"OnClickBGMSlider: {BGMSlider.value}");
        AudioManager.Instance.SetVolume("bgmVolume", BGMSlider.value);
        BGMSliderText.GetComponent<TextMeshProUGUI>().text = $"{BGMSlider.value / 4 * 5 * 100:F0}";
    }

    public void OnClickSFXSlider()
    {
        Debug.Log($"OnClickSFXSlider: {SFXSlider.value}");
        AudioManager.Instance.SetVolume("fsxVolume", SFXSlider.value);
        SFXSliderText.GetComponent<TextMeshProUGUI>().text = $"{SFXSlider.value /4 * 5 * 100:F0}";
    }
}
