using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Settings;

public class DestroyIfTrue : MonoBehaviour
{
    [SerializeField]
    SaveSpecificSettings settings;

    [SerializeField]
    string boolField;

    private void Awake()
    {
        if (settings.TryGetFieldValue<bool>(boolField, out var val) && val)
        {
            Destroy(gameObject);
        }
    }
}
