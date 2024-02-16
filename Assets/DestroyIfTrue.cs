using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;

public class DestroyIfTrue : MonoBehaviour
{
    [SerializeField]
    SaveSpecificSettings settings;

    [SerializeField]
    [SaveSpecificFieldName(typeof(bool), nameof(settings))]
    string boolField;

    private void Start()
    {
        if (settings.TryGetFieldValue<bool>(boolField, out var val) && val)
        {
            Destroy(gameObject);
        }
    }
}
