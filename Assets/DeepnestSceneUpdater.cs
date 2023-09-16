using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepnestSceneUpdater : MonoBehaviour
{
    void Start()
    {
        var boneSprite = GameObject.Find("bone_deep_0122_b (1)");

        if (boneSprite != null)
        {
            boneSprite.transform.position -= new Vector3(0f,0.25f);
        }
    }
}
