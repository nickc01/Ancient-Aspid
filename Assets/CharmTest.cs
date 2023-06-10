using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmTest : MonoBehaviour
{
    private void Start()
    {
        if (CharmIconList.Instance != null)
        {
            GetComponent<SpriteRenderer>().sprite = CharmIconList.Instance.GetSprite(41);
        }
    }
}
