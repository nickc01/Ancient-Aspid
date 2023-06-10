using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorSlider : MonoBehaviour
{
    public UnityEvent<float> onSliderUpdate;

    [SerializeField]
    TextMeshProUGUI numberText;

    Slider slider;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(v =>
        {
            onSliderUpdate.Invoke(v);
            numberText.text = v.ToString();
        });
    }

    public void SetSliderValue(float value)
    {
        slider.value = value;
    }

    public float GetSliderValue()
    {
        return slider.value;
    }
}
