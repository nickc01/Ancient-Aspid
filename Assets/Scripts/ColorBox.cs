using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorBox : MonoBehaviour
{
    public UnityEvent<bool> onValueChanged;


    Toggle toggle;

    private void Awake()
    {
        toggle = GetComponentInChildren<Toggle>();

        toggle.onValueChanged.AddListener(v =>
        {
            onValueChanged.Invoke(v);
        });
    }

    public void SetValue(bool ticked)
    {
        toggle.isOn = ticked;
    }

    public bool GetValue()
    {
        return toggle.isOn;
    }
}
