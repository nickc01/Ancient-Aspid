using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Ancient Aspid Prefabs", menuName = "Ancient Aspid Prefabs")]
public class AncientAspidPrefabs : ScriptableObject
{
    public static AncientAspidPrefabs Instance { get; set; }

    public AspidShot AspidShotPrefab;
}
