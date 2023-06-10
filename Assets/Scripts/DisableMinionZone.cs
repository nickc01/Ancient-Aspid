using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisableMinionZone : MonoBehaviour
{
    [SerializeField]
    List<GameObject> targetsToDisable = new List<GameObject>();

    [NonSerialized]
    List<GameObject> enemyObjects;

    //[NonSerialized]
    //List<int> oldLayers;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Hero Detector");
    }

    IEnumerable<GameObject> GetAllObjectsWithLayer(GameObject obj, int layer)
    {
        if (obj == null)
        {
            yield break;
        }

        if (obj.layer == layer)
        {
            yield return obj;
        }

        var childCount = obj.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            foreach (var childObj in GetAllObjectsWithLayer(obj.transform.GetChild(i).gameObject, layer))
            {
                yield return childObj;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<HeroController>() != null)
        {
            /*if (oldLayers == null)
            {
                oldLayers = new List<int>();
            }
            else
            {
                oldLayers.Clear();
            }*/

            if (enemyObjects == null)
            {
                enemyObjects = new List<GameObject>();
            }
            else
            {
                enemyObjects.Clear();
            }

            var attackLayer = LayerMask.NameToLayer("Attack");

            enemyObjects.AddRange(targetsToDisable.SelectMany(t => GetAllObjectsWithLayer(t, LayerMask.NameToLayer("Enemies"))));

            foreach (var target in enemyObjects)
            {
                target.gameObject.layer = attackLayer;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<HeroController>() != null)
        {
            var enemyLayer = LayerMask.NameToLayer("Enemies");

            for (int i = 0; i < enemyObjects.Count; i++)
            {
                enemyObjects[i].layer = enemyLayer;
            }
        }
    }
}
