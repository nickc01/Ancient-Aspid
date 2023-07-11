using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum CurveType
{
    Linear,
    Smooth
}

public class ColorMode : MonoBehaviour
{
    public UnityEvent<CurveType> onCurveUpdate;

    TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();

        dropdown.options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Linear"),
            new TMP_Dropdown.OptionData("Smooth")
        };

        dropdown.onValueChanged.AddListener(v =>
        {
            onCurveUpdate.Invoke((CurveType)v);
        });
    }

    public void SetValue(CurveType curve)
    {
        dropdown.value = (int)curve;
    }

    public CurveType GetValue()
    {
        return (CurveType)dropdown.value;
    }
}
