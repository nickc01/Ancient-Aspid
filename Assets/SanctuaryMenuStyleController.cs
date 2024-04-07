using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanctuaryMenuStyleController : MonoBehaviour
{
    [SerializeField]
    Transform cloneContainer;

    private void OnDisable()
    {
        for (int i = 0; i < cloneContainer.childCount; i++)
        {
            var child = cloneContainer.GetChild(i);

            if (child.name.Contains("(Clone)"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}
