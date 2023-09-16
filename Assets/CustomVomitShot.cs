/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Interfaces;

public class CustomVomitShot : MonoBehaviour, IOnPool
{
    VomitGlob glob;

    [SerializeField]
    ParticleSystem sprayParticles;

    private void Awake()
    {
        glob = GetComponent<VomitGlob>();
        glob.onLand.AddListener(() =>
        {
            Debug.Log("VOMIT GLOB LANDED");
        });

        glob.onDisappear.AddListener(() =>
        {
            Debug.Log("ON DISSAPEAR");
        });
    }

    void IOnPool.OnPool()
    {
        
    }
}*/
