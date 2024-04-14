using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;

public class DestroyIfTrue : MonoBehaviour
{
    [SerializeField]
    SaveSpecificSettings settings;

    /*[SerializeField]
    [SaveSpecificFieldName(typeof(bool), nameof(settings))]
    string boolField;*/

    [SaveSpecificFieldName(typeof(bool), nameof(settings))]
    [SerializeField]
    List<string> boolNames = new List<string>();

    [SerializeField]
    List<bool> boolValues = new List<bool>();

    private void Start()
    {
        bool delete = true;
        int length = Mathf.Min(boolNames.Count, boolValues.Count);
        for (int i = 0; i < length; i++)
        {
            if (!(settings.TryGetFieldValue<bool>(boolNames[i], out var result) && result == boolValues[i]))
            {
                delete = false;
                break;
                //Destroy(gameObject);
                //return;
            }
        }

        if (delete)
        {
            Destroy(gameObject);
        }
        /*if (settings.TryGetFieldValue<bool>(boolField, out var val) && val)
        {
            Destroy(gameObject);
        }*/
    }
}
