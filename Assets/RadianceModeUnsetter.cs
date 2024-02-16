using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;
using static BossStatue;

public class RadianceModeUnsetter : MonoBehaviour
{
    [SerializeField]
    SaveSpecificSettings settings;

    [SerializeField]
    [SaveSpecificFieldName(typeof(Completion), nameof(settings))]
    string completionFieldName;

    private void FixedUpdate()
    {
        if (settings.TryGetFieldValue(completionFieldName, out Completion completion) && completion.completedTier2 || completion.completedTier3)
        {
            completion.completedTier2 = false;
            completion.completedTier3 = false;
            settings.SetFieldValue(completionFieldName, completion);
        }
    }
}
